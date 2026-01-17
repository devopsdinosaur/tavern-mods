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
    public static MelonPreferences_Entry<bool> m_staff_free_labor;
    public static MelonPreferences_Entry<bool> m_staff_perfect_skills;
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
        m_staff_free_labor = m_category_staff.CreateEntry("Free Labor", false, description: "Set to true to set staff salary to 0.  Note that the game shows the negative salary text at midnight and adds to balance sheet and achievement calcs, but it does not actually deduct the cash");
        m_staff_perfect_skills = m_category_staff.CreateEntry("Perfect Skills", false, description: "Set to true to set all staff skills to 100.");
        m_staff_infinite_energy = m_category_staff.CreateEntry("Infinite Energy", false, description: "Set to true to give hired staff infinite energy.");
        m_staff_remove_traits = m_category_staff.CreateEntry("Remove Traits", false, description: "Set to true to remove specific traits from staff (specified in the 'Traits to Remove' config var).");
        m_staff_traits_to_remove = m_category_staff.CreateEntry("Traits to Remove", "ClumsyTrait,DirtyFeetTrait,DislikesChefRoleTrait,DislikesDogsbodyRoleTrait,DislikesJanitorRoleTrait,DislikesServerRoleTrait,EasilyBoredTrait,EasilyBribedTrait,FearOfTheDarkTrait,FilthyTrait,MentalBreakStealsMoneyTrait,MessyTrait,SlowpokeTrait,SqueamishTrait,TakesShortErraticBreaksTrait,TickingTimeBombTrait", description: "Comma-separated list of traits to remove from hired staff.  Hit F10 and check the console or <game>/MelonLoader/Latest.log file for the list of all traits and those that apply to your current staff.  Strings are case sensitive and must exactly match.");
    }

    public void late_load() {
        
    }

    public static void on_setting_changed(object sender, EventArgs e) {
		
	}
}