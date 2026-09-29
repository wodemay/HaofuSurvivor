using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class DodgeSystem : AbstractSystem, IRunFixedUpdateable
	{
		public bool IsEquipped => this.GetModel<DodgeModel>().Runtime != null;

		public bool Equip(int dodgeId)
		{
			Reset();
			if (dodgeId == 0) return true;
			if (this.GetUtility<DodgeCatalog>().Get(dodgeId) == null) return false;
			this.GetModel<DodgeModel>().Runtime = new DodgeRuntimeData(dodgeId);
			if (this.GetSystem<RunTimerSystem>().IsRunning()) this.GetSystem<GameLoopSystem>().RegisterFixedUpdateable(this);
			return true;
		}

		public bool TryStart()
		{
			if (!this.GetSystem<RunTimerSystem>().IsRunning()) return false;
			var runtime = this.GetModel<DodgeModel>().Runtime;
			var player = this.GetModel<PlayerModel>();
			if (runtime == null || runtime.IsActive || runtime.CooldownRemaining > 0f || !player.IsRegistered || player.IsDead) return false;
			var config = this.GetUtility<DodgeCatalog>().Get(runtime.DodgeId);
			if (config == null) return false;
			var direction = this.GetModel<InputModel>().Movement;
			if (direction.sqrMagnitude <= 0.001f) direction = this.GetModel<InputModel>().LastMovementDirection;
			if (direction.sqrMagnitude <= 0.001f) direction = Vector2.right;
			runtime.Direction = direction.normalized;
			runtime.IsActive = true;
			runtime.DurationRemaining = GetDuration(config, runtime.Level);
			runtime.CooldownRemaining = GetCooldown(config, runtime.Level);
			player.DodgeInvulnerabilityRemaining = GetInvulnerabilityDuration(config, runtime.Level);
			GameArchitecture.Interface.GetSystem<CharacterCombatSystem>().BeginDodge(config, runtime);
			this.SendEvent(new DodgeStartedEvent(runtime.DodgeId, runtime.Level));
			return true;
		}

		public void OnRunFixedUpdate(float deltaTime)
		{
			if (deltaTime <= 0f) return;
			var runtime = this.GetModel<DodgeModel>().Runtime;
			if (runtime == null) return;
			runtime.CooldownRemaining = Mathf.Max(0f, runtime.CooldownRemaining - deltaTime);
			if (!runtime.IsActive) return;
			var config = this.GetUtility<DodgeCatalog>().Get(runtime.DodgeId);
			if (config == null)
			{
				runtime.IsActive = false;
				runtime.DurationRemaining = 0f;
				runtime.CooldownRemaining = 0f;
				this.GetModel<PlayerModel>().DodgeInvulnerabilityRemaining = 0f;
				return;
			}
			var player = this.GetModel<PlayerModel>();
			if (!player.IsRegistered || player.IsDead)
			{
				runtime.IsActive = false;
				runtime.DurationRemaining = 0f;
				player.DodgeInvulnerabilityRemaining = 0f;
				return;
			}
			var stepTime = Mathf.Min(deltaTime, Mathf.Max(0f, runtime.DurationRemaining));
			var movement = runtime.Direction * GetDistance(config, runtime.Level) / GetDuration(config, runtime.Level) * stepTime;
			var before = player.Position;
			this.GetSystem<PlayerSystem>().MoveBy(movement);
			var after = player.Position;
			GameArchitecture.Interface.GetSystem<CharacterCombatSystem>().TickDodge(config, runtime, before, after);
			runtime.DurationRemaining = Mathf.Max(0f, runtime.DurationRemaining - stepTime);
			var blocked = ((after - before) - movement).sqrMagnitude > 0.000001f;
			if (!blocked && runtime.DurationRemaining > 0f) return;
			if (blocked) player.DodgeInvulnerabilityRemaining = 0f;
			runtime.DurationRemaining = 0f;
			runtime.IsActive = false;
			GameArchitecture.Interface.GetSystem<CharacterCombatSystem>().EndDodge(config, runtime);
			this.SendEvent(new DodgeEndedEvent(runtime.DodgeId, runtime.Level));
		}

		public bool Upgrade()
		{
			var runtime = this.GetModel<DodgeModel>().Runtime;
			var config = runtime == null ? null : this.GetUtility<DodgeCatalog>().Get(runtime.DodgeId);
			if (runtime == null || config == null || !config.CanUpgrade || runtime.Level >= config.MaxLevel) return false;
			runtime.Level++;
			this.SendEvent(new DodgeUpgradedEvent(runtime.DodgeId, runtime.Level));
			return true;
		}

		public bool HasUpgrade()
		{
			var runtime = this.GetModel<DodgeModel>().Runtime;
			var config = runtime == null ? null : this.GetUtility<DodgeCatalog>().Get(runtime.DodgeId);
			return runtime != null && config != null && config.CanUpgrade && runtime.Level < config.MaxLevel;
		}

		public float GetCooldown(DodgeConfig config, int level) => Mathf.Max(0.01f,
			(config.Cooldown + (GetUpgrade(config, level)?.CooldownAdd ?? 0f)) * this.GetSystem<StatSystem>().GetCooldownMultiplier());
		public float GetDuration(DodgeConfig config, int level) => Mathf.Max(0.01f, config.Duration + (GetUpgrade(config, level)?.DurationAdd ?? 0f));
		public float GetDistance(DodgeConfig config, int level) => Mathf.Max(0f, config.Distance + (GetUpgrade(config, level)?.DistanceAdd ?? 0f));
		public float GetInvulnerabilityDuration(DodgeConfig config, int level) => Mathf.Max(0f, config.InvulnerabilityDuration + (GetUpgrade(config, level)?.InvulnerabilityDurationAdd ?? 0f));

		public void Reset()
		{
			this.GetModel<DodgeModel>().Reset();
			this.GetModel<PlayerModel>().DodgeInvulnerabilityRemaining = 0f;
			this.GetSystem<GameLoopSystem>().UnregisterFixedUpdateable(this);
		}

		public void RestoreRuntime(float cooldownRemaining, float durationRemaining, Vector2 direction, bool isActive)
		{
			var runtime = this.GetModel<DodgeModel>().Runtime;
			if (runtime == null) return;
			runtime.CooldownRemaining = Mathf.Max(0f, cooldownRemaining);
			runtime.DurationRemaining = Mathf.Max(0f, durationRemaining);
			runtime.Direction = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.right;
			runtime.IsActive = isActive && runtime.DurationRemaining > 0f;
			if (!runtime.IsActive) return;
			var config = this.GetUtility<DodgeCatalog>().Get(runtime.DodgeId);
			if (config == null)
			{
				runtime.IsActive = false;
				runtime.DurationRemaining = 0f;
				this.GetModel<PlayerModel>().DodgeInvulnerabilityRemaining = 0f;
				return;
			}
			runtime.DurationRemaining = Mathf.Min(runtime.DurationRemaining, GetDuration(config, runtime.Level));
			runtime.IsRestoring = true;
			try
			{
				GameArchitecture.Interface.GetSystem<CharacterCombatSystem>().BeginDodge(config, runtime);
			}
			finally
			{
				runtime.IsRestoring = false;
			}
		}

		private DodgeLevelUpgrade GetUpgrade(DodgeConfig config, int level)
		{
			return config.LevelUpgrades.Find(item => item != null && item.Level == level);
		}

		protected override void OnInit()
		{
			Reset();
		}
	}
}
