using BepInEx;
using BepInEx.Logging;
using ExtendedLateCompany.Patches;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;
using UnityEngine.SceneManagement;
using GameNetcodeStuff;
using Unity.Netcode;

namespace ExtendedLateCompany
{
	[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
	internal class ExtendedLateCompany : BaseUnityPlugin
	{
		public static ConfigEntry<bool> LateJoin;

		public static ExtendedLateCompany Instance { get; private set; } = null!;
		internal new static ManualLogSource Logger { get; private set; } = null!;
		internal static Harmony Harmony { get; set; }
		public void Awake()
		{
			Instance = this;
			Logger = base.Logger;
			Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
			harmony.PatchAll(typeof(ExtendedLateCompany).Assembly);
			Logger.LogInfo("Extended Late Company has loaded!");
			BGReplace.Init();
			SceneManager.sceneLoaded += OnSceneLoaded;

			LateJoin = Config.Bind("LateJoin", "EnableLateJoin", true, "Enable or disable Late Joiners");
		}
		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (scene.name == "MainMenu" || scene.name == "InitScene" || scene.name == "InitSceneLaunchOptions")
			{
				if (DebugUI.Instance != null)
				{
					DebugUI.Instance.HideMenu();
				}
				return;
			}
			if (DebugUI.Instance != null) return;
			GameObject ui = new GameObject("ExtendedLateCompany_UI");
			DontDestroyOnLoad(ui);
			ui.AddComponent<DebugUI>();
		}
	}
}

public class DebugUI : MonoBehaviour
{
	public static DebugUI Instance;
	private static bool _menuOpen;

	private Rect _windowRect = new Rect(1000, 20, 300, 200);

	public static void SetMenuForAll(bool value)
	{
		if (GameNetworkManager.Instance != null && !NetworkManager.Singleton.IsHost)
		{
			_menuOpen = false;
			return;
		}
		if (!Instance)
		{
			var obj = new GameObject("ExtendedLateCompany_UI");
			DontDestroyOnLoad(obj);
			Instance = obj.AddComponent<DebugUI>();
		}

		_menuOpen = value;
		ExtendedLateCompany.ExtendedLateCompany.Logger.LogInfo($"ELC Ui: {_menuOpen}");
	}
	public void HideMenu()
	{
		_menuOpen = false;
	}
	private void Awake()
	{
		if (!NetworkManager.Singleton.IsHost)
		{
			Destroy(this.gameObject);
			return;
		}

		Instance = this;
		this.enabled = true;
	}

	private void OnGUI()
	{
		if (!_menuOpen) return;

		_windowRect = GUILayout.Window(
			0,
			_windowRect,
			DrawWindow,
			"ExtendedLateCompany Config"
		);
	}

	private void DrawWindow(int windowID)
	{
		// Toggles
		ExtendedLateCompany.ExtendedLateCompany.LateJoin.Value = GUILayout.Toggle(
			ExtendedLateCompany.ExtendedLateCompany.LateJoin.Value,
			"Enable Late Joiners"
		);

		if (GUILayout.Button("Apply and Refresh"))
		{
			ExtendedLateCompany.ExtendedLateCompany.Instance.Config.Save();
			ExtendedLateCompany.Patches.LobbyManager.RefreshLobbyVisibility();
		}



		GUI.DragWindow(); // make the window draggable
	}

	// Harmony patches to open/close the menu with the game's QuickMenu
	[HarmonyPatch(typeof(QuickMenuManager))]
	public class QuickMenuPatch
	{
		[HarmonyPatch(nameof(QuickMenuManager.OpenQuickMenu))]
		[HarmonyPostfix]
		public static void OnOpenQuickMenu()
		{
			DebugUI.SetMenuForAll(true);
		}

		[HarmonyPatch(nameof(QuickMenuManager.CloseQuickMenu))]
		[HarmonyPostfix]
		public static void OnCloseQuickMenu()
		{
			DebugUI.SetMenuForAll(false);
		}
	}

}



