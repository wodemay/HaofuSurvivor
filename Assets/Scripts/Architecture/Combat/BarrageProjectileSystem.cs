using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class BarrageProjectileSystem : AbstractSystem, IRunUpdateable
	{
		private readonly List<BarrageRuntime> mPendingBarrages = new();

		public void Schedule(GameObject owner, CombatFaction ownerFaction, BarrageProjectileAttackParameterConfig parameters, float damage)
		{
			if (owner == null || parameters == null || parameters.ProjectilePrefab == null) return;
			var runtime = new BarrageRuntime(owner, ownerFaction, parameters, damage);
			LaunchBurst(runtime);
			if (runtime.BurstsRemaining <= 0) return;
			mPendingBarrages.Add(runtime);
			this.GetSystem<GameLoopSystem>().RegisterUpdateable(this);
		}

		public void OnRunUpdate(float deltaTime)
		{
			for (var index = mPendingBarrages.Count - 1; index >= 0; index--)
			{
				var runtime = mPendingBarrages[index];
				if (runtime.Owner == null)
				{
					mPendingBarrages.RemoveAt(index);
					continue;
				}
				runtime.TimeUntilNextBurst -= deltaTime;
				while (runtime.BurstsRemaining > 0 && runtime.TimeUntilNextBurst <= 0f)
				{
					LaunchBurst(runtime);
					if (runtime.BurstsRemaining > 0) runtime.TimeUntilNextBurst += Mathf.Max(0.01f, runtime.Parameters.BurstInterval);
				}
				if (runtime.BurstsRemaining <= 0) mPendingBarrages.RemoveAt(index);
			}
			if (mPendingBarrages.Count == 0) this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
		}

		public void Reset()
		{
			mPendingBarrages.Clear();
			this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
		}

		private static void LaunchBurst(BarrageRuntime runtime)
		{
			var projectileCount = Mathf.Max(1, runtime.Parameters.ProjectilesPerBurst);
			var angleStep = 360f / projectileCount;
			var position = (Vector2)runtime.Owner.transform.position;
			for (var index = 0; index < projectileCount; index++)
			{
				var angle = angleStep * index * Mathf.Deg2Rad;
				var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
				ProjectileFactory.Instance.Spawn(runtime.Parameters, position, direction, runtime.OwnerFaction,
					runtime.Damage, runtime.Parameters.MoveSpeed, 0);
			}
			runtime.BurstsRemaining--;
		}

		protected override void OnInit()
		{
			Reset();
		}

		private class BarrageRuntime
		{
			public readonly GameObject Owner;
			public readonly CombatFaction OwnerFaction;
			public readonly BarrageProjectileAttackParameterConfig Parameters;
			public readonly float Damage;
			public int BurstsRemaining;
			public float TimeUntilNextBurst;

			public BarrageRuntime(GameObject owner, CombatFaction ownerFaction, BarrageProjectileAttackParameterConfig parameters, float damage)
			{
				Owner = owner;
				OwnerFaction = ownerFaction;
				Parameters = parameters;
				Damage = damage;
				BurstsRemaining = Mathf.Max(1, parameters.BurstCount);
				TimeUntilNextBurst = Mathf.Max(0.01f, parameters.BurstInterval);
			}
		}
	}
}
