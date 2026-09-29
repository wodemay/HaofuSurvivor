using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class CombatAudioView : MonoBehaviour, IController
	{
		public AudioSource[] Voices;
		private int mNext;
		private bool mPaused;
		public IArchitecture GetArchitecture() => GameArchitecture.Interface;
		public void Play(AudioClip clip)
		{
			if (Voices == null || Voices.Length == 0 || clip == null) return;
			var voice = Voices[mNext++ % Voices.Length];
			voice.Stop(); voice.clip = clip; voice.Play();
		}
		private void Update()
		{
			var paused = !this.SendQuery(new GetRunTimeStateQuery()).IsRunning;
			if (paused == mPaused) return;
			mPaused = paused;
			foreach (var voice in Voices) { if (paused) voice.Pause(); else voice.UnPause(); }
		}
		private void OnDisable()
		{
			if (Voices != null) foreach (var voice in Voices) if (voice != null) voice.Stop();
			mPaused = false; mNext = 0;
		}
	}
}
