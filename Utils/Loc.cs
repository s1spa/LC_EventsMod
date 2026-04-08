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
            ["ui.enabled"]      = T("Enabled",               "Увімкнено"),
            ["ui.disabled"]     = T("Disabled",              "Вимкнено"),

            // ── Settings UI ──────────────────────────────────────────────
            ["ui.s.lang"]             = T("LANGUAGE",              "МОВА"),
            ["ui.s.min_interval"]     = T("MIN INTERVAL (sec)",    "МІН ІНТЕРВАЛ (сек)"),
            ["ui.s.max_interval"]     = T("MAX INTERVAL (sec)",    "МАКС ІНТЕРВАЛ (сек)"),
            // Accordion headers
            ["ui.s.mines"]            = T("WHAT'S THAT SQUEAK?",   "ЩО ЦЕ ЗА ПИСК?"),
            ["ui.s.turrets"]          = T("DON'T MOVE!",           "НЕ РУХАЙСЯ!"),
            ["ui.s.size_matters"]     = T("SIZE MATTERS",          "РОЗМІР МАЄ ЗНАЧЕННЯ"),
            ["ui.s.berserk_turret"]   = T("TURRETS GOT RABIES!",   "У ТУРЕЛІ ЗНАЙШЛИ СКАЗ!"),
            ["ui.s.football"]         = T("FOOTBALL",              "ФУТБОЛ"),
            ["ui.s.stamina"]          = T("ADRENALINE",            "АДРЕНАЛІН"),
            // Simple toggles
            ["ui.s.mob_spawn"]        = T("HUNTING",               "ПОЛЮВАННЯ"),
            ["ui.s.teleport_dungeon"] = T("WHERE AM I?",           "ДЕ Я? ХТО Я?"),
            ["ui.s.teleport_ship"]    = T("LUNCH BREAK!",          "ОБІДНЯ ПЕРЕРВА!"),
            ["ui.s.random_sound"]     = T("SOMETHING'S WATCHING..", "ЩОСЬ СТЕЖИТЬ ЗА МНОЮ.."),
            ["ui.s.firefly"]          = T("FIREFLY",               "СВІТЛЯЧОК"),
            ["ui.s.player_swap"]      = T("POSITIONING ERROR",     "ПОМИЛКА ПОЗИЦІОНУВАННЯ"),
            ["ui.s.fake_message"]     = T("COMPANY MESSAGE",       "ПОВІДОМЛЕННЯ КОМПАНІЇ"),
            // Accordion content
            ["ui.s.enable"]           = T("Enable",                "Увімкнути"),
            ["ui.s.min_count"]        = T("Min Count",             "Мін. кількість"),
            ["ui.s.max_count"]        = T("Max Count",             "Макс. кількість"),
            ["ui.s.rate_min"]         = T("Rate Min (s)",          "Частота мін. (с)"),
            ["ui.s.rate_max"]         = T("Rate Max (s)",          "Частота макс. (с)"),
            ["ui.s.duration"]         = T("Duration (s)",          "Тривалість (с)"),
            ["ui.s.shrink_scale"]     = T("Shrink Scale",          "Масштаб зменшення"),
            ["ui.s.stretch_scale"]    = T("Stretch Scale (Y)",     "Масштаб розтягу (Y)"),
        };

        private static Dictionary<string, string> T(string en, string ua) =>
            new() { ["EN"] = en, ["UA"] = ua };
    }
}
