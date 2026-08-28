using System.Collections.Generic;
using System.Globalization;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class PickupSystem : AbstractSystem, IRunUpdateable
	{
		private readonly List<ActivePickup> mPickups = new();

		public void SpawnFromTable(int tableId, Vector2 position)
		{
			var table = this.GetUtility<DropTableCatalog>().Get(tableId);
			var entry = PickEntry(table);
			var controller = PickupFactory.Instance.Spawn(tableId, entry, position);
			if (controller == null || entry == null) return;
			mPickups.Add(new ActivePickup(tableId, entry.Id, controller));
			this.GetSystem<GameLoopSystem>().RegisterUpdateable(this);
		}

		public IEnumerable<PickupSaveData> GetSaveData()
		{
			foreach (var pickup in mPickups)
				if (pickup.Controller != null)
					yield return new PickupSaveData
					{
						TableId = pickup.TableId,
						EntryId = pickup.EntryId,
						PositionX = pickup.Controller.transform.position.x,
						PositionY = pickup.Controller.transform.position.y,
						IsCaptured = pickup.IsCaptured,
						AbsorbSpeed = pickup.AbsorbSpeed
					};
		}

		public void Restore(IEnumerable<PickupSaveData> entries)
		{
			Clear();
			if (entries == null) return;
			foreach (var entry in entries)
			{
				var table = entry == null ? null : this.GetUtility<DropTableCatalog>().Get(entry.TableId);
				var drop = table?.Get(entry.EntryId);
				var controller = PickupFactory.Instance.Spawn(entry.TableId, drop, new Vector2(entry.PositionX, entry.PositionY));
				if (controller == null) continue;
				mPickups.Add(new ActivePickup(entry.TableId, entry.EntryId, controller) { IsCaptured = entry.IsCaptured, AbsorbSpeed = Mathf.Max(0f, entry.AbsorbSpeed) });
			}
			if (mPickups.Count > 0 && this.GetSystem<RunTimerSystem>().IsRunning()) this.GetSystem<GameLoopSystem>().RegisterUpdateable(this);
		}

		public void OnRunUpdate(float deltaTime)
		{
			var player = this.GetModel<PlayerModel>();
			if (!player.IsRegistered || player.IsDead) return;
			var stats = this.GetModel<PlayerStatModel>();
			for (var i = mPickups.Count - 1; i >= 0; i--)
			{
				var pickup = mPickups[i];
				if (pickup.Controller == null) { mPickups.RemoveAt(i); continue; }
				var distance = Vector2.Distance(pickup.Controller.transform.position, player.Position);
				if (!pickup.IsCaptured)
				{
					if (distance > stats.ExperienceAbsorbRadius) continue;
					pickup.IsCaptured = true;
				}
				if (distance <= 0.1f)
				{
					Collect(pickup);
					mPickups.RemoveAt(i);
					continue;
				}
				pickup.AbsorbSpeed = Mathf.Min(stats.ExperienceAbsorbMaxSpeed, pickup.AbsorbSpeed + stats.ExperienceAbsorbAcceleration * deltaTime);
				pickup.Controller.MoveTowards(player.Position, pickup.AbsorbSpeed * deltaTime);
			}
			if (mPickups.Count == 0) this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
		}

		public void Clear()
		{
			PickupFactory.Instance.ReleaseAll();
			mPickups.Clear();
			this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
		}

		private void OnBreakableDestroyed(BreakableObjectDestroyedEvent e) => SpawnFromTable(e.DropTableId, e.Position);

		private void Collect(ActivePickup pickup)
		{
			var table = this.GetUtility<DropTableCatalog>().Get(pickup.TableId);
			var entry = table?.Get(pickup.EntryId);
			if (entry == null) return;
			switch (entry.Type)
			{
				case PickupType.Coin:
					this.GetSystem<RunEconomySystem>().AddCoins(new BigCoin(string.IsNullOrEmpty(entry.Amount) ? "0" : entry.Amount));
					break;
				case PickupType.Health:
					if (float.TryParse(entry.Amount, NumberStyles.Float, CultureInfo.InvariantCulture, out var health)) this.GetSystem<PlayerSystem>().RestoreHealth(health);
					break;
				case PickupType.ExperienceAbsorb:
					this.GetSystem<ExperienceSystem>().CaptureAll();
					break;
			}
			PickupFactory.Instance.Release(pickup.Controller);
		}

		private static DropEntry PickEntry(DropTableConfig table)
		{
			if (table == null) return null;
			var total = 0f;
			foreach (var entry in table.Entries) if (entry != null) total += Mathf.Max(0f, entry.Weight);
			if (total <= 0f) return null;
			var value = Random.value * total;
			foreach (var entry in table.Entries)
			{
				if (entry == null) continue;
				value -= Mathf.Max(0f, entry.Weight);
				if (value <= 0f) return entry;
			}
			return table.Entries.Find(entry => entry != null);
		}

		protected override void OnInit()
		{
			this.RegisterEvent<RunEndedEvent>(_ => Clear());
			this.RegisterEvent<BreakableObjectDestroyedEvent>(OnBreakableDestroyed);
		}

		private sealed class ActivePickup
		{
			public readonly int TableId;
			public readonly int EntryId;
			public readonly PickupController Controller;
			public bool IsCaptured;
			public float AbsorbSpeed;

			public ActivePickup(int tableId, int entryId, PickupController controller)
			{
				TableId = tableId;
				EntryId = entryId;
				Controller = controller;
			}
		}
	}
}
