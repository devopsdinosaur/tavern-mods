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

[assembly: MelonInfo(typeof(BetterStaffPlugin), "Better Staff", "0.0.1", "devopsdinosaur")]

public static class PluginInfo {

    public static string TITLE;
    public static string NAME;
    public static string SHORT_DESCRIPTION = "Remove bad traits, improve skills, infinite energy, and more!";
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
        NAME = TITLE.ToLower().Replace(" ", "_");
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

public class BetterStaffPlugin : DDPlugin {
	private static BetterStaffPlugin m_plugin = null;
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

	private void dump_staff_traits() {
		_info_log("\n\n== All Possible Staff Traits ==\n");
        Type base_type = typeof(StaffTrait);
        foreach (Type type in base_type.Assembly.GetTypes()) {
            if (type.IsSubclassOf(base_type)) {
                _info_log($"--> {type.Name}");
            }
        }
		_info_log("\n\n== Hired + Available Staff Traits ==\n");
        foreach (Staff staff in Staff.AllStaff) {
            _info_log($"--> {staff.GetDisplayName()} ({(staff.IsHired ? "Hired" : "Available")}");
            foreach (GameObjectXTrait trait in staff.Traits.ToList()) {
				_info_log($"    .. {trait.Name}"); 
            }
        }
    }

    public override void OnUpdate() {
		try {
            if (Input.GetKeyDown(KeyCode.F10)) {
				dump_staff_traits();
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
			MelonCoroutines.Start(modify_staff_routine());
        }
    }

    private static IEnumerator modify_staff_routine() {
        Dictionary<int, int> original_salaries = new Dictionary<int, int>();
		Dictionary<int, float> original_skills = new Dictionary<int, float>();
        Type base_type = typeof(StaffTrait);
		List<string> names = new List<string>();
		foreach (string _word in Settings.m_staff_traits_to_remove.Value.Split(',')) {
			string word = _word.Trim();
			if (!string.IsNullOrEmpty(word)) {
				names.Add(word);
			}
		}
		List<string> valid_names = new List<string>();
		foreach (Type type in base_type.Assembly.GetTypes()) {
            if (type.IsSubclassOf(base_type)) {
				if (names.Contains(type.Name)) {
					valid_names.Add(type.Name);
				}
            }
        }
		string[] traits_to_be_removed = valid_names.ToArray();
		if (Settings.m_staff_remove_traits.Value) {
			_info_log($"The following traits will be removed from staff as encountered: {(traits_to_be_removed.Length > 0 ? string.Join(", ", traits_to_be_removed) : "None")}.");
		}
        for (; ; ) {
            yield return new WaitForSeconds(1);
			if (!Settings.m_enabled.Value) {
				continue;
			}
			try {
				foreach (Staff staff in Staff.AllStaff) {
					if (!staff.IsHired) {
						continue;
					}
					if (!original_salaries.TryGetValue(staff.GetHashCode(), out int original_salary)) {
						original_salary = original_salaries[staff.GetHashCode()] = staff.Data.Salary;
					}
					staff.Data.Salary = Mathf.Max(0, Mathf.FloorToInt(original_salary * Settings.m_staff_salary_multiplier.Value));
                    if (Settings.m_staff_infinite_energy.Value) {
						staff.EnergyStat.Value = 100;
					}
					if (Settings.m_staff_perfect_skills.Value) {
						foreach (ActorSkill skill in staff.Skills.ToList()) {
							skill.MinValue = skill.MaxValue;
						}
					}
					if (Settings.m_staff_remove_traits.Value) {
						foreach (GameObjectXTrait trait in staff.Traits.ToList()) {
							if (traits_to_be_removed.Contains(trait.Name)) {
								trait.AutoRemoveInSeconds = 0;
							}
						}
					}
                }
			} catch (Exception e) {
				_warn_log("* modify_staff_routine ERROR - " + e);
            }
        }
    }
}