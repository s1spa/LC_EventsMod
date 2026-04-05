using System.Collections.Generic;

namespace LCChaosMod
{
    /// <summary>
    /// Проста статична локалізація. Додай нові рядки в таблицю нижче.
    /// Використання: Loc.Get("key")
    /// </summary>
    public static class Loc
    {
        public static string Lang { get; private set; } = "EN";

        public static void SetLang(string lang) =>
            Lang = (lang == "UA") ? "UA" : "EN";

        public static string Get(string key) =>
            _t.TryGetValue(key, out var row) && row.TryGetValue(Lang, out var s) ? s : key;

        // ── Таблиця перекладів ───────────────────────────────────
        // Формат: ["ключ"] = { ["EN"] = "...", ["UA"] = "..." }
        private static readonly Dictionary<string, Dictionary<string, string>> _t = new()
        {
            // ── Settings overlay ─────────────────────────────────
            ["ui.title"]        = T("Chaos Mod Settings",       "Налаштування Chaos Mod"),
            ["ui.lang"]         = T("Language",                 "Мова"),
            ["ui.min_interval"] = T("Minimum Event Interval",   "Мінімальний інтервал"),
            ["ui.max_interval"] = T("Maximum Event Interval",   "Максимальний інтервал"),
            ["ui.severity"]     = T("Event Severity",           "Складність подій"),
            ["ui.save"]         = T("SAVE & CLOSE",             "ЗБЕРЕГТИ"),
            ["ui.cancel"]       = T("CANCEL",                   "СКАСУВАТИ"),

            // ── Складності ───────────────────────────────────────
            ["diff.1"]          = T("NOOB",      "НУБ"),
            ["diff.2"]          = T("WEAKLING",  "СЛАБАК"),
            ["diff.3"]          = T("NORMAL",    "НОРМАЛЬНО"),
            ["diff.4"]          = T("HARDCORE",  "ВАЖКОВАТО"),
            ["diff.5"]          = T("EASY MAYBE", "ЛЕГКО МАБУТЬ"),

            // ── Повідомлення про події (in-game HUD) ─────────────
            ["event.mines"]           = T("💥 Mines spawned nearby!",           "💥 Міни з'явились поруч!"),
            ["event.turrets"]         = T("🔫 Turrets deployed!",               "🔫 Турелі розставлені!"),
            ["event.mob"]             = T("👾 Something is hunting you...",     "👾 Щось полює на тебе..."),
            ["event.teleport_dungeon"]= T("🌀 Teleported to a random location!","🌀 Тебе телепортовано!"),
            ["event.teleport_ship"]   = T("🚀 Someone was sent to the ship!",  "🚀 Когось відправили на корабель!"),
            ["event.player_swap"]     = T("🔀 Players swapped positions!",      "🔀 Гравці помінялись місцями!"),
            ["event.glowstick"]       = T("💡 Glowstick rain!",                "💡 Дощ із ціпків!"),
        };

        private static Dictionary<string, string> T(string en, string ua) =>
            new() { ["EN"] = en, ["UA"] = ua };
    }
}
