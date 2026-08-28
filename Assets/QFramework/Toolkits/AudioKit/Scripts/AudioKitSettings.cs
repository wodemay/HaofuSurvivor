/****************************************************************************
 * Copyright (c) 2015 - 2025 liangxiegame UNDER MIT LICENSE
 *
 * https://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 * AudioKit v1.0: use QFramework.cs architecture
 ****************************************************************************/

namespace QFramework
{
    /// <summary>
    /// TODO maybe support custom settings storage later
    /// </summary>
    public class AudioKitSettingsModel : AbstractModel
    {
        public RuntimeBooleanProperty IsSoundOn { get; private set; }

        public RuntimeBooleanProperty IsMusicOn { get; private set; }

        public RuntimeBooleanProperty IsVoiceOn { get; private set; }

        public RuntimeFloatProperty SoundVolume { get; private set; }

        public RuntimeFloatProperty MusicVolume { get; private set; }

        public RuntimeFloatProperty VoiceVolume { get; private set; }

        public CustomProperty<bool> IsOn { get; private set; }
        protected override void OnInit()
        {
            IsSoundOn = new RuntimeBooleanProperty("Audio.SoundOn", true);

            IsMusicOn = new RuntimeBooleanProperty("Audio.MusicOn", true);

            IsVoiceOn = new RuntimeBooleanProperty("Audio.VoiceOn", true);


            IsOn = new CustomProperty<bool>(
                () => IsSoundOn.Value && IsMusicOn.Value && IsVoiceOn.Value,
                isOn =>
                {
                    IsSoundOn.Value = isOn;
                    IsMusicOn.Value = isOn;
                    IsVoiceOn.Value = isOn;
                }
            );

            SoundVolume = new RuntimeFloatProperty("Audio.SoundVolume", 1.0f);

            MusicVolume = new RuntimeFloatProperty("Audio.MusicVolume", 1.0f);

            VoiceVolume = new RuntimeFloatProperty("Audio.VoiceVolume", 1.0f);
        }
    }
}
