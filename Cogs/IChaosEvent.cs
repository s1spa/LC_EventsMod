namespace LCChaosMod.Cogs
{
 
    //Базовий інтерфейс для кожного хаос-евенту.
    //Кожен евент — окрема папка в Cogs/.

    public interface IChaosEvent
    {
        //Назва евенту (показується гравцю в HUD)
        string GetName();

        //Чи увімкнений евент в налаштуваннях.
        bool IsEnabled();

        // Виконати евент.
        void Execute();

        // Чи показувати HUD-попередження за 5 секунд до евенту.
        bool ShowWarning() => true;
    }
}
 