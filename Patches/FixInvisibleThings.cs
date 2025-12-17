// using HarmonyLib;
// using Unity.Netcode;
// using GameNetcodeStuff;
// using UnityEngine;
// using System.Runtime.InteropServices;

// namespace ExtendedLateCompany.Patches
// {
//     [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnPlayerDC))]
//     public static class FixDeadSlotOnDisconnect
//     {
//         [HarmonyPrefix]
//         static void DisconnectPrefix(ulong clientId)
//         {
//             PlayerControllerB pcb = GetControllerForClient(clientId);
//             ExtendedLateCompany.Logger.LogWarning("PCB: " + pcb);
//             if (pcb != null)
//             {
//                 ResetPlayerControllerToDefault(pcb);
//                 ExtendedLateCompany.Logger.LogWarning("Calling ResetThings");
//             }
//         }
//         private static PlayerControllerB GetControllerForClient(ulong clientId)
//         {
//             foreach (var p in StartOfRound.Instance.allPlayerScripts)
//             {
//                 ExtendedLateCompany.Logger.LogWarning("PLAYERCLIENTID" + p.playerClientId);
//                 ExtendedLateCompany.Logger.LogWarning("ACTUALLICNETID" + p.actualClientId);
//                 ExtendedLateCompany.Logger.LogWarning("OWNERCLIENTID" + clientId);
//                 if (p != null && p.actualClientId == clientId)
//                     return p;
//             }
//             ExtendedLateCompany.Logger.LogWarning("Returning NULL");
//             return null;
//         }
//         private static void ResetPlayerControllerToDefault(PlayerControllerB p)
//         {
//             if (p == null) return;
//             p.isClimbingLadder = false;
//             p.disableMoveInput = false;
//             p.ResetZAndXRotation();
//             p.thisController.enabled = false;
//             p.isPlayerDead = false;
//             p.DisablePlayerModel(StartOfRound.Instance.allPlayerObjects[p.actualClientId], enable: true, disableLocalArms: true);
//             p.health = 100;
//             p.hasBeenCriticallyInjured = false;
//             p.disableLookInput = false;
//             p.disableInteract = false;
//             p.isPlayerDead = false;
//             p.isInElevator = true;
//             p.isInHangarShipRoom = true;
//             p.isInsideFactory = false;
//             p.parentedToElevatorLastFrame = false;
//             p.overrideGameOverSpectatePivot = null;
//             p.setPositionOfDeadPlayer = false;
//             p.Crouch(crouch: false);
//             p.criticallyInjured = false;
//             p.playerBodyAnimator.enabled = true;
//             p.thisPlayerModel.enabled = true;
//             if (p.playerBodyAnimator != null)
//             {
//                 p.playerBodyAnimator.SetBool("Limp", value: false);
//             }
//             p.bleedingHeavily = false;
//             p.activatingItem = false;
//             p.twoHanded = false;
//             p.inShockingMinigame = false;
//             p.inSpecialInteractAnimation = false;
//             p.freeRotationInInteractAnimation = false;
//             p.disableSyncInAnimation = false;
//             p.inAnimationWithEnemy = null;
//             p.holdingWalkieTalkie = false;
//             p.speakingToWalkieTalkie = false;
//             p.isSinking = false;
//             p.isUnderwater = false;
//             p.sinkingValue = 0f;
//             p.mapRadarDotAnimator.SetBool("dead", value: false);
//             p.externalForceAutoFade = Vector3.zero;
//             p.voiceMuffledByEnemy = false;
//             var netObj = p.gameObject.GetComponent<NetworkObject>();
//             if (netObj != null)
//             {
//                 netObj.RemoveOwnership(); // frees the slot for new players
//             }


//             ExtendedLateCompany.Logger.LogWarning("ResetThings is done");
//         }
//     }
// }


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


// using HarmonyLib;
// using Unity.Netcode;
// using GameNetcodeStuff;
// using UnityEngine;

// namespace ExtendedLateCompany.Patches
// {
//     [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Start))]
//     public static class SoRSavePrefPatch
//     {
//         [HarmonyPostfix]
//         private static void Postfix()
//         {
//             Invisiblethings.Copyfromslot(3);
//         }
//     }

//     [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnPlayerDC))]
//     public static class Invisiblethings
//     {
//         public static GameObject safecopy;
//         public static void Copyfromslot(int id)
//         {
//             if (safecopy != null) return;
//             GameObject copyid = StartOfRound.Instance.allPlayerObjects[id];
//             if (copyid == null)
//             {
//                 ExtendedLateCompany.Logger.LogError($"Slot {id} is empty. Couldnt copy.");
//                 return;
//             }
//             safecopy = UnityEngine.Object.Instantiate(copyid);
//             safecopy.SetActive(false);
//             ExtendedLateCompany.Logger.LogWarning($"Copy made from:{id}.");
//         }
//         [HarmonyPrefix]
//         static void Prefix(int playerObjectNumber, ulong clientId)
//         {
//             if (safecopy == null)
//             {
//                 ExtendedLateCompany.Logger.LogError("Copy not copied");
//                 return;
//             }
//             GameObject oldPlayer = StartOfRound.Instance.allPlayerObjects[playerObjectNumber];
//             if (oldPlayer != null)
//             {
//                 PlayerControllerB oldController = oldPlayer.GetComponent<PlayerControllerB>();
//                 if (oldController != null)
//                 {
//                     oldController.DropAllHeldItems(itemsFall: true, disconnecting: true);
//                     oldController.disconnectedMidGame = true;
//                     NetworkObject netObj = oldController.gameObject.GetComponent<NetworkObject>();
//                     if (netObj != null && netObj.IsSpawned)
//                     {
//                         if (NetworkManager.Singleton.IsServer)
//                             netObj.Despawn(true);
//                         UnityEngine.Object.Destroy(oldController.gameObject);
//                     }
//                     ExtendedLateCompany.Logger.LogWarning($"Old player {clientId} slot destroyed oof.");
//                 }
//                 StartOfRound.Instance.allPlayerObjects[playerObjectNumber] = null;
//                 StartOfRound.Instance.allPlayerScripts[playerObjectNumber] = null;
//             }
//             GameObject newPlayer = UnityEngine.Object.Instantiate(safecopy);
//             newPlayer.SetActive(true);
//             PlayerControllerB newController = newPlayer.GetComponent<PlayerControllerB>();
//             StartOfRound.Instance.allPlayerObjects[playerObjectNumber] = newPlayer;
//             StartOfRound.Instance.allPlayerScripts[playerObjectNumber] = newController;
//             NetworkObject newPobj = newPlayer.GetComponent<NetworkObject>();
//             if (NetworkManager.Singleton.IsServer && !newPobj.IsSpawned)
//             {
//                 newPobj.Spawn();
//             }
//             ExtendedLateCompany.Logger.LogWarning($"New player object spawned and reset in slot {playerObjectNumber}, praying for it to work.");
//         }
//     }
// }

using HarmonyLib;
using Unity.Netcode;
using GameNetcodeStuff;
using UnityEngine;

namespace ExtendedLateCompany.Patches
{
    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Start))]
    public static class SoRSavePrefPatch
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                ExtendedLateCompany.Logger.LogWarning("[ELC INV] Is not server SoR.Start Patch");
                return;
            }
            Invisiblethings.CopyFromSlot(3);
        }
    }
    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnPlayerDC))]
    public static class Invisiblethings
    {
        public static GameObject safecopy;
        public static void CopyFromSlot(int id)
        {
            if (safecopy != null) return;
            GameObject source = StartOfRound.Instance.allPlayerObjects[id];
            if (source == null)
            {
                ExtendedLateCompany.Logger.LogError($"[ELC INV] Slot {id} empty, cannot create copy.");
                return;
            }
            safecopy = Object.Instantiate(source);
            safecopy.SetActive(false);
            NetworkObject net = safecopy.GetComponent<NetworkObject>();
            if (net != null)
                net.enabled = false;

            ExtendedLateCompany.Logger.LogWarning($"[ELC INV] Safecopy created from slot {id}");
        }

        [HarmonyPostfix]
        static void Postfix(int playerObjectNumber, ulong clientId)
        {
            // SERVER ONLY
            if (!NetworkManager.Singleton.IsServer)
            {
                ExtendedLateCompany.Logger.LogWarning("[ELC INV] Is not server SoR.OnPlayerDC Patch");
                return;
            }
            if (safecopy == null)
            {
                ExtendedLateCompany.Logger.LogWarning("[ELC INV] Safecopy is null");
                return;
            }
            if (playerObjectNumber < 0 || playerObjectNumber >= StartOfRound.Instance.allPlayerObjects.Length)
            {
                ExtendedLateCompany.Logger.LogWarning("[ELC INV] B");
                return;
            }
            GameObject oldPlayer = StartOfRound.Instance.allPlayerObjects[playerObjectNumber];
            PlayerControllerB oldController = oldPlayer != null ? oldPlayer.GetComponent<PlayerControllerB>() : null;
            if (!oldController.isPlayerDead)
                //hmmmmmmmmmmmmmmmmmmmmmmmmmm
            if (oldController != null)
            {
                ExtendedLateCompany.Logger.LogWarning("[ELC INV] Oldcontroller not null, destroying old playerthings and spawning new.");
                oldController.DropAllHeldItems(true, true);

                NetworkObject oldNet = oldController.GetComponent<NetworkObject>();
                if (oldNet != null && oldNet.IsSpawned)
                    oldNet.Despawn(true);

                Object.Destroy(oldController.gameObject);
            }
            StartOfRound.Instance.allPlayerObjects[playerObjectNumber] = null;
            StartOfRound.Instance.allPlayerScripts[playerObjectNumber] = null;
            GameObject newPlayer = Object.Instantiate(safecopy);
            newPlayer.SetActive(true);
            PlayerControllerB pc = newPlayer.GetComponent<PlayerControllerB>();
            if (pc == null)
                ExtendedLateCompany.Logger.LogWarning("pc is null");
            pc.playerClientId = (ulong)playerObjectNumber;
            pc.actualClientId = (ulong)playerObjectNumber;
            ExtendedLateCompany.Logger.LogWarning(pc.playerClientId);
            ExtendedLateCompany.Logger.LogWarning(pc.actualClientId);
            // ResetValues(pc);
            pc.TeleportPlayer(StartOfRound.Instance.notSpawnedPosition.position);
            StartOfRound.Instance.allPlayerObjects[playerObjectNumber] = newPlayer;
            StartOfRound.Instance.allPlayerScripts[playerObjectNumber] = pc;
            NetworkObject newNet = newPlayer.GetComponent<NetworkObject>();
            if(newNet == null)
                ExtendedLateCompany.Logger.LogWarning("newNet is null");
            newNet.enabled = true;
            newNet.Spawn();
            newNet.RemoveOwnership();
            ExtendedLateCompany.Logger.LogWarning($"[ELC INV] Replaced disconnected player {clientId} with a copy {playerObjectNumber}");
        }
        private static void ResetValues(PlayerControllerB p)
        {
            p.isPlayerControlled = false;
            p.isPlayerDead = false;
            p.disconnectedMidGame = false;
            p.disableMoveInput = true;
            p.disableLookInput = true;
            p.disableInteract = true;
            p.health = 100;
            p.bleedingHeavily = false;
            p.criticallyInjured = false;
            p.isInElevator = true;
            p.isInHangarShipRoom = true;
            p.isInsideFactory = false;
            p.overrideGameOverSpectatePivot = null;
            p.setPositionOfDeadPlayer = false;
            p.externalForceAutoFade = Vector3.zero;
            if (StartOfRound.Instance != null)
            {
                p.DisablePlayerModel(
                    StartOfRound.Instance.allPlayerObjects[(int)p.playerClientId],
                    enable: false,
                    disableLocalArms: true
                );
            }
            ExtendedLateCompany.Logger.LogWarning("Reset done");
        }
    }
}



