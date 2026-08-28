using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace QFramework
{
    public class RuntimeBooleanProperty : BindableProperty<bool>
    {
        public RuntimeBooleanProperty(string key, bool defaultValue = false)
        {
            mValue = RuntimeSettingsStorage.GetBool(key, defaultValue);
            Register(value => RuntimeSettingsStorage.SetBool(key, value));
        }
    }
}

namespace QFramework
{
    public static class RuntimeSettingsStorage
    {
        private const string FileName = "Settings/qframework-settings.json";
        private static readonly object Sync = new object();
        private static SettingsData mData;

        public static bool GetBool(string key, bool defaultValue)
        {
            lock (Sync)
            {
                var entry = Find(key, "bool");
                return entry == null ? defaultValue : entry.BoolValue;
            }
        }

        public static void SetBool(string key, bool value)
        {
            lock (Sync)
            {
                var entry = GetOrCreate(key, "bool");
                entry.BoolValue = value;
                Save();
            }
        }

        public static float GetFloat(string key, float defaultValue)
        {
            lock (Sync)
            {
                var entry = Find(key, "float");
                return entry == null ? defaultValue : entry.FloatValue;
            }
        }

        public static void SetFloat(string key, float value)
        {
            lock (Sync)
            {
                var entry = GetOrCreate(key, "float");
                entry.FloatValue = value;
                Save();
            }
        }

        public static int GetInt(string key, int defaultValue)
        {
            lock (Sync)
            {
                var entry = Find(key, "int");
                return entry == null ? defaultValue : entry.IntValue;
            }
        }

        public static void SetInt(string key, int value)
        {
            lock (Sync)
            {
                var entry = GetOrCreate(key, "int");
                entry.IntValue = value;
                Save();
            }
        }

        private static SettingEntry GetOrCreate(string key, string type)
        {
            var entry = Find(key, type);
            if (entry == null)
            {
                entry = new SettingEntry { Key = key, Type = type };
                mData.Entries.Add(entry);
            }
            return entry;
        }

        private static SettingEntry Find(string key, string type)
        {
            EnsureLoaded();
            return mData.Entries.Find(entry => entry.Key == key && entry.Type == type);
        }

        private static void EnsureLoaded()
        {
            if (mData != null) return;
            mData = new SettingsData();
            var path = GetPath();
            if (path == null || !File.Exists(path)) return;
            try
            {
                mData = JsonUtility.FromJson<SettingsData>(File.ReadAllText(path)) ?? new SettingsData();
                if (mData.Entries == null) mData.Entries = new List<SettingEntry>();
            }
            catch
            {
                mData = new SettingsData();
            }
        }

        private static void Save()
        {
            var path = GetPath();
            if (path == null) return;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var temporaryPath = path + ".tmp";
                File.WriteAllText(temporaryPath, JsonUtility.ToJson(mData), new UTF8Encoding(false));
                if (File.Exists(path)) File.Replace(temporaryPath, path, null);
                else File.Move(temporaryPath, path);
            }
            catch
            {
            }
        }

        private static string GetPath()
        {
            var dataDirectory = new DirectoryInfo(Path.GetFullPath(Application.dataPath));
            var root = dataDirectory.Parent?.FullName ?? dataDirectory.FullName;
            if (string.IsNullOrEmpty(root)) return null;
            foreach (var character in root)
                if (character < 0x20 || character > 0x7E) return null;
            return Path.Combine(root, FileName.Replace('/', Path.DirectorySeparatorChar));
        }

        [Serializable]
        private class SettingsData
        {
            public List<SettingEntry> Entries = new List<SettingEntry>();
        }

        [Serializable]
        private class SettingEntry
        {
            public string Key;
            public string Type;
            public bool BoolValue;
            public int IntValue;
            public float FloatValue;
        }
    }
}
