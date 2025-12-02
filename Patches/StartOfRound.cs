using System.Collections.Generic;
using HarmonyLib;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using System;
using GameNetcodeStuff;
using Steamworks;
using Steamworks.Data;
using Steamworks.ServerList;

namespace ExtendedLateCompany.Patches;

internal static class StartOfRoundPatch
{
    
    private static void SetLobbyVisibility(bool visible)
    {
        if (!GameNetworkManager.Instance.currentLobby.HasValue) return;
        var lobby = GameNetworkManager.Instance.currentLobby.Value;
        ExtendedLateCompany.SetLobbyJoinable(visible);
        lobby.SetData("joinable", visible ? "true" : "false");
    }

    [HarmonyPatch(typeof(StartOfRound), "OnPlayerConnectedClientRpc")]
    [HarmonyWrapSafe]
    private static class OnPlayerConnectedClientRpc_Patch
    {
        private static void UpdateControlledState()
        {
            
            for (int j = 0; j < StartOfRound.Instance.connectedPlayersAmount + 1; j++)
            {
                
                if ((j == 0 || !StartOfRound.Instance.allPlayerScripts[j].IsOwnedByServer) && !StartOfRound.Instance.allPlayerScripts[j].isPlayerDead)
                {
                    StartOfRound.Instance.allPlayerScripts[j].isPlayerControlled = true;
                }
            }
        }

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var newInstructions = new List<CodeInstruction>();
            bool foundInitial = false;
            bool shouldSkip = false;
            bool alreadyReplaced = false;

            foreach (var instruction in instructions)
            {
                if (!alreadyReplaced)
                {
                    if (!foundInitial && instruction.opcode == OpCodes.Call && 
                        instruction.operand?.ToString().Contains("setPlayerToSpawnPosition") == true)
                    {
                        foundInitial = true;
                    }
                    else if (foundInitial && instruction.opcode == OpCodes.Ldc_I4_0)
                    {
                        shouldSkip = true;
                        continue;
                    }
                    else if (shouldSkip && instruction.opcode == OpCodes.Ldloc_0)
                    {
                        shouldSkip = false;
                        alreadyReplaced = true;
                        newInstructions.Add(new CodeInstruction(OpCodes.Call, 
                            AccessTools.Method(typeof(OnPlayerConnectedClientRpc_Patch), nameof(UpdateControlledState))));
                    }
                }
                if (!shouldSkip)
                    newInstructions.Add(instruction);
            }

            if (!alreadyReplaced) 
                ExtendedLateCompany.Logger.LogError("ELC: Failed to transpile OnPlayerConnectedClientRpc");

            return newInstructions.AsEnumerable();
        }

        [HarmonyPostfix]
        private static void Postfix()
        {
            if (StartOfRound.Instance.connectedPlayersAmount + 1 >= StartOfRound.Instance.allPlayerScripts.Length)
            {
                ExtendedLateCompany.SetLobbyJoinable(false);
                SetLobbyVisibility(false);
            }
        }
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnPlayerDC))]
    [HarmonyWrapSafe]
    private static class OnPlayerDC_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(int playerObjectNumber)
        {
            if (StartOfRound.Instance.inShipPhase)
            {
                bool hasOpenSlot = StartOfRound.Instance.connectedPlayersAmount + 1 < StartOfRound.Instance.allPlayerScripts.Length;
                if (hasOpenSlot)
                {
                    SetLobbyVisibility(true);
                    ExtendedLateCompany.SetLobbyJoinable(true);
                }
            }

            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerObjectNumber];
            player.activatingItem = false;
            player.bleedingHeavily = false;
            player.clampLooking = false;
            player.criticallyInjured = false;
            player.Crouch(false);
            player.disableInteract = false;
            player.DisableJetpackControlsLocally();
            player.disableLookInput = false;
            player.disableMoveInput = false;
            player.DisablePlayerModel(player.gameObject, enable: true, disableLocalArms: true);
            player.disableSyncInAnimation = false;
            player.externalForceAutoFade = Vector3.zero;
            player.freeRotationInInteractAnimation = false;
            player.hasBeenCriticallyInjured = false;
            player.health = 100;
            player.helmetLight.enabled = false;
            player.holdingWalkieTalkie = false;
            player.inAnimationWithEnemy = null;
            player.inShockingMinigame = false;
            player.inSpecialInteractAnimation = false;
            player.inVehicleAnimation = false;
            player.isClimbingLadder = false;
            player.isSinking = false;
            player.isUnderwater = false;
            player.mapRadarDotAnimator?.SetBool("dead", false);
            player.playerBodyAnimator?.SetBool("Limp", false);
            player.ResetZAndXRotation();
            player.sinkingValue = 0f;
            player.speakingToWalkieTalkie = false;
            player.statusEffectAudio?.Stop();
            player.thisController.enabled = true;
            player.transform.SetParent(StartOfRound.Instance.playersContainer);
            player.twoHanded = false;
            player.voiceMuffledByEnemy = false;
        }
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.StartGame))]
    private static class StartGame_Patch
    {
        [HarmonyPrefix]
        private static void Prefix()
        {
            SetLobbyVisibility(false);
            ExtendedLateCompany.SetLobbyJoinable(false);
        }
    }

    [HarmonyPatch(typeof(StartOfRound), "SetShipReadyToLand")]
    private static class SetShipReadyToLand_Patch
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            bool hasOpenSlot = StartOfRound.Instance.connectedPlayersAmount + 1 < StartOfRound.Instance.allPlayerScripts.Length;
            SetLobbyVisibility(hasOpenSlot);
            ExtendedLateCompany.SetLobbyJoinable(hasOpenSlot);
            //hopefully this works xD
            GameNetworkManager.Instance.connectedPlayers = StartOfRound.Instance.connectedPlayersAmount+1;

//            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
//            {
//                var quickMenu = UnityEngine.Object.FindObjectOfType<QuickMenuManager>();
//                var player = StartOfRound.Instance.allPlayerScripts[i];
//                var slot = quickMenu.playerListSlots[i];
//                ExtendedLateCompany.Logger.LogWarning(player.playerClientId);
//                ExtendedLateCompany.Logger.LogWarning("GNM Connected players: "+GameNetworkManager.Instance.connectedPlayers);
//                ExtendedLateCompany.Logger.LogWarning("SOR Connected players: "+StartOfRound.Instance.connectedPlayersAmount);
//            }
        }
    }
}