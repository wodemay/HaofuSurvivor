using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class CombatEntity : MonoBehaviour, IController
	{
		public CombatFaction Faction { get; private set; }
		private bool mIsInitialized;

		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		public void Initialize(CombatFaction faction)
		{
			Faction = faction;
			mIsInitialized = true;
			this.SendCommand(new RegisterCombatTargetCommand(this));
		}

		private void OnEnable()
		{
			if (mIsInitialized) this.SendCommand(new RegisterCombatTargetCommand(this));
		}

		private void OnDisable()
		{
			if (mIsInitialized) this.SendCommand(new UnregisterCombatTargetCommand(this));
		}
	}
}
