using QFramework;
using UnityEngine;
using UnityEngine.UI;

namespace HaoFuSurvivor
{
	public class PlayerHealthBarView : MonoBehaviour, IController
	{
		private Slider mHealthSlider;

		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		private void Awake()
		{
			mHealthSlider = GetComponentInChildren<Slider>();
			if (mHealthSlider == null)
			{
				Debug.LogError("Player health bar requires a Slider.");
				return;
			}

			this.RegisterEvent<PlayerDamagedEvent>(OnPlayerDamaged)
				.UnRegisterWhenGameObjectDestroyed(gameObject);
			this.RegisterEvent<PlayerHealedEvent>(OnPlayerHealed)
				.UnRegisterWhenGameObjectDestroyed(gameObject);
			this.RegisterEvent<PlayerHealthRestoredEvent>(_ => Refresh())
				.UnRegisterWhenGameObjectDestroyed(gameObject);
			Refresh();
		}

		private void OnPlayerDamaged(PlayerDamagedEvent damageEvent)
		{
			Refresh();
		}

		private void OnPlayerHealed(PlayerHealedEvent healedEvent)
		{
			Refresh();
		}

		private void Refresh()
		{
			if (mHealthSlider == null) return;
			var maxHealth = this.GetModel<PlayerStatModel>().MaxHealth;
			var currentHealth = this.GetModel<PlayerModel>().CurrentHealth;
			mHealthSlider.value = maxHealth > 0f ? currentHealth / maxHealth : 0f;
		}
	}
}
