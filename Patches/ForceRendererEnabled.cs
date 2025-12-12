using HarmonyLib;
using Unity.Netcode;
using GameNetcodeStuff;
using UnityEngine;
using System.Runtime.InteropServices;

namespace ExtendedLateCompany.Patches
{
    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnPlayerDC))]
    public static class FixDeadSlotOnDisconnect
    {
        [HarmonyPrefix]
        static void DisconnectPrefix(ulong clientId)
        {
            PlayerControllerB pcb = GetControllerForClient(clientId);
            if (pcb != null)
            {
                ResetPlayerControllerToDefault(pcb);
                ExtendedLateCompany.Logger.LogWarning("Calling ResetThings");
            }
        }
        private static PlayerControllerB GetControllerForClient(ulong clientId)
        {
            foreach (var p in StartOfRound.Instance.allPlayerScripts)
            {
                if (p != null && p.OwnerClientId == clientId)
                    return p;
            }
            ExtendedLateCompany.Logger.LogWarning("Returning NULL");
            return null;
        }
        private static void ResetPlayerControllerToDefault(PlayerControllerB p)
        {
            if (p == null) return;
            p.isClimbingLadder = false;
            p.disableMoveInput = false;
            p.ResetZAndXRotation();
            p.thisController.enabled = true;
            p.health = 100;
            p.hasBeenCriticallyInjured = false;
            p.disableLookInput = false;
            p.disableInteract = false;
            p.isPlayerDead = false;
            p.isInElevator = true;
            p.isInHangarShipRoom = true;
            p.isInsideFactory = false;
            p.parentedToElevatorLastFrame = false;
            p.overrideGameOverSpectatePivot = null;
            p.setPositionOfDeadPlayer = false;
            p.Crouch(crouch: false);
            p.criticallyInjured = false;
            if (p.playerBodyAnimator != null)
            {
                p.playerBodyAnimator.SetBool("Limp", value: true);
            }
            p.bleedingHeavily = false;
            p.activatingItem = false;
            p.twoHanded = false;
            p.inShockingMinigame = false;
            p.inSpecialInteractAnimation = false;
            p.freeRotationInInteractAnimation = false;
            p.disableSyncInAnimation = false;
            p.inAnimationWithEnemy = null;
            p.holdingWalkieTalkie = false;
            p.speakingToWalkieTalkie = false;
            p.isSinking = false;
            p.isUnderwater = false;
            p.sinkingValue = 0f;
            p.mapRadarDotAnimator.SetBool("dead", value: false);
            p.externalForceAutoFade = Vector3.zero;
            p.voiceMuffledByEnemy = false;
            ExtendedLateCompany.Logger.LogWarning("ResetThings is done");
        }
    }
}





// using System;
// using HarmonyLib;
// using UnityEngine;
// using Unity.Netcode;
// using GameNetcodeStuff;

// namespace ExtendedLateCompany
// {
//     [HarmonyPatch]
//     public class ForceRendererEnabled
//     {
//         private static readonly string[] forbiddenNames =
//         {
//             "PlayerPhysicsBox",
//             "LineOfSightCube",
//             "LineOfSightCubeSmall",
//             "LineOfSight2",
//             "MapDot",
//             "MapDirectionIndicator",
//             "BeamUp",
//             "BeamOutRedBuildup",
//             "BeamOutRed",
//             "Circle",
//             "CopyHeldProp"
//         };
//         [HarmonyPatch(typeof(StartOfRound), "OnPlayerConnectedClientRpc")]
//         [HarmonyPostfix]
//         public static void FixInvisiblePlayer(int assignedPlayerObjectId)
//         {
//             try
//             {
//                 var player = StartOfRound.Instance.allPlayerScripts[assignedPlayerObjectId];
//                 if (player == null)
//                 {
//                     ExtendedLateCompany.Logger.LogError($"[ELC] ERROR in FRE: Player object is NULL for playerID {assignedPlayerObjectId}");
//                     return;
//                 }
//                 if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
//                 {
//                     var ps = player.GetComponent<PlayerScript>();
//                     if (ps == null)
//                     {
//                         try
//                         {
//                             ps = player.gameObject.AddComponent<PlayerScript>();
//                         }
//                         catch (Exception ex)
//                         {
//                             ExtendedLateCompany.Logger.LogError($"[ELC] ERROR in FRE: Failed to add PlayerScript to player {player.name}: {ex}");
//                             return;
//                         }
//                     }
//                     if (ps != null)
//                     {
//                         ps.FixRenderersClientRpc();
//                     }
//                     else
//                     {
//                         ExtendedLateCompany.Logger.LogError($"[ELC] ERROR in FRE: PlayerScript is still null for player {player.name}");
//                     }
//                 }
//                 else if (NetworkManager.Singleton == null)
//                 {
//                     ExtendedLateCompany.Logger.LogError("[ELC] ERROR in FRE: NetworkManager.Singleton is null");
//                 }
//             }
//             catch (Exception ex)
//             {
//                 ExtendedLateCompany.Logger.LogError($"[ELC] EXCEPTION in FRE: {ex}");
//             }
//         }
//         public static void EnableRenderersAndResetState(GameObject player)
//         {
//             if (player == null)
//             {
//                 ExtendedLateCompany.Logger.LogError("[ELC] ERROR in FRE: player GameObject is null in EnableRenderersAndResetState");
//                 return;
//             }
//             try
//             {
//                 foreach (var smr in player.GetComponentsInChildren<SkinnedMeshRenderer>(true))
//                 {
//                     if (ShouldSkipRenderer(smr.gameObject.name)) continue;
//                     smr.enabled = true;
//                 }
//                 foreach (var r in player.GetComponentsInChildren<Renderer>(true))
//                 {
//                     if (ShouldSkipRenderer(r.gameObject.name)) continue;
//                     r.enabled = true;
//                 }
//                 Animator animator = player.GetComponentInChildren<Animator>(true);
//                 if (animator != null)
//                 {
//                     animator.enabled = true;
//                 }
//                 if (!player.activeSelf)
//                 {
//                     player.SetActive(true);
//                 }
//                 var playerScript = player.GetComponent<PlayerControllerB>();
//                 if (playerScript != null)
//                 {
//                     playerScript.health = 100;
//                     playerScript.bleedingHeavily = false;
//                     playerScript.criticallyInjured = false;
//                     playerScript.disableMoveInput = false;
//                     playerScript.disableLookInput = false;
//                     playerScript.disableInteract = false;

//                     if (playerScript.playerBodyAnimator != null)
//                         playerScript.playerBodyAnimator.SetBool("Limp", false);
//                 }
//                 else
//                 {
//                     ExtendedLateCompany.Logger.LogError($"[ELC] ERROR in FRE: PlayerControllerB component missing on player {player.name}");
//                 }
//             }
//             catch (Exception ex)
//             {
//                 ExtendedLateCompany.Logger.LogError($"[ELC] EXCEPTION in FRE in EnableRenderersAndResetState: {ex}");
//             }
//         }
//         private static bool ShouldSkipRenderer(string name)
//         {
//             foreach (var bad in forbiddenNames)
//             {
//                 if (string.Equals(name, bad, StringComparison.OrdinalIgnoreCase))
//                     return true;
//             }
//             return false;
//         }
//     }
//     public partial class PlayerScript : NetworkBehaviour
//     {
//         [ClientRpc]
//         public void FixRenderersClientRpc()
//         {
//             ForceRendererEnabled.EnableRenderersAndResetState(gameObject);
//         }
//     }
// }