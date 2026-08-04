using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class EnemyAttackSystem : AbstractSystem
	{
		private readonly Dictionary<int, ContactAttackRuntime> mContactAttacks = new();

		public void RegisterContactAttack(int runtimeId, int attackId)
		{
			var attack = this.GetUtility<EnemyAttackCatalog>().Get(attackId);
			if (attack == null || attack.AttackType != EnemyAttackType.Contact) return;
			mContactAttacks[runtimeId] = new ContactAttackRuntime(attack);
		}

		public void UnregisterContactAttack(int runtimeId)
		{
			mContactAttacks.Remove(runtimeId);
		}

		public void SetContactState(int runtimeId, bool isContacting)
		{
			if (!mContactAttacks.TryGetValue(runtimeId, out var runtime)) return;
			runtime.IsContacting = isContacting;
			if (isContacting) runtime.CooldownRemaining = 0f;
		}

		public void Tick(float deltaTime)
		{
			if (this.GetModel<RunModel>().Phase != RunPhase.Active) return;

			foreach (var runtime in mContactAttacks.Values)
			{
				if (!runtime.IsContacting) continue;
				runtime.CooldownRemaining -= deltaTime;
				if (runtime.CooldownRemaining > 0f) continue;

				runtime.CooldownRemaining = Mathf.Max(0.01f, runtime.Config.Cooldown);
				var damage = runtime.Config.Damage * this.GetModel<RunTimerModel>().EnemyDamageMultiplier;
				this.GetSystem<DamageSystem>().ApplyPlayerDamage(damage);
			}
		}

		protected override void OnInit()
		{
		}

		private class ContactAttackRuntime
		{
			public readonly EnemyAttackConfig Config;
			public bool IsContacting;
			public float CooldownRemaining;

			public ContactAttackRuntime(EnemyAttackConfig config)
			{
				Config = config;
			}
		}
	}
}
