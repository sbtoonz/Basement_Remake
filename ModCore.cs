using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using PieceManager;
using UnityEngine;

using ServerSync;

namespace Basements
{
    [BepInPlugin(HGUIDLower, ModName, version)]
    public class BasementPlugin : BaseUnityPlugin
    {
        public const string version = "1.0.1";
        public const string ModName = "Basements";
        internal const string Author = "RoloPogo";
        private const string HarmonyGUID = "Harmony." + Author + "." + ModName;
        internal const string HGUIDLower = HarmonyGUID;
        ConfigSync configSync = new(HarmonyGUID) 
            { DisplayName = ModName, CurrentVersion = version, MinimumRequiredVersion = version};
        internal static ConfigEntry<int>? MaxNestedLimit;
        private static ConfigEntry<bool> serverConfigLocked = null!;
        public static GameObject basementPrefab { get; private set; }
        private static Harmony? harmony = null;
        private BuildPiece buildPiece;

        private void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            BuildPiece basement = new BuildPiece("basement", "Basement");
            basement.Name.English("Basement");
            basement.Name.Russian("Подвал");
            basement.Description.Russian("Хороший и прохладный подвал для ваших вещей");
            basement.Description.English("A nice cool underground storage room for your things");
            basement.RequiredItems.Add("Stone", 200, false);
            basement.RequiredItems.Add("Wood", 100, false);
            basement.Prefab.AddComponent<Basement>();
            basementPrefab = basement.Prefab.gameObject;
            MaterialReplacer.RegisterGameObjectForMatSwap(basementPrefab, false);
            var piece = basementPrefab.GetComponent<Piece>();
            piece.m_clipEverything = true;
            piece.m_repairPiece = false;
            harmony = new(HarmonyGUID);
            harmony.PatchAll(assembly);

            serverConfigLocked = config("1 - General", "Lock Configuration", true, "If on, the configuration is locked and can be changed by server admins only.");

            MaxNestedLimit = config("General", "Nested Limit", 3,
                "This is the maximum number of nested basements you can have");
        }
        
        public void OnDestroy()
        {
            harmony.UnpatchSelf();
        }
        
        ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);

    }
}