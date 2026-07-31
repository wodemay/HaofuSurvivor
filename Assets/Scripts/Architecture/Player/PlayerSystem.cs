using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class PlayerSystem : AbstractSystem
	{
		public void Register(Vector2 initialPosition, CharacterConfig character)
		{
			var playerModel = this.GetModel<PlayerModel>();
			var statModel = this.GetModel<PlayerStatModel>();

			statModel.MaxHealth = Mathf.Max(1f, character.MaxHealth);
			statModel.MoveSpeed = Mathf.Max(0f, character.MoveSpeed);
			statModel.AttackPower = Mathf.Max(0f, character.AttackPower);
			playerModel.CharacterId = character.Id;
			playerModel.Position = initialPosition;
			playerModel.CurrentHealth = statModel.MaxHealth;
			playerModel.IsDead = false;
			playerModel.IsRegistered = true;
		}

		public void Unregister()
		{
			var playerModel = this.GetModel<PlayerModel>();
			playerModel.IsRegistered = false;
			playerModel.IsDead = false;
			this.GetModel<InputModel>().Movement = Vector2.zero;
		}

		public void Move(float deltaTime)
		{
			var playerModel = this.GetModel<PlayerModel>();
			if (!playerModel.IsRegistered || playerModel.IsDead) return;
			if (this.GetModel<RunModel>().Phase != RunPhase.Active) return;

			var direction = this.GetModel<InputModel>().Movement;
			playerModel.Position += direction * this.GetSystem<StatSystem>().GetMoveSpeed() * deltaTime;
		}

		public void ApplyDamage(float damage)
		{
			var playerModel = this.GetModel<PlayerModel>();
			if (!playerModel.IsRegistered || playerModel.IsDead || damage <= 0f) return;

			playerModel.CurrentHealth = Mathf.Max(0f, playerModel.CurrentHealth - damage);
			this.SendEvent(new PlayerDamagedEvent(damage, playerModel.CurrentHealth));

			if (playerModel.CurrentHealth > 0f) return;

			playerModel.IsDead = true;
			this.SendEvent(new PlayerDiedEvent());
			this.GetSystem<RunSystem>().EndWithDefeat();
		}

		protected override void OnInit()
		{
		}
	}
}
