using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class CombatTargetSystem : AbstractSystem
	{
		private readonly HashSet<CombatEntity> mEntities = new();

		public void Register(CombatEntity entity)
		{
			if (entity != null) mEntities.Add(entity);
		}

		public void Unregister(CombatEntity entity)
		{
			if (entity != null) mEntities.Remove(entity);
		}

		public CombatEntity FindClosestOpponent(Vector2 position, CombatFaction ownerFaction, float range)
		{
			var rangeSquared = range * range;
			var closestDistanceSquared = rangeSquared;
			CombatEntity closest = null;
			foreach (var entity in mEntities)
			{
				if (entity == null || !entity.isActiveAndEnabled || entity.Faction == ownerFaction) continue;
				var distanceSquared = ((Vector2)entity.transform.position - position).sqrMagnitude;
				if (distanceSquared > closestDistanceSquared) continue;
				closestDistanceSquared = distanceSquared;
				closest = entity;
			}
			return closest;
		}

		public IReadOnlyList<CombatEntity> FindOpponentsInRange(Vector2 position, CombatFaction ownerFaction, float range)
		{
			var targets = new List<CombatEntity>();
			var rangeSquared = Mathf.Max(0f, range) * Mathf.Max(0f, range);
			foreach (var entity in mEntities)
			{
				if (entity == null || !entity.isActiveAndEnabled || entity.Faction == ownerFaction) continue;
				if (((Vector2)entity.transform.position - position).sqrMagnitude <= rangeSquared) targets.Add(entity);
			}
			return targets;
		}

		protected override void OnInit()
		{
		}
	}

	public class FindClosestCombatTargetQuery : AbstractQuery<CombatEntity>
	{
		private readonly Vector2 mPosition;
		private readonly CombatFaction mOwnerFaction;
		private readonly float mRange;

		public FindClosestCombatTargetQuery(Vector2 position, CombatFaction ownerFaction, float range)
		{
			mPosition = position;
			mOwnerFaction = ownerFaction;
			mRange = Mathf.Max(0f, range);
		}

		protected override CombatEntity OnDo()
		{
			return this.GetSystem<CombatTargetSystem>().FindClosestOpponent(mPosition, mOwnerFaction, mRange);
		}
	}

	public class FindCombatTargetsInRangeQuery : AbstractQuery<IReadOnlyList<CombatEntity>>
	{
		private readonly Vector2 mPosition;
		private readonly CombatFaction mOwnerFaction;
		private readonly float mRange;

		public FindCombatTargetsInRangeQuery(Vector2 position, CombatFaction ownerFaction, float range)
		{
			mPosition = position;
			mOwnerFaction = ownerFaction;
			mRange = range;
		}

		protected override IReadOnlyList<CombatEntity> OnDo()
		{
			return this.GetSystem<CombatTargetSystem>().FindOpponentsInRange(mPosition, mOwnerFaction, mRange);
		}
	}
}
