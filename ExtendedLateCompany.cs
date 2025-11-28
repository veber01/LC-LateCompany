using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
namespace ExtendedLateCompany
{
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
internal class ExtendedLateCompany : BaseUnityPlugin
{
    public static ExtendedLateCompany Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony Harmony { get; set; }
    public void Awake()
    {
		Instance = this;
		Logger = base.Logger;
		Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
		harmony.PatchAll(typeof(ExtendedLateCompany).Assembly);
		Logger.Log(LogLevel.Info, "Extended Late Company has loaded!");

	}
    public static bool LobbyJoinable = true;
    static public void SetLobbyJoinable(bool joinable)
    {
		LobbyJoinable = joinable;
		GameNetworkManager.Instance.SetLobbyJoinable(joinable);
		QuickMenuManager quickMenu = Object.FindObjectOfType<QuickMenuManager>();
		if (quickMenu) quickMenu.inviteFriendsTextAlpha.alpha = joinable ? 1f : 0.2f;
	}
}
}
