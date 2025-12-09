using Unity.Netcode;
using HarmonyLib;
namespace ExtendedLateCompany.Patches
{
	[HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.LeaveLobbyAtGameStart))]
	[HarmonyWrapSafe]
	internal static class LeaveLobbyAtGameStart_Patch
	{
		[HarmonyPrefix]
		private static bool Prefix()
		{
			return false;
		}
	}
	[HarmonyPatch(typeof(GameNetworkManager), "ConnectionApproval")]
	[HarmonyWrapSafe]
	internal static class ConnectionApproval_Patch
	{
		[HarmonyPostfix]
		private static void Postfix(ref NetworkManager.ConnectionApprovalRequest request, ref NetworkManager.ConnectionApprovalResponse response)
		{
			if (!NetworkManager.Singleton.IsHost)
			{
				return;
			}
			if (request.ClientNetworkId == NetworkManager.Singleton.LocalClientId)
				return;
			if (LobbyManager.currentLobbyVisible && response.Reason == "Game has already started!")
			{
				ExtendedLateCompany.Logger.LogInfo($"[ELC GNM] Allowing late joiner {request.ClientNetworkId}");
				response.Reason = "";
				response.Approved = true;
			}
		}
	}
}
