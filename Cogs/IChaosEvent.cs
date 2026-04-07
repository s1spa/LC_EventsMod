namespace LCChaosMod.Cogs
{
    /// <summary>
    /// Базовий інтерфейс для кожного хаос-евенту.
    /// Кожен евент — окрема папка в Cogs/.
    /// </summary>
    public interface IChaosEvent
    {
        /// <summary>Назва евенту (показується гравцю в HUD).</summary>
        string GetName();

        /// <summary>Чи увімкнений евент в налаштуваннях.</summary>
        bool IsEnabled();

        /// <summary>Виконати евент.</summary>
        void Execute();

        /// <summary>Чи показувати HUD-попередження за 5 секунд до евенту.</summary>
        bool ShowWarning() => true;
    }
}
 