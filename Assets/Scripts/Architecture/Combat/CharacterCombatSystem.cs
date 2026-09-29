using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public interface IPreparedAttackExecutor
	{
		bool CanExecute(AttackExecutionContext context);
		float Cooldown(AttackExecutionContext context);
	}

	public interface ICharacterSequenceExecutor
	{
		bool Advance(CharacterActionState state, CharacterAttackParameterConfig config, GameObject owner);
	}
	public interface ITransientAttackSequence { }

	public class CharacterCombatSystem : AbstractSystem, IRunUpdateable
	{
		private class Effect
		{
			public GameObject Object;
			public float Age;
			public float Duration;
			public Color Color;
		}
		private readonly List<Effect> mEffects = new();
		private readonly Dictionary<string, IDodgeEffectExecutor> mDodges = new();
		private int mResetVersion;
		private GameObject mHeldWeapon;
		private GameObject mStatusGlow;
		private GameObject mInvulnerabilityGlow;
		private GameObject mAudioObject;
		private GameObject mAudioPrefab;
		public CharacterCombatModel State => this.GetModel<CharacterCombatModel>();
		public GameObject Owner => this.GetModel<PlayerModel>().RuntimeRoot;

		public void Schedule(AttackExecutionContext context)
		{
			var config = (CharacterAttackParameterConfig)context.Config.ExecutorParameterConfig;
			var level = context.SkillRuntime?.Level ?? 1;
			var values = config.AtLevel(level);
			var direction = context.Target == null ? this.GetModel<InputModel>().LastMovementDirection : (Vector2)(context.Target.transform.position - context.Owner.transform.position);
			if (direction.sqrMagnitude < 0.001f) direction = Vector2.right;
			direction.Normalize();
			State.Swing = -State.Swing;
			State.Actions.Add(new CharacterActionState {
				AttackId = context.Config.Id, Level = level, Target = context.Target, TargetSpawnId = context.Target == null ? 0 : context.Target.SpawnId,
				Damage = ((values?.Damage ?? context.Config.Damage) + context.GetModifierValue(WeaponUpgradeModifierKeys.ProjectileDamageAdd, 0f)) * context.GetDamageMultiplier(),
				Radius = (values?.Radius ?? config.Radius) + context.GetModifierValue("Melee.RadiusAdd", 0f),
				Angle = config.Angle + context.GetModifierValue("Melee.AngleAdd", 0f),
				DirectionX = direction.x, DirectionY = direction.y, Swing = State.Swing
			});
			this.GetSystem<GameLoopSystem>().RegisterUpdateable(this);
		}

		public void OnRunUpdate(float deltaTime)
		{
			var version = mResetVersion;
			var effectCount = mEffects.Count;
			State.ReductionRemaining = Mathf.Max(0, State.ReductionRemaining - deltaTime);
			State.EmpowerRemaining = Mathf.Max(0, State.EmpowerRemaining - deltaTime);
			UpdateEquipmentVisuals();
			for (var i = State.Actions.Count - 1; i >= 0; i--)
			{
				var state = State.Actions[i];
				state.Age += deltaTime;
				var attack = this.GetUtility<AttackCatalog>().Get(state.AttackId);
				var executor = attack == null ? null : this.GetUtility<AttackExecutorRegistry>().Get(attack.ExecutorId) as ICharacterSequenceExecutor;
				var alive = Owner != null && !this.GetModel<PlayerModel>().IsDead && executor != null &&
					executor.Advance(state, attack.ExecutorParameterConfig as CharacterAttackParameterConfig, Owner);
				if (version != mResetVersion) return;
				if (alive) continue;
				ReleaseVisuals(state);
				State.Actions.RemoveAt(i);
			}
			for (var i = effectCount - 1; i >= 0; i--)
			{
				var effect = mEffects[i];
				effect.Age += deltaTime;
				if (effect.Object != null)
				{
					var renderer = effect.Object.GetComponent<SpriteRenderer>();
					if (renderer != null) { var color = effect.Color; color.a *= Mathf.Clamp01(1 - effect.Age / effect.Duration); renderer.color = color; }
				}
				if (effect.Object != null && effect.Age < effect.Duration) continue;
				AreaEffectFactory.Instance.Release(effect.Object);
				mEffects.RemoveAt(i);
			}
		}

		public GameObject Visual(GameObject prefab, Vector2 position, float angle, float size)
		{
			if (!this.GetSystem<RunTimerSystem>().IsRunning()) return null;
			var obj = AreaEffectFactory.Instance.Spawn(prefab, position, WorldRootSlot.CombatEffect);
			if (obj == null) return null;
			obj.transform.rotation = Quaternion.Euler(0, 0, angle);
			obj.transform.localScale = Vector3.one * size;
			var renderer = obj.GetComponent<SpriteRenderer>();
			if (renderer != null) { renderer.color = Color.white; renderer.sortingOrder = 20; }
			return obj;
		}

		public GameObject Flash(GameObject prefab, Vector2 position, float angle, float size, float duration, Color color)
		{
			var obj = Visual(prefab, position, angle, size);
			if (obj == null) return null;
			var renderer = obj.GetComponent<SpriteRenderer>();
			if (renderer != null) renderer.color = color;
			mEffects.Add(new Effect { Object = obj, Duration = Mathf.Max(0.01f, duration), Color = color });
			this.GetSystem<GameLoopSystem>().RegisterUpdateable(this);
			return obj;
		}
		public void Sound(AudioClip clip)
		{
			if (clip == null || !this.GetSystem<RunTimerSystem>().IsRunning()) return;
			if (mAudioPrefab == null) mAudioPrefab = Resources.Load<GameObject>("Presentation/CharacterCombatAudio");
			if (mAudioObject == null) mAudioObject = AreaEffectFactory.Instance.Spawn(mAudioPrefab, Vector2.zero, WorldRootSlot.CombatEffect);
			if (mAudioObject != null) mAudioObject.GetComponent<CombatAudioView>().Play(clip);
		}

		public void BeginDodge(DodgeConfig config, DodgeRuntimeData runtime)
		{
			State.DodgeHits.Clear(); State.TrailElapsed = 0; State.TrailCount = 0;
			if (mDodges.TryGetValue(config.ExecutorId, out var executor)) executor.Begin(this, config, runtime);
			this.GetSystem<GameLoopSystem>().RegisterUpdateable(this);
		}
		public void TickDodge(DodgeConfig config, DodgeRuntimeData runtime, Vector2 before, Vector2 after)
		{
			if (mDodges.TryGetValue(config.ExecutorId, out var executor)) executor.Tick(this, config, runtime, before, after);
		}
		public void EndDodge(DodgeConfig config, DodgeRuntimeData runtime)
		{
			if (mDodges.TryGetValue(config.ExecutorId, out var executor)) executor.End(this, config, runtime);
		}

		public static bool Visible(Vector2 origin, Vector2 target)
		{
			foreach (var hit in Physics2D.LinecastAll(origin, target))
				if (MapColliderUtility.IsProjectileBlocker(hit.collider)) return false;
			return true;
		}

		public void HitArea(Vector2 origin, Vector2 direction, float radius, float angle, float damage, float knockback, HashSet<long> hits = null)
		{
			foreach (var target in this.GetSystem<CombatTargetSystem>().FindOpponentsInRange(origin, CombatFaction.Player, radius))
			{
				if (target == null || !target.isActiveAndEnabled) continue;
				var offset = (Vector2)target.transform.position - origin;
				if (Vector2.Angle(direction, offset) > angle * 0.5f || !Visible(origin, target.transform.position)) continue;
				if (hits != null && !hits.Add(target.SpawnId)) continue;
				this.GetSystem<EnemySystem>().Knockback(target, offset.normalized * knockback);
				this.GetSystem<DamageSystem>().ApplyDamage(target, damage);
			}
		}

		public void Reset()
		{
			mResetVersion++;
			AreaEffectFactory.Instance.Release(mAudioObject); mAudioObject = null;
			AreaEffectFactory.Instance.Release(mHeldWeapon); mHeldWeapon = null;
			AreaEffectFactory.Instance.Release(mStatusGlow); mStatusGlow = null;
			AreaEffectFactory.Instance.Release(mInvulnerabilityGlow); mInvulnerabilityGlow = null;
			foreach (var action in State.Actions) ReleaseVisuals(action);
			State.Actions.Clear(); State.DodgeHits.Clear();
			State.ReductionRemaining = State.EmpowerRemaining = 0;
			foreach (var effect in mEffects) AreaEffectFactory.Instance.Release(effect.Object);
			mEffects.Clear();
			this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
		}
		public void Capture(RunSaveData data)
		{
			data.CharacterActions.Clear();
			foreach (var action in State.Actions)
			{
				var attack = this.GetUtility<AttackCatalog>().Get(action.AttackId);
				if (attack == null || this.GetUtility<AttackExecutorRegistry>().Get(attack.ExecutorId) is ITransientAttackSequence) continue;
				data.CharacterActions.Add(JsonUtility.FromJson<CharacterActionState>(JsonUtility.ToJson(action)));
			}
			data.CombatReductionRemaining = State.ReductionRemaining;
			data.CombatReduction = State.Reduction;
			data.CombatEmpowerRemaining = State.EmpowerRemaining;
			data.CombatEmpowerMultiplier = State.EmpowerMultiplier;
		}
		public void Restore(RunSaveData data)
		{
			Reset();
			foreach (var action in data.CharacterActions ?? new List<CharacterActionState>())
				State.Actions.Add(JsonUtility.FromJson<CharacterActionState>(JsonUtility.ToJson(action)));
			State.ReductionRemaining = data.CombatReductionRemaining;
			State.Reduction = data.CombatReduction;
			State.EmpowerRemaining = data.CombatEmpowerRemaining;
			State.EmpowerMultiplier = data.CombatEmpowerMultiplier;
			this.GetSystem<GameLoopSystem>().RegisterUpdateable(this);
		}
		private static void ReleaseVisuals(CharacterActionState state)
		{
			foreach (var visual in state.Visuals) AreaEffectFactory.Instance.Release(visual);
			state.Visuals.Clear();
		}
		private void UpdateEquipmentVisuals()
		{
			if (Owner == null) return;
			var direction = this.GetModel<InputModel>().LastMovementDirection;
			if (direction.sqrMagnitude < 0.001f) direction = Vector2.right;
			GameObject heldPrefab = null;
			foreach (var weapon in this.GetModel<PlayerLoadoutModel>().Weapons)
				foreach (var id in weapon.AttackIds)
					if (this.GetUtility<AttackCatalog>().Get(id)?.ExecutorParameterConfig is CharacterAttackParameterConfig config && config.WeaponVisual != null)
						heldPrefab = config.WeaponVisual;
			if (heldPrefab != null && mHeldWeapon == null) mHeldWeapon = Visual(heldPrefab, Owner.transform.position, 0, 1.6f);
			if (mHeldWeapon != null)
			{
				var visible = State.Actions.Count == 0 && heldPrefab != null;
				mHeldWeapon.SetActive(visible);
				mHeldWeapon.transform.position = Owner.transform.position + new Vector3(direction.x < 0 ? -0.2f : 0.2f, -0.15f);
				mHeldWeapon.transform.rotation = Quaternion.Euler(0, 0, this.GetModel<DodgeModel>().Runtime?.IsActive == true
					? Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg : direction.x < 0 ? 130 : 50);
			}
			var dodge = this.GetModel<DodgeModel>().Runtime;
			var dodgeConfig = dodge == null ? null : this.GetUtility<DodgeCatalog>().Get(dodge.DodgeId);
			var prefab = dodgeConfig == null ? null : dodgeConfig.BurstPrefab;
			UpdateGlow(ref mStatusGlow, prefab, State.ReductionRemaining > 0 || State.EmpowerRemaining > 0,
				State.ReductionRemaining > 0 ? new Color(1, 0.75f, 0.2f, 0.55f) : new Color(0.2f, 1, 0.65f, 0.85f),
				State.ReductionRemaining > 0 ? 1.5f : 0.35f,
				State.ReductionRemaining > 0 ? Vector2.zero : direction.normalized * 0.4f);
			UpdateGlow(ref mInvulnerabilityGlow, prefab, this.GetModel<PlayerModel>().DodgeInvulnerabilityRemaining > 0,
				new Color(0.6f, 0.9f, 1, 0.5f), 1.25f, Vector2.zero);
		}
		private void UpdateGlow(ref GameObject glow, GameObject prefab, bool visible, Color color, float scale, Vector2 offset)
		{
			if (!visible) { AreaEffectFactory.Instance.Release(glow); glow = null; return; }
			if (glow == null) glow = Visual(prefab, Owner.transform.position, 0, scale);
			if (glow == null) return;
			glow.transform.position = Owner.transform.position + (Vector3)offset;
			glow.transform.localScale = Vector3.one * scale;
			glow.GetComponent<SpriteRenderer>().color = color;
		}
		protected override void OnInit()
		{
			mDodges.Add("dash", new DashEffectExecutor());
			mDodges.Add("sword-dash", new SwordDashEffectExecutor());
			mDodges.Add("reload-dash", new ReloadDashEffectExecutor());
			this.RegisterEvent<RunStartedEvent>(_ => { Reset(); this.GetSystem<GameLoopSystem>().RegisterUpdateable(this); });
			this.RegisterEvent<RunEndedEvent>(_ => Reset());
			this.RegisterEvent<RunExitedEvent>(_ => Reset());
		}
	}
}
