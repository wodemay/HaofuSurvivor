using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class BarrageProjectileSystem : AbstractSystem, IRunUpdateable
	{
		private readonly List<BarrageRuntime> mPendingBarrages = new();

		public void Schedule(GameObject owner, CombatFaction ownerFaction, BarrageProjectileAttackParameterConfig parameters, float damage,
			bool isUltimate)
		{
			if (owner == null || parameters == null || parameters.ProjectilePrefab == null) return;
			var runtime = new BarrageRuntime(owner, ownerFaction, parameters, damage, isUltimate);
			LaunchProjectile(runtime);
			runtime.TimeUntilNextProjectile = Mathf.Max(0.01f, runtime.ProjectileInterval);
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
				runtime.OrbitAngle -= runtime.OrbitDegreesPerSecond * deltaTime;
				runtime.TimeUntilNextProjectile -= deltaTime;
				while (runtime.DurationRemaining > 0f && runtime.TimeUntilNextProjectile <= 0f)
				{
					LaunchProjectile(runtime);
					runtime.TimeUntilNextProjectile += Mathf.Max(0.01f, runtime.ProjectileInterval);
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

		public IEnumerable<BarrageSaveData> GetSaveData()
		{
			var attacks = this.GetUtility<AttackCatalog>().Config;
			foreach (var runtime in mPendingBarrages)
			{
				var attack = attacks?.Attacks.Find(item => item != null && item.ExecutorParameterConfig == runtime.Parameters);
				if (attack == null || runtime.Owner == null) continue;
				yield return new BarrageSaveData
				{
					AttackId = attack.Id,
					OwnerFaction = (int)runtime.OwnerFaction,
					Damage = runtime.Damage,
					IsUltimate = runtime.IsUltimate,
					DurationRemaining = runtime.DurationRemaining,
					TimeUntilNextProjectile = runtime.TimeUntilNextProjectile,
					OrbitAngle = runtime.OrbitAngle
				};
			}
		}

		public void Restore(IEnumerable<BarrageSaveData> entries, GameObject owner)
		{
			Reset();
			if (entries == null || owner == null) return;
			foreach (var entry in entries)
			{
				var parameters = entry == null ? null : this.GetUtility<AttackCatalog>().Get(entry.AttackId)?.ExecutorParameterConfig as BarrageProjectileAttackParameterConfig;
				if (parameters == null) continue;
				mPendingBarrages.Add(new BarrageRuntime(owner, (CombatFaction)entry.OwnerFaction, parameters, entry));
			}
			if (mPendingBarrages.Count > 0) this.GetSystem<GameLoopSystem>().RegisterUpdateable(this);
		}

		private void LaunchProjectile(BarrageRuntime runtime)
		{
			var orbitDirection = new Vector2(Mathf.Cos(runtime.OrbitAngle * Mathf.Deg2Rad), Mathf.Sin(runtime.OrbitAngle * Mathf.Deg2Rad));
			var position = (Vector2)runtime.Owner.transform.position + orbitDirection * runtime.OrbitRadius +
				Random.insideUnitCircle * runtime.Parameters.EmissionRadius;
			ProjectileFactory.Instance.Spawn(runtime.Parameters, position, orbitDirection, runtime.OwnerFaction,
				runtime.Damage, runtime.MoveSpeed, runtime.Pierce);
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
			public readonly float ProjectileInterval;
			public readonly float OrbitRadius;
			public readonly float OrbitDegreesPerSecond;
			public readonly float MoveSpeed;
			public readonly int Pierce;
			public readonly bool IsUltimate;
			public float DurationRemaining;
			public float TimeUntilNextProjectile;
			public float OrbitAngle;

			public BarrageRuntime(GameObject owner, CombatFaction ownerFaction, BarrageProjectileAttackParameterConfig parameters, float damage,
				bool isUltimate)
			{
				Owner = owner;
				OwnerFaction = ownerFaction;
				Parameters = parameters;
				Damage = damage * (isUltimate ? parameters.UltimateDamageMultiplier : 1f);
				ProjectileInterval = isUltimate ? parameters.UltimateProjectileInterval : parameters.ProjectileInterval;
				OrbitRadius = isUltimate ? parameters.UltimateOrbitRadius : parameters.OrbitRadius;
				OrbitDegreesPerSecond = isUltimate ? parameters.UltimateOrbitDegreesPerSecond : parameters.OrbitDegreesPerSecond;
				MoveSpeed = parameters.MoveSpeed * (isUltimate ? parameters.UltimateSpeedMultiplier : 1f);
				Pierce = isUltimate ? Mathf.Max(0, parameters.UltimatePierce) : 0;
				IsUltimate = isUltimate;
				DurationRemaining = Mathf.Max(0.01f, isUltimate ? parameters.UltimateDuration : parameters.Duration);
				TimeUntilNextProjectile = 0f;
				OrbitAngle = Random.Range(0f, 360f);
			}

			public BarrageRuntime(GameObject owner, CombatFaction ownerFaction, BarrageProjectileAttackParameterConfig parameters,
				BarrageSaveData data)
			{
				Owner = owner;
				OwnerFaction = ownerFaction;
				Parameters = parameters;
				Damage = data.Damage;
				IsUltimate = data.IsUltimate;
				ProjectileInterval = IsUltimate ? parameters.UltimateProjectileInterval : parameters.ProjectileInterval;
				OrbitRadius = IsUltimate ? parameters.UltimateOrbitRadius : parameters.OrbitRadius;
				OrbitDegreesPerSecond = IsUltimate ? parameters.UltimateOrbitDegreesPerSecond : parameters.OrbitDegreesPerSecond;
				MoveSpeed = parameters.MoveSpeed * (IsUltimate ? parameters.UltimateSpeedMultiplier : 1f);
				Pierce = IsUltimate ? Mathf.Max(0, parameters.UltimatePierce) : 0;
				DurationRemaining = Mathf.Max(0.01f, data.DurationRemaining);
				TimeUntilNextProjectile = Mathf.Max(0.01f, data.TimeUntilNextProjectile);
				OrbitAngle = data.OrbitAngle;
			}
		}
	}
}
