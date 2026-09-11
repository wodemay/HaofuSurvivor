using UnityEngine;
using QFramework;

namespace HaoFuSurvivor
{
	public class BgmSystem : AbstractSystem
	{
		private AudioClip mClip;

		public AudioClip CurrentClip => mClip;

		public void SetClip(AudioClip clip)
		{
			mClip = clip;
			if (clip == null) Debug.LogWarning("BGM clip is missing; music is disabled.");
		}

		public void PlayFromStart(AudioClip clip)
		{
			mClip = clip;
			if (clip == null) return;

			AudioKit.StopMusic();
			AudioKit.PlayMusic(clip);
			SetPitch(0f);
		}

		protected override void OnInit()
		{
			this.RegisterEvent<RunStartedEvent>(_ => PlayFromStart(mClip));
			this.RegisterEvent<RunTimerUpdatedEvent>(OnRunTimerUpdated);
		}

		private void OnRunTimerUpdated(RunTimerUpdatedEvent timerEvent)
		{
			if (mClip == null || !this.GetSystem<RunTimerSystem>().IsRunning()) return;
			SetPitch(this.GetModel<RunTimerModel>().ElapsedSeconds);
		}

		private void SetPitch(float elapsedSeconds)
		{
			AudioKit.MusicPlayer.Pitch(Mathf.Clamp(1f + elapsedSeconds / 1800f, 1f, 2f));
		}
	}
}
