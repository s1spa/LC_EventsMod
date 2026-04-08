using System.Collections.Generic;
using GameNetcodeStuff;
using LCChaosMod.Utils;
using Unity.Netcode;
using UnityEngine;

namespace LCChaosMod.Cogs
{
    public class PlayerSwapEvent : IChaosEvent
    {
        public string GetName()   => Loc.Get("event.player_swap");
        public bool   IsEnabled() => ChaosSettings.EnablePlayerSwap.Value;

        public void Execute()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Plugin.Log.LogInfo("[PlayerSwapEvent] Skipped - not host.");
                return;
            }

            var eligible = GetEligiblePlayers();
            if (eligible.Count < 2)
            {
                Plugin.Log.LogInfo("[PlayerSwapEvent] Not enough players to swap.");
                return;
            }

            int idxA = Random.Range(0, eligible.Count);
            int idxB;
            do { idxB = Random.Range(0, eligible.Count); } while (idxB == idxA);

            var playerA = eligible[idxA];
            var playerB = eligible[idxB];

            Vector3 posA      = playerA.transform.position;
            Vector3 posB      = playerB.transform.position;
            bool    insideA   = playerA.isInsideFactory;
            bool    insideB   = playerB.isInsideFactory;

            Plugin.Log.LogInfo($"[PlayerSwapEvent] Swapping {playerA.playerUsername} ↔ {playerB.playerUsername}.");

            PlayerSwap.Net.Send(playerA, posB, insideB);
            PlayerSwap.Net.Send(playerB, posA, insideA);
        }

        private static List<PlayerControllerB> GetEligiblePlayers()
        {
            var all = StartOfRound.Instance?.allPlayerScripts;
            if (all == null) return new List<PlayerControllerB>();

            var result = new List<PlayerControllerB>();
            foreach (var p in all)
                if (p.isPlayerControlled && !p.isPlayerDead && !PlayerUtils.IsOnShip(p))
                    result.Add(p);
            return result;
        }
    }
}
