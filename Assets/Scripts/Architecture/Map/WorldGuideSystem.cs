using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class WorldGuideSystem : AbstractSystem, IRunUpdateable
	{
		private readonly Dictionary<string, WorldGuideItemView> mViews = new();
		private readonly List<string> mStaleIds = new();

		public void Reset()
		{
			foreach (var view in mViews.Values)
				if (view != null) Object.Destroy(view.gameObject);
			mViews.Clear();
		}

		public void OnRunUpdate(float deltaTime)
		{
			var player = this.GetModel<PlayerModel>();
			var container = WorldRootLocator.Get(WorldRootSlot.WorldUI);
			if (container == null || !player.IsRegistered || player.IsDead) return;
			mStaleIds.Clear();
			foreach (var id in mViews.Keys) mStaleIds.Add(id);
			var states = GameArchitecture.Interface.SendQuery(new GetMapEventsQuery());
			foreach (var state in states)
			{
				if (state.IsCompleted || !TryGetConfig(state.ConfigId, out var config) || config.GuidePrefab == null)
				{
					mStaleIds.Add(state.StableId);
					continue;
				}
				if (!mViews.TryGetValue(state.StableId, out var view) || view == null)
				{
					var instance = Object.Instantiate(config.GuidePrefab, container);
					view = instance.GetComponent<WorldGuideItemView>() ?? instance.AddComponent<WorldGuideItemView>();
					view.Configure(config.GuideIcon);
					mViews[state.StableId] = view;
				}
				mStaleIds.Remove(state.StableId);
				var distance = Vector2.Distance(player.Position, state.Position);
				var scale = Mathf.Clamp(config.GuideMaxScale - distance * config.GuideScaleFalloff,
					config.GuideMinScale, config.GuideMaxScale);
				view.SetTarget(state.Position, scale);
			}
			foreach (var id in mStaleIds)
			{
				if (!mViews.TryGetValue(id, out var view)) continue;
				if (view != null) Object.Destroy(view.gameObject);
				mViews.Remove(id);
			}
		}

		private bool TryGetConfig(int configId, out MapEventConfig config)
		{
			config = this.GetUtility<MapEventCatalog>().Get(configId);
			return config != null;
		}

		protected override void OnInit()
		{
			this.RegisterEvent<RunEndedEvent>(_ => Reset());
		}
	}
}
