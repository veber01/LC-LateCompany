using HarmonyLib;
using GameNetcodeStuff;
using Steamworks;
using System.Linq;
using Steamworks.Data;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;



namespace ExtendedLateCompany.Patches
{
    [HarmonyWrapSafe]
    public static class PlayerNameFixPatch
    {
        private static readonly Queue<ulong> pendingJoinedSteamIds = new Queue<ulong>();
        private static void RefreshAllPlayerNames()
        {
            var sor = StartOfRound.Instance;
            var gnm = GameNetworkManager.Instance;
            if (sor == null || gnm == null) return;
            if (gnm.disableSteam) return;
            if (sor.allPlayerScripts == null) return;
            UpdateQuickMenuNames();
            UpdateBillboardNames();
            UpdateMapScreenName();
        }
        private static void UpdateQuickMenuNames()
        {
            var sor = StartOfRound.Instance;
            var quickMenu = UnityEngine.Object.FindObjectOfType<QuickMenuManager>();
            if (quickMenu == null) return;

            for (int i = 0; i < sor.allPlayerScripts.Length; i++)
            {
                var player = sor.allPlayerScripts[i];
                var slot = quickMenu.playerListSlots[i];
                if (player.playerSteamId == 0)
                {
                    if (slot.slotContainer.activeSelf)
                        slot.slotContainer.SetActive(false);
                    continue;
                }
                if (!slot.slotContainer.activeSelf)
                    slot.slotContainer.SetActive(true);
                var steamName = new Friend(player.playerSteamId).Name;
                if (steamName == "[unknown]") continue;

                if (slot.usernameHeader.text != steamName)
                {
                    slot.usernameHeader.text = steamName;
                    slot.playerSteamId = player.playerSteamId;
                }
            }
        }
        private static void UpdateBillboardNames()
        {
            var sor = StartOfRound.Instance;

            foreach (var player in sor.allPlayerScripts)
            {
                if (player.playerSteamId == 0)
                    continue;
                var steamName = new Friend(player.playerSteamId).Name;
                if (steamName == "[unknown]") continue;

                if (player.playerUsername != steamName)
                {
                    player.playerUsername = steamName;
                    player.usernameBillboardText.text = steamName;
                }
            }
        }
        private static ManualCameraRenderer manualCamera = null;
        private static void UpdateMapScreenName()
        {
            var sor = StartOfRound.Instance;

            if (manualCamera != null &&
                manualCamera.targetedPlayer != null)
            {
                var player = manualCamera.targetedPlayer;
                if (sor.mapScreenPlayerName.text != player.playerUsername)
                {
                    sor.mapScreenPlayerName.text = player.playerUsername;
                }
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.SteamMatchmaking_OnLobbyMemberJoined))]
        public static void LobbyJoinedPatch(Lobby lobby, Friend friend)
        {
            if (friend.Id != 0)
            {
                lock (pendingJoinedSteamIds)
                {
                    pendingJoinedSteamIds.Enqueue(friend.Id);
                }
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.SendNewPlayerValuesClientRpc))]
        public static void NewPlayerValuesPatch()
        {
            RefreshAllPlayerNames();
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnPlayerConnectedClientRpc))]
        public static void PlayerConnectedPatch()
        {
            RefreshAllPlayerNames();
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
        public static void ConnectToPlayerPatch(PlayerControllerB __instance)
        {
            ulong idToSend = 0;

            lock (pendingJoinedSteamIds)
            {
                if (pendingJoinedSteamIds.Count > 0)
                {
                    idToSend = pendingJoinedSteamIds.Dequeue();
                }
            }
            if (idToSend != 0)
            {
                try
                {
                    __instance.SendNewPlayerValuesServerRpc(idToSend);
                }
                catch (System.Exception ex)
                {
                    ExtendedLateCompany.Logger.LogError($"[ELC PName] Failed to call SendNewPlayerValuesServerRpc: {ex}");
                }
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.StartGame))]
        public static void StartGamePatch()
        {
            RefreshAllPlayerNames();
        }
        public static class SpectateModUtils
        {
            public static void ClearSpectateBox(PlayerControllerB leavingPlayer)
            {
                var boxEntry = HUDManager.Instance.spectatingPlayerBoxes
                    .FirstOrDefault(x => x.Value == leavingPlayer);
                if (boxEntry.Key != null)
                {
                    var boxGO = boxEntry.Key.gameObject;
                    var rawImage = boxGO.GetComponent<RawImage>();
                    if (rawImage != null)
                        rawImage.texture = null;
                    var usernameText = boxGO.GetComponentInChildren<TextMeshProUGUI>();
                    if (usernameText != null)
                        usernameText.text = "";
                    HUDManager.Instance.spectatingPlayerBoxes.Remove(boxEntry.Key);
                    UnityEngine.Object.Destroy(boxGO);
                }
            }
        }
    }
}
