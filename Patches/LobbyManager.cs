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

            ExtendedLateCompany.Logger.LogWarning($"[ELC LM] Lobby visibility changed to: {visible}");

            if (!GameNetworkManager.Instance.currentLobby.HasValue)
            {
                ExtendedLateCompany.Logger.LogWarning("[ELC LM]] No current lobby available.");
                return;
            }

            var lobby = GameNetworkManager.Instance.currentLobby.Value;
            lobby.SetData("joinable", visible ? "true" : "false");
            ExtendedLateCompany.Logger.LogWarning($"[ELC LM]] Steam lobby metadata 'joinable' set to: {visible}");
            GameNetworkManager.Instance.SetLobbyJoinable(visible);
            var quickMenu = Object.FindObjectOfType<QuickMenuManager>();
            if (quickMenu) 
            {
                quickMenu.inviteFriendsTextAlpha.alpha = visible ? 1f : 0.2f;
                ExtendedLateCompany.Logger.LogWarning($"[ELC LM]] Invite friends button alpha updated to {(visible ? 1f : 0.2f)}");
            }
        }

        public static void UpdateLobbyVisibilityBasedOnSlots()
        {
            if (StartOfRound.Instance == null)
            {
                ExtendedLateCompany.Logger.LogWarning("[ELC LM]] StartOfRound.Instance is null, cannot update lobby visibility.");
                return;
            }

            bool hasOpenSlot = StartOfRound.Instance.connectedPlayersAmount + 1 < StartOfRound.Instance.allPlayerScripts.Length;
            ExtendedLateCompany.Logger.LogWarning($"[ELC LM]] Updating lobby visibility based on slots: hasOpenSlot={hasOpenSlot}");
            SetLobbyVisible(hasOpenSlot);
        }
    }
}