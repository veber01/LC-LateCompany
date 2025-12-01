using HarmonyLib;
using UnityEngine;
using GameNetcodeStuff;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
using System.Runtime.CompilerServices;


namespace ExtendedLateCompany.Patches
{
    [HarmonyWrapSafe]
    public static class PlayerNameFixPatch
    {
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
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "SteamMatchmaking_OnLobbyMemberJoined")]
        public static void LobbyJoinedPatch()
        {
            RefreshAllPlayerNames();
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "SendNewPlayerValuesClientRpc")]
        public static void NewPlayerValuesPatch()
        {
            RefreshAllPlayerNames();
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), "OnPlayerConnectedClientRpc")]
        public static void PlayerConnectedPatch()
        {
            RefreshAllPlayerNames();
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
        public static void ConnectToPlayerPatch()
        {
            RefreshAllPlayerNames();
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), "StartGame")]
        public static void StartGamePatch()
        {
            RefreshAllPlayerNames();
        }
    }
}