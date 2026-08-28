using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class BreakableObjectSystem : AbstractSystem
	{
		private readonly Dictionary<string, BreakableObjectView> mActive = new();
		private readonly Dictionary<string, float> mHealth = new();

		public void LoadChunk(MapChunkData data)
		{
			if (data?.Breakables == null) return;
			var container = WorldRootLocator.Get(WorldRootSlot.MapDecoration);
			foreach (var entry in data.Breakables)
			{
				if (entry == null || entry.IsDestroyed || string.IsNullOrEmpty(entry.StableId) || mActive.ContainsKey(entry.StableId)) continue;
				var config = this.GetUtility<BreakableObjectCatalog>().Get(entry.ConfigId);
				if (config == null) continue;
				var position = new Vector2(entry.WorldCellX + 0.5f, entry.WorldCellY + 0.5f);
				var view = BreakableObjectFactory.Instance.Spawn(config, entry.StableId, position, container);
				if (view == null) continue;
				mActive[entry.StableId] = view;
				mHealth[entry.StableId] = Mathf.Max(1f, config.HitPoints);
			}
		}

		public void UnloadChunk(MapChunkData data)
		{
			if (data?.Breakables == null) return;
			foreach (var entry in data.Breakables)
			{
				if (entry == null || string.IsNullOrEmpty(entry.StableId) || !mActive.TryGetValue(entry.StableId, out var view)) continue;
				mActive.Remove(entry.StableId);
				mHealth.Remove(entry.StableId);
				BreakableObjectFactory.Instance.Release(view);
			}
		}

		public void ApplyDamage(BreakableObjectView view, float damage)
		{
			if (view == null || damage <= 0f || string.IsNullOrEmpty(view.StableId) || !mHealth.TryGetValue(view.StableId, out var health)) return;
			health -= 1f;
			mHealth[view.StableId] = health;
			if (health > 0f) return;

			var stableId = view.StableId;
			var config = view.Config;
			var position = view.transform.position;
			mActive.Remove(stableId);
			mHealth.Remove(stableId);
			this.GetSystem<MapSystem>().MarkBreakableDestroyed(stableId);
			BreakableObjectFactory.Instance.Release(view);
			this.SendEvent(new BreakableObjectDestroyedEvent(config == null ? 0 : config.Id,
				config == null ? 0 : config.DropTableId, position));
		}

		public void Reset()
		{
			BreakableObjectFactory.Instance.ReleaseAll();
			mActive.Clear();
			mHealth.Clear();
		}

		protected override void OnInit()
		{
			this.RegisterEvent<RunEndedEvent>(_ => Reset());
		}
	}
}
