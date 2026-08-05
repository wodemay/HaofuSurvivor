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
}
