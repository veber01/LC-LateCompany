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
        public static StartOfRound playersManager;
        public static ulong playerClientId;
        private static void RefreshAllPlayerNames()
        {
            ExtendedLateCompany.Logger.LogWarning("RefreshAllPlayerNames called.");
            var sor = StartOfRound.Instance;
            var gnm = GameNetworkManager.Instance;
            if (sor == null || gnm == null) return;
            ExtendedLateCompany.Logger.LogWarning("SOR and GNM !NULL");
            if (gnm.disableSteam) return;
            ExtendedLateCompany.Logger.LogWarning("GNM.disableSteam !null");
            if (sor.allPlayerScripts == null) return;
            ExtendedLateCompany.Logger.LogWarning("sor.allplayerscripts !NULL");
            ExtendedLateCompany.Logger.LogWarning("Calling all local name updates and serverrpc to force refresh");
            UpdateQuickMenuNames();
            UpdateBillboardNames();
            UpdateMapScreenName();
            ForceRefreshAllPlayerNames();
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
            ExtendedLateCompany.Logger.LogWarning("UpdateQuickMenuNames Updated");
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
            ExtendedLateCompany.Logger.LogWarning("UpdateBillboardNames Updated");
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
            ExtendedLateCompany.Logger.LogWarning("UpdateMapScreenName Updated");
        }
        public static void ForceRefreshAllPlayerNames()
        {
            var sor = StartOfRound.Instance;
            if (sor == null) return;

            for (int i = 0; i < sor.allPlayerScripts.Length; i++)
            {
                var player = sor.allPlayerScripts[i];
                if (player == null) continue;

                ulong steamId = player.playerSteamId;
                if (steamId == 0) continue;
                player.SendNewPlayerValuesServerRpc(steamId);
            }
            ExtendedLateCompany.Logger.LogWarning("ForceRefreshAllPlayerNames Updated");
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.SteamMatchmaking_OnLobbyMemberJoined))]
        public static void LobbyJoinedPatch()
        {
            RefreshAllPlayerNames();
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
        public static void ConnectToPlayerPatch()
        {
            RefreshAllPlayerNames();
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.StartGame))]
        public static void StartGamePatch()
        {
            RefreshAllPlayerNames();
        }
    }
}