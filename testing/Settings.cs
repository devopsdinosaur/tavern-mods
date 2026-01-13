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
    }

    public void late_load() {
        
    }

    public static void on_setting_changed(object sender, EventArgs e) {
		
	}
}