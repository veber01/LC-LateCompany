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
using ExtendedLateCompany.Patches;
using Unity.Netcode;

namespace ExtendedLateCompany.Patches;

internal static class StartOfRoundPatch
{
    [HarmonyPatch(typeof(StartOfRound), "OnPlayerConnectedClientRpc")]
    [HarmonyWrapSafe]
    private static class OnPlayerConnectedClientRpc_Patch
    {
        private static void UpdateControlledState()
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }
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
                ExtendedLateCompany.Logger.LogError("ELC SoR: Failed to transpile OnPlayerConnectedClientRpc");

            return newInstructions.AsEnumerable();
        }
        [HarmonyPostfix]
        private static void Postfix()
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            if (StartOfRound.Instance.connectedPlayersAmount + 1 >= StartOfRound.Instance.allPlayerScripts.Length)
            {
                LobbyManager.SetLobbyVisible(false);
            }
        }
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnPlayerDC))]
    [HarmonyWrapSafe]
    private static class OnPlayerDC_Patch
    {
        [HarmonyPrefix]
        private static void Prefix()
        {

        }

        [HarmonyPostfix]
        private static void Postfix(int playerObjectNumber)
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }
            if (StartOfRound.Instance.inShipPhase)
            {
                bool hasOpenSlot = StartOfRound.Instance.connectedPlayersAmount + 1 < StartOfRound.Instance.allPlayerScripts.Length;
                if (hasOpenSlot)
                {
                    LobbyManager.SetLobbyVisible(true);
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
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }
            LobbyManager.SetLobbyVisible(false);
        }
    }

    [HarmonyPatch(typeof(StartOfRound), "SetShipReadyToLand")]
    private static class SetShipReadyToLand_Patch
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            bool hasOpenSlot = StartOfRound.Instance.connectedPlayersAmount + 1 < StartOfRound.Instance.allPlayerScripts.Length;
            LobbyManager.SetLobbyVisible(hasOpenSlot);
            try
            {
                GameNetworkManager.Instance.connectedPlayers = StartOfRound.Instance.connectedPlayersAmount + 1;
            }
            catch (Exception ex)
            {
                ExtendedLateCompany.Logger.LogWarning($"[ELC SoR] SetShipReadyToLand: failed to set GameNetworkManager.connectedPlayers: {ex}");
            }
        }
    }
}
