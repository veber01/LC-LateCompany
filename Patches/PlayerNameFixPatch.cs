using HarmonyLib;
using UnityEngine;

namespace ExtendedLateCompany.Patches
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
            ExtendedLateCompany.Logger.LogWarning($"[ELC] finalName: {finalName} SteamID: {steamId}");
            for (int i = 0; i < quickMenu.playerListSlots.Length; i++)
            {
                var slot = quickMenu.playerListSlots[i];
                if (slot == null) continue;
                ExtendedLateCompany.Logger.LogWarning($"[ELC] Slot {i} SteamID: {slot.playerSteamId}");
                bool match = slot.playerSteamId == steamId;
                bool empty = slot.playerSteamId == 0;
                if ((match || empty) && (slot.usernameHeader.text != finalName || slot.playerSteamId != steamId))
                {
                    ExtendedLateCompany.Logger.LogWarning(
                        $"[ELC] Updating slot {i}: OldName='{slot.usernameHeader.text}' → NewName='{finalName}'"
                    );

                    ExtendedLateCompany.Logger.LogWarning(
                        $"[ELC] Updating slot {i}: OldSteamID='{slot.playerSteamId}' → NewSteamID='{steamId}'"
                    );
                    slot.usernameHeader.text = finalName;
                    slot.playerSteamId = steamId;
                    break;
                }
            }
        }
    }
}