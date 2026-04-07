using LCChaosMod.Utils;

namespace LCChaosMod.Cogs.BerserkTurret
{
    internal static class Lang
    {
        public static void Init()
        {
            Loc.Register("event.berserk_turret",  "Berserk Turrets",  "Турелі збожеволіли");
            Loc.Register("ui.berserk_duration",   "Duration (seconds)", "Тривалість (секунди)");
        }
    }
}
