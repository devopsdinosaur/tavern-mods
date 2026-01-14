using HarmonyLib;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class Settings {
    private static Settings m_instance = null;
    public static Settings Instance {
        get {
            if (m_instance == null) {
                m_instance = new Settings();
            }
            return m_instance;
        }
    }
    private DDPlugin m_plugin = null;

    // General
    public static MelonPreferences_Category m_category_general;
    public static MelonPreferences_Entry<bool> m_enabled;
    public static MelonPreferences_Entry<string> m_log_level;

    // Staff
    public static MelonPreferences_Category m_category_staff;
    public static MelonPreferences_Entry<float> m_staff_salary_multiplier;
    public static MelonPreferences_Entry<float> m_staff_skills_multiplier;
    public static MelonPreferences_Entry<bool> m_staff_infinite_energy;
    public static MelonPreferences_Entry<bool> m_staff_remove_traits;
    public static MelonPreferences_Entry<string> m_staff_traits_to_remove;

    public MelonPreferences_Entry<T> create_entry<T>(MelonPreferences_Category category, string name, T default_value, string description) {
        return category.CreateEntry(name, default_value, description);
    }

    public void early_load(DDPlugin plugin) {
        this.m_plugin = plugin;
        
        // General
        string category_prefix = plugin.UNDERSCORE_GUID + "_";
        m_category_general = MelonPreferences.CreateCategory(category_prefix + "General");
        m_enabled = m_category_general.CreateEntry("Enabled", true, description: "Set to false to disable this mod.");
        m_log_level = m_category_general.CreateEntry("Log Level", "info", description: "[Advanced] Logging level, one of: 'none' (no logging), 'error' (only errors), 'warn' (errors and warnings), 'info' (normal logging), 'debug' (extra log messages for debugging issues).  Not case sensitive [string, default info].  Debug level not recommended unless you're noticing issues with the mod.  Changes to this setting require an application restart.");
        DDPlugin.set_log_level(m_log_level.Value);

        // Staff
        m_category_staff = MelonPreferences.CreateCategory(category_prefix + "Staff");
        m_staff_salary_multiplier = m_category_staff.CreateEntry("Salary Multiplier", 1f, description: "Multiplier applied to hired staff salaries (float, default 1f [no change]).");
        m_staff_skills_multiplier = m_category_staff.CreateEntry("Skills Multiplier", 1f, description: "Multiplier applied to hired staff skills [i.e. ] (float, default 1f [no change]).  Note that this multiplier applies only to the 'base' value of the skills used in runtime calculations.  Skill values are dynamic and will change based on game state.");
        m_staff_infinite_energy = m_category_staff.CreateEntry("Infinite Energy", false, description: "Set to true to give hired staff infinite energy.");
        m_staff_remove_traits = m_category_staff.CreateEntry("Remove Traits", false, description: "Set to true to remove traits from staff (specified in the 'Traits to Remove' config var).");
        m_staff_traits_to_remove = m_category_staff.CreateEntry("Traits to Remove", "SqueamishTrait,NotARealTrait,,", description: "Comma-separated list of traits to remove from hired staff.  Check the console or <game>/MelonLoader/Latest.log file for the list of traits that apply to your current staff.  Strings are case sensitive and must exactly match.");
    }

    public void late_load() {
        
    }

    public static void on_setting_changed(object sender, EventArgs e) {
		
	}
}