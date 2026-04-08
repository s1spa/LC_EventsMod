using LCChaosMod.Cogs.Firefly;
using UnityEngine;

namespace LCChaosMod.Patches
{
    //Watches every frame if the local player is holding the Apparatus (LungProp).
    // Triggers the Firefly glow once on pickup.
    public class LungPropWatcher : MonoBehaviour
    {
        private bool _triggered;

        private void Update()
        {
            // Round ended — remove glow and reset
            if (StartOfRound.Instance != null && StartOfRound.Instance.inShipPhase)
            {
                if (_triggered)
                {
                    _triggered = false;
                    FireflyTracker.Cleanup();
                }
                return;
            }

            if (_triggered) return;
            if (!ChaosSettings.EnableGlowstick.Value) return;

            var player = GameNetworkManager.Instance?.localPlayerController;
            if (player == null) return;
            if (player.currentlyHeldObjectServer is not LungProp) return;

            _triggered = true;
            Plugin.Log.LogInfo("[LungPropWatcher] Local player grabbed Apparatus.");
            FireflyTracker.OnLocalPlayerGrabbed();
        }
    }
}
