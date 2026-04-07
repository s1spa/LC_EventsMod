using LCChaosMod.Utils;

namespace LCChaosMod.Cogs.SizeMatters
{
    internal static class Lang
    {
        public static void Init()
        {
            Loc.Register("event.size_matters",      "Size Matters!",        "Розмір має значення!");
            Loc.Register("ui.size_scale",           "Size scale",           "Розмір (маленький)");
            Loc.Register("ui.size_stretch_scale",   "Stretch scale",        "Висота (великий)");
            Loc.Register("ui.size_duration",        "Duration (seconds)",   "Тривалість (секунди)");
        }
    }
}
