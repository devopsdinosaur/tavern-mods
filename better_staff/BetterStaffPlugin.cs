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
    
    private static string[] m_traits_to_be_removed;
	private static Dictionary<int, int> m_original_salaries = new Dictionary<int, int>();
    private static Dictionary<int, float> m_original_skill_values = new Dictionary<int, float>();
    private static Dictionary<int, List<string>> m_removed_traits = new Dictionary<int, List<string>>();
    private static bool m_is_data_modded = false;
    
    public override void OnInitializeMelon() {
		try {
			this.m_plugin_info = PluginInfo.to_dict();
			m_plugin = this;
			logger = LoggerInstance;
			Settings.Instance.early_load(m_plugin);
			create_nexus_page();
			(m_harmony = new HarmonyLib.Harmony(PluginInfo.GUID)).PatchAll();
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
            m_traits_to_be_removed = valid_names.ToArray();
            if (Settings.m_staff_remove_traits.Value) {
                _info_log($"The following traits will be removed from staff as encountered: {(m_traits_to_be_removed.Length > 0 ? string.Join(", ", m_traits_to_be_removed) : "None")}.");
            }
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

    private static void restore_original_values(Staff staff) {
        try {
            if (!staff.IsHired) {
                return;
            }
            if (m_original_salaries.TryGetValue(staff.GetHashCode(), out int original_salary)) {
                staff.Data.Salary = original_salary;
                staff.RaiseWageChangedEvent();
                //_debug_log($"{staff.GetDisplayName()}.Salary = {staff.Data.Salary}");
            }
            if (Settings.m_staff_perfect_skills.Value) {
                foreach (ActorSkill skill in staff.Skills.ToList()) {
                    if (m_original_skill_values.ContainsKey(skill.GetHashCode())) {
                        skill.MinValue = m_original_skill_values[skill.GetHashCode()];
                        //_debug_log($"{staff.GetDisplayName()}.Skill_{skill.Name} = {skill.MinValue}");
                    }
                }
            }
            if (Settings.m_staff_remove_traits.Value && m_removed_traits.TryGetValue(staff.GetHashCode(), out List<string> removed)) {
                foreach (string trait in removed) {
                    //_debug_log($"{staff.GetDisplayName()}.AddTrait('Gh.Tk.{trait}'");
                    staff.AddTrait("Gh.Tk." + trait);
                }
                m_removed_traits[staff.GetHashCode()] = new List<string>();
            }
        } catch (Exception e) {
            _error_log("** set_modded_values ERROR - " + e);
        }
    }

    private static void set_modded_values(Staff staff) {
        try {
            if (!staff.IsHired) {
                return;
            }
            m_is_data_modded = true;
            if (!m_original_salaries.TryGetValue(staff.GetHashCode(), out int original_salary)) {
                original_salary = m_original_salaries[staff.GetHashCode()] = staff.Data.Salary;
                //_debug_log($"{staff.GetDisplayName()}.OriginalSalary = {original_salary}");
            }
            int prev_salary = staff.Data.Salary;
            int new_salary = Mathf.Max(0, Mathf.FloorToInt(original_salary * Settings.m_staff_salary_multiplier.Value));
            staff.Data.Salary = new_salary;
            if (prev_salary != new_salary) {
                staff.RaiseWageChangedEvent();
            }
            if (Settings.m_staff_infinite_energy.Value) {
                staff.EnergyStat.Value = 100;
            }
            if (Settings.m_staff_perfect_skills.Value) {
                foreach (ActorSkill skill in staff.Skills.ToList()) {
                    if (!m_original_skill_values.ContainsKey(skill.GetHashCode())) {
                        m_original_skill_values[skill.GetHashCode()] = skill.MinValue;
                        //_debug_log($"{staff.GetDisplayName()}.Original_{skill.Name} = {skill.MinValue}");
                    }
                    skill.MinValue = skill.MaxValue;
                }
            }
            if (Settings.m_staff_remove_traits.Value) {
                foreach (GameObjectXTrait trait in staff.Traits.ToList()) {
                    if (m_traits_to_be_removed.Contains(trait.Name)) {
                        trait.AutoRemoveInSeconds = 0;
                        if (!m_removed_traits.ContainsKey(staff.GetHashCode())) {
                            m_removed_traits[staff.GetHashCode()] = new List<string>();
                        }
                        //_debug_log($"{staff.GetDisplayName()}.RemoveTrait({trait.Name})");
                        m_removed_traits[staff.GetHashCode()].Add(trait.Name);
                    }
                }
            }
        } catch (Exception e) {
            _error_log("** set_modded_values ERROR - " + e);
        }
    }

    private static IEnumerator modify_staff_routine() {
        for (; ; ) {
            yield return new WaitForSeconds(1);
			if (!Settings.m_enabled.Value) {
				continue;
			}
			foreach (Staff staff in Staff.AllStaff) {
				set_modded_values(staff);
            }
        }
    }

    private static void restore_staff_for_save() {
        if (!Settings.m_enabled.Value || !m_is_data_modded) {
            return;
        }
        foreach (Staff staff in Staff.AllStaff) {
            restore_original_values(staff);
        }
        m_is_data_modded = false;
    }

    [HarmonyPatch(typeof(SaveLoadManager), "Save", new Type[] { typeof(string), typeof(Il2CppSystem.Action) })]
    class HarmonyPatch_SaveLoadManager_Save1 {
		private static bool Prefix() {
            restore_staff_for_save();
			return true;
		}
	}

    [HarmonyPatch(typeof(SaveLoadManager), "Save", new Type[] { typeof(string), typeof(int), typeof(Il2CppSystem.Action) })]
    class HarmonyPatch_SaveLoadManager_Save2 {
		private static bool Prefix() {
            restore_staff_for_save();
			return true;
		}
	}
}