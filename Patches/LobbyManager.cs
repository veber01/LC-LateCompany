using UnityEngine;

namespace ExtendedLateCompany.Patches
{
    public static class LobbyManager
    {
        public static bool currentLobbyVisible = true;

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
            lobby.SetData("joinable", visible ? "true" : "false");
            GameNetworkManager.Instance.SetLobbyJoinable(visible);
            var quickMenu = Object.FindObjectOfType<QuickMenuManager>();
            if (quickMenu) 
            {
                quickMenu.inviteFriendsTextAlpha.alpha = visible ? 1f : 0.2f;
            }
        }

        public static void UpdateLobbyVisibilityBasedOnSlots()
        {
            if (StartOfRound.Instance == null)
            {
                return;
            }

            bool hasOpenSlot = StartOfRound.Instance.connectedPlayersAmount + 1 < StartOfRound.Instance.allPlayerScripts.Length;
            SetLobbyVisible(hasOpenSlot);
        }
    }
}