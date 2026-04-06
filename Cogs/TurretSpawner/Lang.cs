namespace LCChaosMod.Cogs.TurretSpawner
{
    internal static class Lang
    {
        internal static void Init()
        {
            Loc.Register("event.turrets",          "Don't Move!", "Не рухайся!");
            Loc.Register("ui.turret_count_min",    "Turrets count min",               "Мін турелей");
            Loc.Register("ui.turret_count_max",    "Turrets count max",               "Макс турелей");
            Loc.Register("ui.turret_rate_min",     "Spawn interval min",              "Інтервал спавну мін");
            Loc.Register("ui.turret_rate_max",     "Spawn interval max",              "Інтервал спавну макс");
        }
    }
}
