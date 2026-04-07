using LCChaosMod.Utils;
using Unity.Netcode;

namespace LCChaosMod.Cogs.FakeMessage
{
    public class FakeMessageEvent : IChaosEvent
    {
        public string GetName()    => Loc.Get("event.fake_message");
        public bool   IsEnabled()  => ChaosSettings.EnableFakeMessage.Value;
        public bool   ShowWarning() => false;

        public void Execute()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Plugin.Log.LogInfo("[FakeMessageEvent] Skipped - not host.");
                return;
            }

            Plugin.Log.LogInfo("[FakeMessageEvent] Broadcasting fake company message.");
            Net.Broadcast();
        }
    }
}
