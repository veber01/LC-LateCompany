using Unity.Netcode;
using UnityEngine;

namespace ExtendedLateCompany.Patches
{
    public static class LobbyManager
    {
        public static bool currentLobbyVisible = true;
        public static void RefreshLobbyVisibility()
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }
            if (StartOfRound.Instance == null)
                return;
            bool lateJoinEnabled = ExtendedLateCompany.LateJoin.Value;
            bool inShip = StartOfRound.Instance.inShipPhase;
            if (!lateJoinEnabled)
            {
                SetLobbyVisible(false);
                return;
            }
            if (!inShip)
            {
                SetLobbyVisible(false);
                return;
            }
            bool hasOpenSlot = StartOfRound.Instance.connectedPlayersAmount + 1 < StartOfRound.Instance.allPlayerScripts.Length;
            SetLobbyVisible(hasOpenSlot);
        }

        public static void SetLobbyVisible(bool visible)
        {
            if (currentLobbyVisible == visible) return;

            currentLobbyVisible = visible;
            ExtendedLateCompany.Logger.LogInfo($"[ELC LM] Lobby visibility changed to: {visible}");

            if (!GameNetworkManager.Instance.currentLobby.HasValue)
            {
                return;
            }
            var lobby = GameNetworkManager.Instance.currentLobby.Value;
            try
            {
                lobby.SetData("joinable", visible ? "true" : "false");
            }
            catch (System.Exception ex)
            {
                ExtendedLateCompany.Logger.LogWarning($"[ELC LM] Failed to set lobby data: {ex.Message}");
            }
            GameNetworkManager.Instance.SetLobbyJoinable(visible);
            var quickMenu = Object.FindObjectOfType<QuickMenuManager>();
            if (quickMenu)
            {
                quickMenu.inviteFriendsTextAlpha.alpha = visible ? 1f : 0.2f;
            }
        }
        public static void OnPlayerJoined()
        {
            RefreshLobbyVisibility();
        }

        public static void OnPlayerLeft()
        {
            RefreshLobbyVisibility();
        }

        public static void OnShipPhaseChanged(bool inShipPhaseNow)
        {
            RefreshLobbyVisibility();
        }
    }
}