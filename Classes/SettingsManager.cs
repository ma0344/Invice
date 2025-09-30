using System;
using System.Collections.Generic;
using System.IO;

namespace Invoice
{
    public static class SettingsManager
    {
        private static readonly string iniPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.ini");
        private static Dictionary<string, string>? _settings;

        private static void EnsureLoaded()
        {
            if (_settings != null) return;
            _settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(iniPath)) return;
            foreach (var line in File.ReadAllLines(iniPath))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;
                var idx = trimmed.IndexOf('=');
                if (idx > 0)
                {
                    var key = trimmed.Substring(0, idx).Trim();
                    var value = trimmed.Substring(idx + 1).Trim();
                    _settings[key] = value;
                }
            }
        }

        public static string? Get(string key)
        {
            EnsureLoaded();
            if (_settings != null && _settings.TryGetValue(key, out var value))
                return value;
            return null;
        }
    }
}
