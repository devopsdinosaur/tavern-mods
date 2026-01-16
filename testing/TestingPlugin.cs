using HarmonyLib;
using Il2Cpp;
using Il2CppGh.Tk;
using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Il2CppSystem.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Il2CppGh;
using System.Linq.Expressions;

[assembly: MelonInfo(typeof(TestingPlugin), "Testing", "0.0.1", "devopsdinosaur")]

public static class PluginInfo {

    public static string TITLE;
    public static string NAME;
    public static string SHORT_DESCRIPTION = "For testing only";
    public static string EXTRA_DETAILS = "";
    public static string VERSION;
    public static string AUTHOR;
    public static string GAME_TITLE = "Tavern Keeper";
    public static string GAME = "tavern";
    public static string GUID;
	public static string UNDERSCORE_GUID;
	public static string REPO = GAME + "-mods";

    static PluginInfo() {
        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        MelonInfoAttribute info = assembly.GetCustomAttribute<MelonInfoAttribute>();
        TITLE = info.Name;
        NAME = TITLE.ToLower().Replace(" ", "-");
        VERSION = info.Version;
        AUTHOR = info.Author;
        GUID =  AUTHOR + "." + GAME + "." + NAME;
		UNDERSCORE_GUID = GUID.Replace(".", "_").Replace("-", "_");

	}

    public static Dictionary<string, string> to_dict() {
        Dictionary<string, string> info = new Dictionary<string, string>();
        foreach (FieldInfo field in typeof(PluginInfo).GetFields((BindingFlags) 0xFFFFFFF)) {
            info[field.Name.ToLower()] = (string) field.GetValue(null);
        }
        return info;
    }
}

public class TestingPlugin : DDPlugin {
	private static TestingPlugin m_plugin = null;
	private static HarmonyLib.Harmony m_harmony = null;
    
    public override void OnInitializeMelon() {
		try {
			this.m_plugin_info = PluginInfo.to_dict();
			m_plugin = this;
			logger = LoggerInstance;
			Settings.Instance.early_load(m_plugin);
			create_nexus_page();
			(m_harmony = new HarmonyLib.Harmony(PluginInfo.GUID)).PatchAll();
		} catch (Exception e) {
			_error_log("** OnInitializeMelon FATAL - " + e);
		}
	}

	private static void dump_all_objects() {
		string directory = "C:/tmp/dump_" + Il2CppSystem.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
		Directory.CreateDirectory(directory);
		foreach (string file in Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly)) {
			File.Delete(Path.Combine(directory, file));
		}
		foreach (GameObject obj in SceneManager.GetActiveScene().GetRootGameObjects()) {
			string path = null;
			int counter = 0;
			while (File.Exists(path = Path.Combine(directory, $"{obj.name}_{counter++:D4}.json".Replace('*', '_').Replace(':', '_'))));
			UnityUtils.json_dump(obj.transform, path);
		}
	}

    public override void OnUpdate() {
		try {
            if (Input.GetKeyDown(KeyCode.Backslash)) {
				//dump_all_objects();
				//Application.Quit();
				
			}
			
		} catch (Exception e) {
			_error_log("** OnUpdate ERROR - " + e);
		}
    }

    [HarmonyPatch(typeof(SteamManager), "Awake")]
    class HarmonyPatch_SteamManager_Awake {
		private static bool m_has_run = false;
        private static void Postfix() {
			if (m_has_run) {
				return;
			}
			_info_log("Launching control coroutines.");
			m_has_run = true;
			MelonCoroutines.Start(testing_routine());
        }
    }

    private static IEnumerator testing_routine() {
        for (; ; ) {
            yield return new WaitForSeconds(1);
			if (!Settings.m_enabled.Value) {
				continue;
			}
			try {
				//foreach (Tap tap in Resources.FindObjectsOfTypeAll<Tap>()) {
				//	if (tap.ServiceSource == null || tap.ServiceSource._inventory == null) {
				//		continue;
				//	}
				//	foreach (GameItem item in tap.ServiceSource._inventory._inventory) {
				//		if (item.Amount < item.MaxAmount) {
				//			item.Amount = item.MaxAmount;
				//		}
				//	}
    //            }
                //foreach (Larder_Tile tile in Larder_Tile.AllLarder_Tiles) {
                //    foreach (var kvp in tile._storedItemIds) {
                //        _info_log($"{kvp.Key}: {kvp.Value}");
                //    }
                //}
				foreach (Inventory inventory in Inventory.AllInventories) {
					if (!(inventory.name == "larder_Shelf(Clone)" || inventory.name.StartsWith("Taproom_tap_tier"))) {
						continue;
					}
					foreach (GameItem item in inventory._inventory) {
						if (item.Amount < item.MaxAmount) {
							item.Amount = item.MaxAmount;
						}
					}
				}
			} catch (Exception e) {
				_warn_log("* testing_routine ERROR - " + e);
            }
        }
    }

	private static bool adjust_money(ref int adjustment, string category, string reasonKey) {
		_info_log($"AdjustMoney(adjustment: {adjustment}, category: {category}, reasonKey: {reasonKey})");
		if (category == "Staff") {
			adjustment = 0;
			return false;
		}
		return true;
	}

	//[HarmonyPatch(typeof(GameController), "AdjustMoney", new Type[] { typeof(int), typeof(string), typeof(string), typeof(bool), typeof(bool), typeof(bool) })]
	class HarmonyPatch_GameController_AdjustMoney_1 {
		private static bool Prefix(ref int adjustment, string category, string reasonKey, bool unscaledTime, bool showFloatingText, bool triggerCashAudio) {
			return adjust_money(ref adjustment, category, reasonKey);
		}
	}

	[HarmonyPatch(typeof(GameController), "AdjustMoney", new Type[] { typeof(int), typeof(string), typeof(string), typeof(Vector3), typeof(bool), typeof(bool), typeof(bool) })]
	class HarmonyPatch_GameController_AdjustMoney_2 {
		private static bool Prefix(ref int adjustment, string category, string reasonKey, Vector3 spawnPosition, bool unscaledTime, bool showFloatingText, bool triggerCashAudio) {
			return adjust_money(ref adjustment, category, reasonKey);
		}
	}

    /*
	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static bool Prefix() {
			
			return true;
		}
	}

	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static void Postfix() {
			
		}
	}

	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static bool Prefix() {
			try {

				return false;
			} catch (Exception e) {
				_error_log("** XXXXX.Prefix ERROR - " + e);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static void Postfix() {
			try {
				
			} catch (Exception e) {
				_error_log("** XXXXX.Postfix ERROR - " + e);
			}
		}
	}
	*/
}