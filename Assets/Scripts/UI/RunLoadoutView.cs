using System;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

namespace HaoFuSurvivor
{
	public sealed class RunLoadoutView : MonoBehaviour, IController
	{
		[Serializable]
		public sealed class Slot
		{
			public Image Icon;
			public Text Name;
			public Text Level;
		}
		public Slot[] Weapons;
		public Slot[] Skills;
		public IArchitecture GetArchitecture() => GameArchitecture.Interface;
		private void Awake()
		{
			this.RegisterEvent<WeaponEquippedEvent>(_ => Refresh()).UnRegisterWhenGameObjectDestroyed(gameObject);
			this.RegisterEvent<WeaponReplacedEvent>(_ => Refresh()).UnRegisterWhenGameObjectDestroyed(gameObject);
			this.RegisterEvent<WeaponUpgradedEvent>(_ => Refresh()).UnRegisterWhenGameObjectDestroyed(gameObject);
			this.RegisterEvent<SkillUpgradedEvent>(_ => Refresh()).UnRegisterWhenGameObjectDestroyed(gameObject);
			this.RegisterEvent<DodgeUpgradedEvent>(_ => Refresh()).UnRegisterWhenGameObjectDestroyed(gameObject);
			this.RegisterEvent<RunStartedEvent>(_ => Refresh()).UnRegisterWhenGameObjectDestroyed(gameObject);
			this.RegisterEvent<CharacterExclusivePerkUpgradedEvent>(_ => Refresh()).UnRegisterWhenGameObjectDestroyed(gameObject);
			this.RegisterEvent<RunTimerUpdatedEvent>(_ => Refresh()).UnRegisterWhenGameObjectDestroyed(gameObject);
		}
		private void OnEnable() => Refresh();
		public void Refresh()
		{
			var state = this.SendQuery(new GetRunLoadoutQuery());
			Render(Weapons, state.Weapons);
			Render(Skills, state.Skills);
		}
		private static void Render(Slot[] slots, IReadOnlyList<RunLoadoutItem> items)
		{
			if (slots == null) return;
			for (var i = 0; i < slots.Length; i++)
			{
				var slot = slots[i];
				if (slot?.Icon == null || slot.Name == null || slot.Level == null) continue;
				var occupied = i < items.Count;
				slot.Icon.sprite = occupied ? items[i].Icon : null;
				slot.Icon.enabled = occupied && slot.Icon.sprite != null;
				slot.Icon.preserveAspect = true;
				slot.Name.text = occupied ? items[i].Name : "空位";
				slot.Level.text = occupied ? $"Lv.{items[i].Level}" : "";
			}
		}
	}
}
