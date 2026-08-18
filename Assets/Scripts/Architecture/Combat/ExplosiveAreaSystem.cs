using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class ExplosiveAreaSystem : AbstractSystem, IRunUpdateable
	{
		private readonly List<TimedEffectRuntime> mTimedEffects = new();
		private readonly List<GroundFlameRuntime> mGroundFlames = new();

		public void SpawnImpact(ExplosiveProjectileAttackParameterConfig parameters, Vector2 position, CombatFaction ownerFaction, float damage)
		{
			if (parameters == null || damage <= 0f) return;
			ApplyAreaDamage(position, parameters.ExplosionRadius, ownerFaction, damage);
			if (parameters.ExplosionPrefab != null)
			{
				var effect = AreaEffectFactory.Instance.Spawn(parameters.ExplosionPrefab, position, WorldRootSlot.CombatEffect);
				if (effect != null) mTimedEffects.Add(new TimedEffectRuntime(effect, position, parameters, parameters.ExplosionVisualDuration));
			}
			if (parameters.GroundFlamePrefab != null && parameters.GroundFlameDuration > 0f)
			{
				var flame = AreaEffectFactory.Instance.Spawn(parameters.GroundFlamePrefab, position, WorldRootSlot.GroundEffect);
				if (flame != null)
				{
					mGroundFlames.Add(new GroundFlameRuntime(flame, position, parameters, ownerFaction, damage));
				}
			}
			RegisterForUpdate();
		}

		public void Reset()
		{
			foreach (var effect in mTimedEffects) AreaEffectFactory.Instance.Release(effect.View);
			foreach (var flame in mGroundFlames) AreaEffectFactory.Instance.Release(flame.View);
			mTimedEffects.Clear();
			mGroundFlames.Clear();
			this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
		}

		public IEnumerable<GroundFlameSaveData> GetGroundFlameSaveData()
		{
			var attacks = this.GetUtility<AttackCatalog>().Config;
			foreach (var flame in mGroundFlames)
			{
				var attack = attacks?.Attacks.Find(item => item != null && item.ExecutorParameterConfig == flame.Parameters);
				if (attack == null) continue;
				yield return new GroundFlameSaveData
				{
					AttackId = attack.Id,
					PositionX = flame.Position.x,
					PositionY = flame.Position.y,
					OwnerFaction = (int)flame.OwnerFaction,
					Damage = flame.Damage,
					RemainingDuration = flame.RemainingDuration,
					RemainingUntilTick = flame.RemainingUntilTick
				};
			}
		}

		public IEnumerable<TimedEffectSaveData> GetTimedEffectSaveData()
		{
			var attacks = this.GetUtility<AttackCatalog>().Config;
			foreach (var effect in mTimedEffects)
			{
				var attack = attacks?.Attacks.Find(item => item != null && item.ExecutorParameterConfig == effect.Parameters);
				if (attack == null) continue;
				yield return new TimedEffectSaveData
				{
					AttackId = attack.Id,
					PositionX = effect.Position.x,
					PositionY = effect.Position.y,
					RemainingDuration = effect.RemainingDuration
				};
			}
		}

		public void RestoreGroundFlames(IEnumerable<GroundFlameSaveData> entries)
		{
			Reset();
			if (entries == null) return;
			foreach (var entry in entries)
			{
				var parameters = entry == null ? null : this.GetUtility<AttackCatalog>().Get(entry.AttackId)?.ExecutorParameterConfig as ExplosiveProjectileAttackParameterConfig;
				if (parameters == null || parameters.GroundFlamePrefab == null) continue;
				var position = new Vector2(entry.PositionX, entry.PositionY);
				var view = AreaEffectFactory.Instance.Spawn(parameters.GroundFlamePrefab, position, WorldRootSlot.GroundEffect);
				if (view != null) mGroundFlames.Add(new GroundFlameRuntime(view, position, parameters, (CombatFaction)entry.OwnerFaction, entry));
			}
			RegisterForUpdate();
		}

		public void RestoreTimedEffects(IEnumerable<TimedEffectSaveData> entries)
		{
			if (entries == null) return;
			foreach (var entry in entries)
			{
				var parameters = entry == null ? null : this.GetUtility<AttackCatalog>().Get(entry.AttackId)?.ExecutorParameterConfig as ExplosiveProjectileAttackParameterConfig;
				if (parameters == null || parameters.ExplosionPrefab == null) continue;
				var position = new Vector2(entry.PositionX, entry.PositionY);
				var view = AreaEffectFactory.Instance.Spawn(parameters.ExplosionPrefab, position, WorldRootSlot.CombatEffect);
				if (view != null) mTimedEffects.Add(new TimedEffectRuntime(view, position, parameters, entry.RemainingDuration));
			}
			RegisterForUpdate();
		}

		public void OnRunUpdate(float deltaTime)
		{
			for (var index = mTimedEffects.Count - 1; index >= 0; index--)
			{
				var effect = mTimedEffects[index];
				effect.RemainingDuration -= deltaTime;
				if (effect.RemainingDuration > 0f) continue;
				AreaEffectFactory.Instance.Release(effect.View);
				mTimedEffects.RemoveAt(index);
			}
			for (var index = mGroundFlames.Count - 1; index >= 0; index--)
			{
				var flame = mGroundFlames[index];
				var activeDeltaTime = Mathf.Min(deltaTime, flame.RemainingDuration);
				flame.RemainingDuration -= deltaTime;
				flame.RemainingUntilTick -= activeDeltaTime;
				while (flame.RemainingUntilTick <= 0f)
				{
					ApplyAreaDamage(flame.Position, flame.Parameters.GroundFlameRadius, flame.OwnerFaction,
						flame.Damage * flame.Parameters.GroundFlameDamageMultiplier);
					flame.RemainingUntilTick += Mathf.Max(0.01f, flame.Parameters.GroundFlameTickInterval);
				}
				if (flame.RemainingDuration > 0f) continue;
				AreaEffectFactory.Instance.Release(flame.View);
				mGroundFlames.RemoveAt(index);
			}
			if (mTimedEffects.Count == 0 && mGroundFlames.Count == 0)
				this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
		}

		private void ApplyAreaDamage(Vector2 position, float radius, CombatFaction ownerFaction, float damage)
		{
			if (radius <= 0f || damage <= 0f) return;
			foreach (var target in GameArchitecture.Interface.SendQuery(new FindCombatTargetsInRangeQuery(position, ownerFaction, radius)))
				this.GetSystem<DamageSystem>().ApplyDamage(target, damage);
		}

		private void RegisterForUpdate()
		{
			if (this.GetSystem<RunTimerSystem>().IsRunning()) this.GetSystem<GameLoopSystem>().RegisterUpdateable(this);
		}

		protected override void OnInit()
		{
			this.RegisterEvent<RunEndedEvent>(_ => Reset());
		}

		private sealed class TimedEffectRuntime
		{
			public readonly GameObject View;
			public readonly Vector2 Position;
			public readonly ExplosiveProjectileAttackParameterConfig Parameters;
			public float RemainingDuration;

			public TimedEffectRuntime(GameObject view, Vector2 position, ExplosiveProjectileAttackParameterConfig parameters, float duration)
			{
				View = view;
				Position = position;
				Parameters = parameters;
				RemainingDuration = Mathf.Max(0.01f, duration);
			}
		}

		private sealed class GroundFlameRuntime
		{
			public readonly GameObject View;
			public readonly Vector2 Position;
			public readonly ExplosiveProjectileAttackParameterConfig Parameters;
			public readonly CombatFaction OwnerFaction;
			public readonly float Damage;
			public float RemainingDuration;
			public float RemainingUntilTick;

			public GroundFlameRuntime(GameObject view, Vector2 position, ExplosiveProjectileAttackParameterConfig parameters,
				CombatFaction ownerFaction, float damage)
			{
				View = view;
				Position = position;
				Parameters = parameters;
				OwnerFaction = ownerFaction;
				Damage = damage;
				RemainingDuration = Mathf.Max(0.01f, parameters.GroundFlameDuration);
				RemainingUntilTick = Mathf.Max(0.01f, parameters.GroundFlameTickInterval);
			}

			public GroundFlameRuntime(GameObject view, Vector2 position, ExplosiveProjectileAttackParameterConfig parameters,
				CombatFaction ownerFaction, GroundFlameSaveData data)
			{
				View = view;
				Position = position;
				Parameters = parameters;
				OwnerFaction = ownerFaction;
				Damage = data.Damage;
				RemainingDuration = Mathf.Max(0.01f, data.RemainingDuration);
				RemainingUntilTick = Mathf.Max(0.01f, data.RemainingUntilTick);
			}
		}
	}
}
