using UnityEngine;

namespace HaoFuSurvivor
{
	[DefaultExecutionOrder(100)]
	public class WorldGuideItemView : MonoBehaviour
	{
		private SpriteRenderer mIcon;
		private Renderer[] mRenderers;
		private Vector2 mWorldTarget;
		private Vector3 mTargetPosition;
		private Quaternion mTargetRotation;
		private float mTargetScale = 1f;
		private bool mHasTarget;

		public void Configure(Sprite icon)
		{
			mRenderers = GetComponentsInChildren<Renderer>(true);
			mIcon = transform.Find("Image_Icon")?.GetComponent<SpriteRenderer>();
			if (mIcon != null && icon != null) mIcon.sprite = icon;
		}

		public void SetTarget(Vector2 worldTarget, float scale)
		{
			mWorldTarget = worldTarget;
			mTargetScale = scale;
			mHasTarget = true;
		}

		private void LateUpdate()
		{
			if (!mHasTarget) return;
			var camera = Camera.main;
			if (camera == null || !TryGetEdgePosition(camera, mWorldTarget, out var position, out var direction))
			{
				SetVisible(false);
				return;
			}

			SetVisible(true);
			var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
			mTargetPosition = position;
			mTargetRotation = Quaternion.Euler(0f, 0f, angle);
			var deltaTime = Time.unscaledDeltaTime;
			var positionLerp = 1f - Mathf.Exp(-32f * deltaTime);
			transform.position = Vector3.Lerp(transform.position, mTargetPosition, positionLerp);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, mTargetRotation, 1440f * deltaTime);
			transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * mTargetScale, 1f - Mathf.Exp(-24f * deltaTime));
		}

		private void SetVisible(bool visible)
		{
			if (mRenderers == null) mRenderers = GetComponentsInChildren<Renderer>(true);
			foreach (var renderer in mRenderers)
				if (renderer != null) renderer.enabled = visible;
		}

		private static bool TryGetEdgePosition(Camera camera, Vector2 target, out Vector3 worldPosition, out Vector2 direction)
		{
			var viewport = camera.WorldToViewportPoint(target);
			var center = new Vector2(0.5f, 0.5f);
			var targetViewport = new Vector2(viewport.x, viewport.y);
			const float margin = 0.08f;
			if (targetViewport.x >= margin && targetViewport.x <= 1f - margin && targetViewport.y >= margin && targetViewport.y <= 1f - margin)
			{
				worldPosition = default;
				direction = default;
				return false;
			}
			direction = targetViewport - center;
			if (direction.sqrMagnitude < 0.0001f) direction = Vector2.up;
			direction.Normalize();
			var scale = Mathf.Min((0.5f - margin) / Mathf.Max(0.0001f, Mathf.Abs(direction.x)),
				(0.5f - margin) / Mathf.Max(0.0001f, Mathf.Abs(direction.y)));
			var edgeViewport = center + direction * scale;
			var screen = new Vector3(edgeViewport.x * Screen.width, edgeViewport.y * Screen.height, Mathf.Abs(camera.transform.position.z));
			worldPosition = camera.ScreenToWorldPoint(screen);
			worldPosition.z = 0f;
			return true;
		}
	}
}
