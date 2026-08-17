using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public readonly struct SavedRunState
	{
		public readonly bool HasSave;

		public SavedRunState(bool hasSave)
		{
			HasSave = hasSave;
		}
	}

	public class HasSavedRunQuery : AbstractQuery<SavedRunState>
	{
		protected override SavedRunState OnDo()
		{
			return new SavedRunState(GameArchitecture.Interface.GetUtility<RunSaveStorage>().HasSave());
		}
	}

	public class RunSaveSystem : AbstractSystem, IRunUpdateable
	{
		private const float AutoSaveIntervalSeconds = 30f;
		private float mAutoSaveElapsed;

		public void ResetAutoSaveTimer()
		{
			mAutoSaveElapsed = 0f;
		}

		public void OnRunUpdate(float deltaTime)
		{
			mAutoSaveElapsed += deltaTime;
			if (mAutoSaveElapsed < AutoSaveIntervalSeconds) return;
			mAutoSaveElapsed = 0f;
			SaveCurrentRun();
		}
		public void SaveCurrentRun()
		{
			var run = this.GetModel<RunModel>();
			if (run.Phase != RunPhase.Active && run.Phase != RunPhase.Paused && run.Phase != RunPhase.LevelUpSelection) return;

			var player = this.GetModel<PlayerModel>();
			var experience = this.GetModel<ExperienceModel>();
			var timer = this.GetModel<RunTimerModel>();
			var data = new RunSaveData
			{
				CharacterId = player.CharacterId,
				ElapsedSeconds = timer.ElapsedSeconds,
				CurrentStageIndex = timer.CurrentStageIndex,
				CurrentHealth = player.CurrentHealth,
				PositionX = player.Position.x,
				PositionY = player.Position.y,
				Level = experience.Level,
				CurrentExperience = experience.CurrentExperience,
				RequiredExperience = experience.RequiredExperience,
				DodgeId = this.GetModel<PlayerLoadoutModel>().DodgeId,
				DodgeLevel = this.GetModel<DodgeModel>().Runtime?.Level ?? 0,
				HasSkillSnapshot = true
			};

			foreach (var skill in this.GetModel<PlayerLoadoutModel>().Skills)
				data.Skills.Add(new SkillSaveData { SkillId = skill.SkillId, Level = skill.Level });
			foreach (var weapon in this.GetModel<PlayerLoadoutModel>().Weapons)
			{
				var weaponData = new WeaponSaveData
				{
					RuntimeId = weapon.RuntimeId,
					WeaponId = weapon.WeaponId,
					Level = weapon.Level,
					CanUpgrade = weapon.CanUpgrade,
					AttackIds = new List<int>(weapon.AttackIds)
				};
				foreach (var modifier in weapon.GetModifierSnapshots())
					weaponData.Modifiers.Add(new WeaponModifierSaveData { AttackId = modifier.AttackId, Key = modifier.Key, Value = modifier.Value });
				data.Weapons.Add(weaponData);
			}

			this.GetUtility<RunSaveStorage>().Save(data);
		}

		public RunSaveData Load() => this.GetUtility<RunSaveStorage>().Load();

		public void Clear() => this.GetUtility<RunSaveStorage>().Clear();

		public bool Restore(RunSaveData data)
		{
			if (data == null) return false;
			var player = this.GetModel<PlayerModel>();
			var timer = this.GetModel<RunTimerModel>();
			var experience = this.GetModel<ExperienceModel>();
			player.CurrentHealth = Mathf.Max(0f, data.CurrentHealth);
			player.Position = new Vector2(data.PositionX, data.PositionY);
			if (player.RuntimeRoot != null) player.RuntimeRoot.transform.position = player.Position;
			this.GetSystem<RunTimerSystem>().Restore(data.ElapsedSeconds, data.CurrentStageIndex);
			experience.Level = Mathf.Max(1, data.Level);
			experience.CurrentExperience = Mathf.Max(0, data.CurrentExperience);
			experience.RequiredExperience = Mathf.Max(1, data.RequiredExperience);

			var loadout = this.GetSystem<PlayerLoadoutSystem>();
			loadout.Reset();
			this.GetModel<PlayerLoadoutModel>().BindOwner(player.RuntimeRoot);
			loadout.SetDodge(data.DodgeId);
			var dodge = this.GetModel<DodgeModel>().Runtime;
			if (dodge != null) dodge.Level = Mathf.Max(1, data.DodgeLevel);
			var restoredWeaponCount = 0;
			foreach (var weapon in data.Weapons ?? new List<WeaponSaveData>())
			{
				var runtime = loadout.EquipWeapon(player.RuntimeRoot, weapon.WeaponId)
					? this.GetModel<PlayerLoadoutModel>().Weapons[this.GetModel<PlayerLoadoutModel>().Weapons.Count - 1]
					: null;
				if (runtime == null || !loadout.RestoreWeapon(runtime, weapon))
				{
					if (runtime != null) loadout.RemoveWeapon(runtime.RuntimeId);
					Debug.LogWarning($"Saved weapon {weapon.WeaponId} was skipped during restore.");
					continue;
				}
				restoredWeaponCount++;
			}
			RestoreSkills(loadout, data, player.RuntimeRoot);
			return data.Weapons == null || data.Weapons.Count == 0 || restoredWeaponCount > 0;
		}

		private void RestoreSkills(PlayerLoadoutSystem loadout, RunSaveData data, GameObject owner)
		{
			if (data.HasSkillSnapshot)
			{
				foreach (var skill in data.Skills ?? new List<SkillSaveData>()) RestoreSkill(loadout, owner, skill);
				return;
			}

			var character = this.GetUtility<CharacterCatalog>().Get(data.CharacterId);
			var skillGroup = character == null ? null : this.GetUtility<SkillGroupCatalog>().Get(character.SkillGroupId);
			foreach (var skillId in skillGroup?.StartingSkillIds ?? new List<int>())
				RestoreSkill(loadout, owner, new SkillSaveData { SkillId = skillId, Level = 1 });
		}

		private void RestoreSkill(PlayerLoadoutSystem loadout, GameObject owner, SkillSaveData data)
		{
			if (data == null || data.SkillId == 0 || !loadout.EquipSkill(owner, data.SkillId))
			{
				if (data != null && data.SkillId != 0) Debug.LogWarning($"Saved skill {data.SkillId} was skipped during restore.");
				return;
			}
			foreach (var skill in this.GetModel<PlayerLoadoutModel>().Skills)
				if (skill.SkillId == data.SkillId) skill.Level = Mathf.Max(1, data.Level);
		}

		protected override void OnInit()
		{
		}
	}
}
