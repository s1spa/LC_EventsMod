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

        // Turrets — налаштування евенту
        public static ConfigEntry<int>   TurretCountMin { get; private set; } = null!;
        public static ConfigEntry<int>   TurretCountMax { get; private set; } = null!;
        public static ConfigEntry<float> TurretRateMin  { get; private set; } = null!;
        public static ConfigEntry<float> TurretRateMax  { get; private set; } = null!;

        // Які евенти увімкнені
        public static ConfigEntry<bool> EnableMines { get; private set; } = null!;
        public static ConfigEntry<bool> EnableTurrets { get; private set; } = null!;
        public static ConfigEntry<bool> EnableMobSpawn { get; private set; } = null!;
        public static ConfigEntry<bool> EnableTeleportDungeon { get; private set; } = null!;
        public static ConfigEntry<bool> EnableTeleportShip { get; private set; } = null!;
        public static ConfigEntry<bool> EnablePlayerSwap { get; private set; } = null!;
        public static ConfigEntry<bool> EnableGlowstick   { get; private set; } = null!;
        public static ConfigEntry<bool> EnableRandomSound { get; private set; } = null!;
        public static ConfigEntry<bool>  EnableInfiniteStamina { get; private set; } = null!;
        public static ConfigEntry<float> StaminaDuration       { get; private set; } = null!;
        public static ConfigEntry<bool>  EnableBerserkTurret   { get; private set; } = null!;
        public static ConfigEntry<float> BerserkDuration       { get; private set; } = null!;
        public static ConfigEntry<bool>  EnableFootball        { get; private set; } = null!;
        public static ConfigEntry<float> FootballDuration      { get; private set; } = null!;
        public static ConfigEntry<bool>  EnableFakeMessage     { get; private set; } = null!;
        public static ConfigEntry<bool>  EnableSizeMatters     { get; private set; } = null!;
        public static ConfigEntry<float> SizeScale             { get; private set; } = null!;
        public static ConfigEntry<float> SizeStretchScale      { get; private set; } = null!;
        public static ConfigEntry<float> SizeDuration          { get; private set; } = null!;

        public static void Init(ConfigFile config)
        {
            ModEnabled = config.Bind("General", "Enabled", true, "Вмикає/вимикає мод повністю");
            Language = config.Bind("General", "Language", "EN", "Мова повідомлень: EN / UA");

            MinInterval = config.Bind("Timing", "MinInterval", 20f,  "Мінімальний інтервал між евентами (секунди)");
            MaxInterval = config.Bind("Timing", "MaxInterval", 120f,  "Максимальний інтервал між евентами (секунди)");

            Difficulty = config.Bind("Difficulty", "Level", 2, "Складність: 1 (легко) — 3 (хаос)");

            MineCountMin = config.Bind("Mines", "CountMin", 5,    "Мінімальна кількість мін за евент");
            MineCountMax = config.Bind("Mines", "CountMax", 10,   "Максимальна кількість мін за евент");
            MineRateMin  = config.Bind("Mines", "RateMin",  1f,   "Мінімальний інтервал між мінами (с)");
            MineRateMax  = config.Bind("Mines", "RateMax",  3f,   "Максимальний інтервал між мінами (с)");

            TurretCountMin = config.Bind("Turrets", "CountMin", 2,   "Мінімальна кількість турелей за евент");
            TurretCountMax = config.Bind("Turrets", "CountMax", 5,   "Максимальна кількість турелей за евент");
            TurretRateMin  = config.Bind("Turrets", "RateMin",  1f,  "Мінімальний інтервал між турелями (с)");
            TurretRateMax  = config.Bind("Turrets", "RateMax",  3f,  "Максимальний інтервал між турелями (с)");

            EnableMines = config.Bind("Events", "Mines", true, "Міни під ногами");
            EnableTurrets = config.Bind("Events", "Turrets", true, "Турелі навколо");
            EnableMobSpawn = config.Bind("Events", "MobSpawn", true, "Спавн рандомного моба");
            EnableTeleportDungeon = config.Bind("Events", "TeleportDungeon", true, "Рандомна телепортація по данжу");
            EnableTeleportShip = config.Bind("Events", "TeleportShip", true, "Телепортація на корабель");
            EnablePlayerSwap = config.Bind("Events", "PlayerSwap", true, "Підміна гравців місцями");
            EnableGlowstick   = config.Bind("Events", "Glowstick",    true, "Світіння при підйомі Apparatus");
            EnableRandomSound     = config.Bind("Events", "RandomSound",     true, "Рандомний звук моба поруч");
            EnableInfiniteStamina = config.Bind("Events", "InfiniteStamina",   true,  "Нескінченна витривалість");
            StaminaDuration       = config.Bind("Events", "StaminaDuration",   10f,   "Тривалість нескінченної витривалості (секунди)");
            EnableBerserkTurret   = config.Bind("Events", "BerserkTurret",     true,  "Турелі збожеволіли");
            BerserkDuration       = config.Bind("Events", "BerserkDuration",   10f,   "Тривалість берсерку турелі (секунди)");
            EnableFootball        = config.Bind("Events", "Football",           true,  "Час футболу");
            FootballDuration      = config.Bind("Events", "FootballDuration",   30f,  "Тривалість футбольного режиму (секунди)");
            EnableFakeMessage     = config.Bind("Events", "FakeMessage",        true,  "Фейкове повідомлення компанії");
            EnableSizeMatters     = config.Bind("Events", "SizeMatters",        true,  "Зміна розміру гравця");
            SizeScale             = config.Bind("Events", "SizeScale",          0.4f,  "Розмір маленького гравця (0.1 - 1.0)");
            SizeStretchScale      = config.Bind("Events", "SizeStretchScale",   1.8f,  "Висота великого гравця (1.0 - 3.0)");
            SizeDuration          = config.Bind("Events", "SizeDuration",       15f,   "Тривалість зміни розміру (секунди)");
        }
    }
}
