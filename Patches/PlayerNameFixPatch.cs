using HarmonyLib;
using UnityEngine;

namespace ExtendedLateCompany
{
    [HarmonyPatch]
    public class PlayerNameFixPatch
    {
        [HarmonyPatch(typeof(StartOfRound), "OnPlayerConnectedClientRpc")]
        [HarmonyPostfix]
        public static void OnPlayerJoinedPostfix(int assignedPlayerObjectId)
        {
            QuickMenuManager quickMenu = Object.FindObjectOfType<QuickMenuManager>();
            if (quickMenu == null) return;
            var player = StartOfRound.Instance.allPlayerScripts[assignedPlayerObjectId];
            if (player == null) return;
            string finalName = player.playerUsername;
            ulong steamId = player.playerSteamId;
            for (int i = 0; i < quickMenu.playerListSlots.Length; i++)
            {
                var slot = quickMenu.playerListSlots[i];
                if (slot.playerSteamId == steamId || slot.playerSteamId == 0)
                {
                    slot.usernameHeader.text = finalName;
                    slot.playerSteamId = steamId;
                    break;
                }
            }
        }
    }
}