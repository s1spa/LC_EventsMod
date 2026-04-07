using System.Collections.Generic;
using LCChaosMod.Utils;
using Unity.Netcode;
using UnityEngine;

namespace LCChaosMod.Cogs.SizeMatters
{
    public class SizeMattersEvent : IChaosEvent
    {

        public string GetName()   => Loc.Get("event.size_matters");
        public bool   IsEnabled() => ChaosSettings.EnableSizeMatters.Value;

        public void Execute()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Plugin.Log.LogInfo("[SizeMattersEvent] Skipped - not host.");
                return;
            }

            var eligible = new List<GameNetcodeStuff.PlayerControllerB>();
            foreach (var p in StartOfRound.Instance.allPlayerScripts)
                if (p.isPlayerControlled && !p.isPlayerDead && !p.isInHangarShipRoom
                    && !Net.IsActive(p.playerClientId)) // Змінено на playerClientId
                    eligible.Add(p);

            if (eligible.Count == 0)
            {
                Plugin.Log.LogInfo("[SizeMattersEvent] No eligible players (all active or none).");
                return;
            }

            float dur = ChaosSettings.SizeDuration.Value;

            // Pick shrink target
            int shrinkIdx = Random.Range(0, eligible.Count);
            var shrinkTarget = eligible[shrinkIdx];
            Plugin.Log.LogInfo($"[SizeMattersEvent] Shrinking {shrinkTarget.playerUsername}.");
            Net.Broadcast(shrinkTarget.playerClientId, ChaosSettings.SizeScale.Value, dur); // Змінено на playerClientId

            // Pick a different player for stretch (if available)
            if (eligible.Count >= 2)
            {
                eligible.RemoveAt(shrinkIdx);
                var stretchTarget = eligible[Random.Range(0, eligible.Count)];
                Plugin.Log.LogInfo($"[SizeMattersEvent] Stretching {stretchTarget.playerUsername}.");
                Net.BroadcastStretch(stretchTarget.playerClientId, ChaosSettings.SizeStretchScale.Value, dur); // Змінено на playerClientId
            }
        }
    }
}