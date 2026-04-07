namespace LCChaosMod.Cogs
{
    public class InfiniteStaminaEvent : IChaosEvent
    {
        public string GetName()   => Loc.Get("event.adrenaline");
        public bool   IsEnabled() => ChaosSettings.EnableInfiniteStamina.Value;

        public void Execute()
        {
            InfiniteStamina.Net.Broadcast(ChaosSettings.StaminaDuration.Value);
        }
    }
}
