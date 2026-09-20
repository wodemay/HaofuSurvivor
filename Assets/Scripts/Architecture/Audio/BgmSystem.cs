using UnityEngine;
using QFramework;

namespace HaoFuSurvivor
{
	public class BgmSystem : AbstractSystem
	{
		private AudioClip mClip;
		private AudioSource mSource;
		private bool mMusicPaused;

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
			mSource = System.Array.Find(Object.FindObjectsOfType<AudioSource>(true), source => source.clip == clip && source.loop);
			mMusicPaused = false;
			SetPitch(0f);
		}

		protected override void OnInit()
		{
			this.RegisterEvent<RunStartedEvent>(_ => PlayFromStart(mClip));
			this.RegisterEvent<RunTimerUpdatedEvent>(OnRunTimerUpdated);
			this.RegisterEvent<RunTimerPauseChangedEvent>(OnPauseChanged);
		}

		private void OnPauseChanged(RunTimerPauseChangedEvent pauseEvent)
		{
			if (mClip == null) return;
			var source = mSource;
			if (source == null || source.clip != mClip) return;

			if (pauseEvent.IsPaused)
			{
				AudioKit.PauseMusic();
				mMusicPaused = true;
			}
			else if (mMusicPaused)
			{
				// AudioKit.ResumeMusic calls Play, so preserve the playback cursor.
				var samples = source.timeSamples;
				AudioKit.ResumeMusic();
				source.timeSamples = samples;
				mMusicPaused = false;
			}
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
