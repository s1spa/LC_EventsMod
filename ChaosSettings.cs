using BepInEx.Configuration;

namespace LCChaosMod
{
    public static class ChaosSettings
    {
        // Загальні
        public static ConfigEntry<bool> ModEnabled { get; private set; } = null!;
        public static ConfigEntry<string> Language { get; private set; } = null!;

        // Таймінги
        public static ConfigEntry<float> MinInterval { get; private set; } = null!;
        public static ConfigEntry<float> MaxInterval { get; private set; } = null!;

        // Складність (впливає на кількість/силу евентів)
        public static ConfigEntry<int> Difficulty { get; private set; } = null!;

        // Mines — налаштування евенту
        public static ConfigEntry<int>   MineCountMin { get; private set; } = null!;
        public static ConfigEntry<int>   MineCountMax { get; private set; } = null!;
        public static ConfigEntry<float> MineRateMin  { get; private set; } = null!;
        public static ConfigEntry<float> MineRateMax  { get; private set; } = null!;

        // Які евенти увімкнені
        public static ConfigEntry<bool> EnableMines { get; private set; } = null!;
        public static ConfigEntry<bool> EnableTurrets { get; private set; } = null!;
        public static ConfigEntry<bool> EnableMobSpawn { get; private set; } = null!;
        public static ConfigEntry<bool> EnableTeleportDungeon { get; private set; } = null!;
        public static ConfigEntry<bool> EnableTeleportShip { get; private set; } = null!;
        public static ConfigEntry<bool> EnablePlayerSwap { get; private set; } = null!;
        public static ConfigEntry<bool> EnableGlowstick { get; private set; } = null!;

        public static void Init(ConfigFile config)
        {
            ModEnabled = config.Bind("General", "Enabled", true, "Вмикає/вимикає мод повністю");
            Language = config.Bind("General", "Language", "EN", "Мова повідомлень: EN / UA");

            MinInterval = config.Bind("Timing", "MinInterval", 15f,  "Мінімальний інтервал між евентами (секунди)");
            MaxInterval = config.Bind("Timing", "MaxInterval", 30f,  "Максимальний інтервал між евентами (секунди)");

            Difficulty = config.Bind("Difficulty", "Level", 2, "Складність: 1 (легко) — 3 (хаос)");

            MineCountMin = config.Bind("Mines", "CountMin", 5,    "Мінімальна кількість мін за евент");
            MineCountMax = config.Bind("Mines", "CountMax", 10,   "Максимальна кількість мін за евент");
            MineRateMin  = config.Bind("Mines", "RateMin",  1f,   "Мінімальний інтервал між мінами (с)");
            MineRateMax  = config.Bind("Mines", "RateMax",  3f,   "Максимальний інтервал між мінами (с)");

            EnableMines = config.Bind("Events", "Mines", true, "Міни під ногами");
            EnableTurrets = config.Bind("Events", "Turrets", true, "Турелі навколо");
            EnableMobSpawn = config.Bind("Events", "MobSpawn", true, "Спавн рандомного моба");
            EnableTeleportDungeon = config.Bind("Events", "TeleportDungeon", true, "Рандомна телепортація по данжу");
            EnableTeleportShip = config.Bind("Events", "TeleportShip", true, "Телепортація на корабель");
            EnablePlayerSwap = config.Bind("Events", "PlayerSwap", true, "Підміна гравців місцями");
            EnableGlowstick = config.Bind("Events", "Glowstick", true, "Світіння при підйомі Apparatus");
        }
    }
}
