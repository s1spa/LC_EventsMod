using System.Collections.Generic;

namespace LCChaosMod
{
    public static class Loc
    {
        public static string Lang { get; private set; } = "EN";

        public static void SetLang(string lang) =>
            Lang = (lang == "UA") ? "UA" : "EN";

        public static string Get(string key) =>
            _t.TryGetValue(key, out var row) && row.TryGetValue(Lang, out var s) ? s : key;

        public static void Register(string key, string en, string ua) =>
            _t[key] = new Dictionary<string, string> { ["EN"] = en, ["UA"] = ua };

        private static readonly Dictionary<string, Dictionary<string, string>> _t = new()
        {
            ["ui.title"]        = T("Chaos Mod Settings",     "Налаштування Chaos Mod"),
            ["ui.lang"]         = T("Language",               "Мова"),
            ["ui.min_interval"] = T("Minimum Event Interval", "Мінімальний інтервал"),
            ["ui.max_interval"] = T("Maximum Event Interval", "Максимальний інтервал"),
            ["ui.save"]         = T("SAVE & CLOSE",           "ЗБЕРЕГТИ"),
            ["ui.cancel"]       = T("CANCEL",                 "СКАСУВАТИ"),
            ["ui.events"]       = T("Events",                 "Події"),
        };

        private static Dictionary<string, string> T(string en, string ua) =>
            new() { ["EN"] = en, ["UA"] = ua };
    }
}
