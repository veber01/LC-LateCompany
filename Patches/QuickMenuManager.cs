using BepInEx.Logging;
using HarmonyLib;
namespace ExtendedLateCompany.Patches;
[HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.DisableInviteFriendsButton))]
internal static class DisableInviteFriendsButton_Patch
{
	[HarmonyPrefix]
	private static bool Prefix()
	{
		return false;
	}
}
[HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.InviteFriendsButton))]
internal static class InviteFriendsButton_Patch
{
	[HarmonyPrefix]
	private static bool Prefix()
	{
		if (LobbyManager.currentLobbyVisible && !GameNetworkManager.Instance.disableSteam) GameNetworkManager.Instance.InviteFriendsUI();
		return false;
	}
}
[HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.OpenQuickMenu))]
internal static class RefreshMenuNamesPatch
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        NameSync.ApplyNames();
    }
}
public static class NameSync
{
    public static void ApplyNames()
    {
        var quickMenu = UnityEngine.Object.FindObjectOfType<QuickMenuManager>();
        if (quickMenu?.playerListSlots == null) return;

        foreach (var slot in quickMenu.playerListSlots)
        {
            if (slot == null) continue;

            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player?.playerSteamId == slot.playerSteamId)
                {
                    slot.usernameHeader.text = player.playerUsername;
                    break;
                }
            }
        }
    }
}
