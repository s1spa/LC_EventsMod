namespace LCChaosMod.Cogs.MineSpawner
{
    internal static class Lang
    {
        internal static void Init()
        {
            Loc.Register("event.mines",       "💥 Mines spawned nearby!", "💥 Міни з'явились поруч!");
            Loc.Register("ui.mine_count_min", "Mines count min",          "Мін мін");
            Loc.Register("ui.mine_count_max", "Mines count max",          "Макс мін");
            Loc.Register("ui.mine_rate_min",  "Spawn interval min",       "Інтервал спавну мін");
            Loc.Register("ui.mine_rate_max",  "Spawn interval max",       "Інтервал спавну макс");
        }
    }
}
