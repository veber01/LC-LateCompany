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
		if (ExtendedLateCompany.LobbyJoinable && !GameNetworkManager.Instance.disableSteam) GameNetworkManager.Instance.InviteFriendsUI();
		return false;
	}
}
