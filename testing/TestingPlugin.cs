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
				foreach (Staff staff in Staff.AllStaff) {
					if (!staff.IsHired) {
						continue;
					}
					staff.Data.Salary = 0;
					staff.EnergyStat.Value = 100;
					foreach (GameObjectXTrait trait in staff.Traits.ToList()) {
						if (trait is DislikesJanitorRoleTrait) {
							_info_log("!!!!!!!!!!!");
						}
					}
					foreach (ActorSkill skill in staff.Skills.ToList()) {
						skill.MinValue = skill.MaxValue;
						_info_log(skill.EffectiveValue);
					}
				}
			}
			
		} catch (Exception e) {
			_error_log("** OnUpdate ERROR - " + e);
		}
    }
	
	//[HarmonyPatch(typeof(Staff), "GetSkill")]
	//class HarmonyPatch_Staff_GetSkill {
	//	private static void Postfix(string role, ref ActorSkill __result) {
	//		_info_log($"GetSkill - {role}: {__result.EffectiveValue}");
	//	}
	//}

	//[HarmonyPatch(typeof(Staff), "GetSkillValue")]
	//class HarmonyPatch_Staff_GetSkillValue {
	//	private static void Postfix(string role, ref float __result) {
	//		_info_log($"GetSkillValue - {role}: {__result}");
	//	}
	//}

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