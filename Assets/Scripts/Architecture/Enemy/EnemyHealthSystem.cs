using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class EnemyHealthSystem : AbstractSystem
	{
		private readonly Dictionary<CombatEntity, float> mCurrentHealth = new();

		public void Register(CombatEntity enemy, float baseHealth)
		{
			if (enemy == null) return;
			var multiplier = this.GetModel<RunTimerModel>().EnemyHealthMultiplier;
			mCurrentHealth[enemy] = Mathf.Max(1f, baseHealth * multiplier);
		}

		public void Unregister(CombatEntity enemy)
		{
			if (enemy != null) mCurrentHealth.Remove(enemy);
		}

		public void ApplyDamage(CombatEntity enemy, float damage)
		{
			if (enemy == null || damage <= 0f || !mCurrentHealth.TryGetValue(enemy, out var currentHealth)) return;
			currentHealth = Mathf.Max(0f, currentHealth - damage);
			mCurrentHealth[enemy] = currentHealth;
			this.SendEvent(new EnemyDamagedEvent(enemy, damage, currentHealth));
			if (currentHealth > 0f) return;

			mCurrentHealth.Remove(enemy);
			this.SendEvent(new EnemyDiedEvent(enemy));
			this.GetSystem<EnemySystem>().Release(enemy.transform);
		}

		protected override void OnInit()
		{
		}
	}
}
