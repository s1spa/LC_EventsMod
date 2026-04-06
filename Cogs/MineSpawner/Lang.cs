namespace LCChaosMod.Cogs.MineSpawner
{
    internal static class Lang
    {
        internal static void Init()
        {
            Loc.Register("event.mines",       "What kind of squeak was that..", "Що це був за писк..");
            Loc.Register("ui.mine_count_min", "Mines count min",          "Мінімум мін");
            Loc.Register("ui.mine_count_max", "Mines count max",          "Максимум мін");
            Loc.Register("ui.mine_rate_min",  "Spawn interval min",       "Інтервал спавну мін");
            Loc.Register("ui.mine_rate_max",  "Spawn interval max",       "Інтервал спавну макс");
        }
    }
}
