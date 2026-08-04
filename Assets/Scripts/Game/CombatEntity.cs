using UnityEngine;

namespace HaoFuSurvivor
{
	public class CombatEntity : MonoBehaviour
	{
		public CombatFaction Faction { get; private set; }

		public void Initialize(CombatFaction faction)
		{
			Faction = faction;
		}
	}
}
