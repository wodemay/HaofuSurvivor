/****************************************************************************
 * Copyright (c) 2015 - 2024 liangxiegame UNDER MIT License
 *
 * http://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 ****************************************************************************/

using System;
using UnityEngine;

namespace QFramework
{
    public class LocaleKit : Architecture<LocaleKit>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void AutoInit()
        {
            // 如果创建了 Config
            // if config is created
            if (Config)
            {
                InitArchitecture();
            }
        }

        public static void ReInit()
        {
            (Interface as LocaleKit)?.Init();
        }

        protected override void Init()
        {
            var languageIndex = LoadCommand(0);

            if (languageIndex >= Config.LanguageDefines.Count)
            {
                languageIndex = 0;
            }

            mCurrentLanguage.Value = Config.LanguageDefines[languageIndex].Language;
        }

        private static readonly BindableProperty<Language> mCurrentLanguage = new BindableProperty<Language>(Language.English);
        public static IReadonlyBindableProperty<Language> CurrentLanguage => mCurrentLanguage ;

        public static Action<int> SaveCommand = languageIndex => RuntimeSettingsStorage.SetInt("Locale.CurrentLanguageIndex", languageIndex);

        public static Func<int, int> LoadCommand = defaultLanguageIndex => RuntimeSettingsStorage.GetInt("Locale.CurrentLanguageIndex", defaultLanguageIndex);


        private static LanguageDefineConfig mConfig;

        public static LanguageDefineConfig Config =>
            mConfig ? mConfig : mConfig = Resources.Load<LanguageDefineConfig>(nameof(LanguageDefineConfig));


        public static Language GetNextLanguage()
        {
            var languageIndex = Config.LanguageDefines.FindIndex(l => l.Language == CurrentLanguage.Value);
            languageIndex++;

            if (languageIndex >= Config.LanguageDefines.Count)
            {
                languageIndex = 0;
            }

            return Config.LanguageDefines[languageIndex].Language;
        }

        public static void ChangeLanguage(Language language)
        {
            mCurrentLanguage.Value = language;
            SaveCommand(Config.LanguageDefines.FindIndex(l => l.Language == language));
        }
    }
}
