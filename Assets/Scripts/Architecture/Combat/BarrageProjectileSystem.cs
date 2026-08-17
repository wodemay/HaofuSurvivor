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
			LaunchProjectile(runtime);
			runtime.TimeUntilNextProjectile = Mathf.Max(0.01f, parameters.ProjectileInterval);
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
				runtime.DurationRemaining -= deltaTime;
				runtime.OrbitAngle -= runtime.Parameters.OrbitDegreesPerSecond * deltaTime;
				runtime.TimeUntilNextProjectile -= deltaTime;
				while (runtime.DurationRemaining > 0f && runtime.TimeUntilNextProjectile <= 0f)
				{
					LaunchProjectile(runtime);
					runtime.TimeUntilNextProjectile += Mathf.Max(0.01f, runtime.Parameters.ProjectileInterval);
				}
				if (runtime.DurationRemaining <= 0f) mPendingBarrages.RemoveAt(index);
			}
			if (mPendingBarrages.Count == 0) this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
		}

		public void Reset()
		{
			mPendingBarrages.Clear();
			this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
		}

		private void LaunchProjectile(BarrageRuntime runtime)
		{
			var orbitDirection = new Vector2(Mathf.Cos(runtime.OrbitAngle * Mathf.Deg2Rad), Mathf.Sin(runtime.OrbitAngle * Mathf.Deg2Rad));
			var position = (Vector2)runtime.Owner.transform.position + orbitDirection * runtime.Parameters.OrbitRadius +
				Random.insideUnitCircle * runtime.Parameters.EmissionRadius;
			ProjectileFactory.Instance.Spawn(runtime.Parameters, position, orbitDirection, runtime.OwnerFaction,
				runtime.Damage, runtime.Parameters.MoveSpeed, 0);
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
			public float DurationRemaining;
			public float TimeUntilNextProjectile;
			public float OrbitAngle;

			public BarrageRuntime(GameObject owner, CombatFaction ownerFaction, BarrageProjectileAttackParameterConfig parameters, float damage)
			{
				Owner = owner;
				OwnerFaction = ownerFaction;
				Parameters = parameters;
				Damage = damage;
				DurationRemaining = Mathf.Max(0.01f, parameters.Duration);
				TimeUntilNextProjectile = 0f;
				OrbitAngle = Random.Range(0f, 360f);
			}
		}
	}
}
