using UnityEngine;

namespace HaoFuSurvivor
{
	public class BreakableObjectView : MonoBehaviour
	{
		public string StableId { get; private set; }
		public BreakableObjectConfig Config { get; private set; }
		public CombatEntity CombatEntity { get; private set; }

		public void Configure(string stableId, BreakableObjectConfig config)
		{
			StableId = stableId;
			Config = config;
			CombatEntity = GetComponent<CombatEntity>();
			if (CombatEntity == null) CombatEntity = gameObject.AddComponent<CombatEntity>();
			CombatEntity.Initialize(CombatFaction.Enemy);
		}
	}
}
