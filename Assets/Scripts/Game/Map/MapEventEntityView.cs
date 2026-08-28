using UnityEngine;
using UnityEngine.UI;

namespace HaoFuSurvivor
{
	public class MapEventEntityView : MonoBehaviour
	{
		private Image mProgressImage;

		public void Configure(float triggerRadius)
		{
			mProgressImage = transform.Find("ProgressCanvas/Image_Progress")?.GetComponent<Image>();
			if (mProgressImage == null) mProgressImage = transform.Find("Image_Progress")?.GetComponent<Image>();
			var circle = GetComponent<CircleCollider2D>();
			if (circle == null) return;
			circle.isTrigger = true;
			circle.radius = Mathf.Max(0.5f, triggerRadius);
			SetProgress(0f);
		}

		public void SetProgress(float normalized)
		{
			if (mProgressImage != null) mProgressImage.fillAmount = Mathf.Clamp01(normalized);
		}
	}
}
