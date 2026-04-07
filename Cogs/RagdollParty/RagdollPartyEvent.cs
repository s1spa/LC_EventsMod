using System.Collections.Generic;
using GameNetcodeStuff;
using LCChaosMod.Utils;
using Unity.Netcode;
using UnityEngine;

namespace LCChaosMod.Cogs.RagdollParty
{
    public class RagdollPartyEvent : IChaosEvent
    {
        public string GetName()   => Loc.Get("event.ragdoll_party");
        public bool   IsEnabled() => ChaosSettings.EnableRagdollParty.Value;

        public void Execute()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Plugin.Log.LogInfo("[RagdollPartyEvent] Skipped - not host.");
                return;
            }

            var eligible = new List<PlayerControllerB>();
            foreach (var p in StartOfRound.Instance.allPlayerScripts)
                if (p.isPlayerControlled && !p.isPlayerDead && !p.isInHangarShipRoom)
                    eligible.Add(p);

            if (eligible.Count == 0)
            {
                Plugin.Log.LogInfo("[RagdollPartyEvent] No eligible players.");
                return;
            }

            var target = eligible[Random.Range(0, eligible.Count)];
            Plugin.Log.LogInfo($"[RagdollPartyEvent] Tripping {target.playerUsername}.");
            Net.Broadcast(target.actualClientId);
        }
    }
}
