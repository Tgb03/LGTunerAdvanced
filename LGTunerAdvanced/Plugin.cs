using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Expedition;
using GameData;
using HarmonyLib;
using LevelGeneration;
using LevelGeneration.Core;
using LGTunerAdvanced;
using LGTunerAdvanced.Data;
using LGTunerAdvanced.Patches;
using UnityEngine;

[assembly: AssemblyVersion(Plugin.VERSION)]
[assembly: AssemblyFileVersion(Plugin.VERSION)]
[assembly: AssemblyInformationalVersion(Plugin.VERSION)]

namespace LGTunerAdvanced;

[BepInPlugin(GUID, MOD_NAME, VERSION)]
[BepInDependency("dev.gtfomodding.gtfo-api")]
public class Plugin : BasePlugin
{
    public const string GUID = "dev.Tgb03.gtfo.LGTunerAdvanced";
    public const string MOD_NAME = ManifestInfo.TSName;
    public const string VERSION = ManifestInfo.TSVersion;

    internal static ManualLogSource L;
    
    private static readonly Harmony _harmony = new(GUID);
    
    public override void Load()
    {
        L = Log;

        _harmony.PatchAll(Assembly.GetExecutingAssembly());
        LoadLevelGenerationData.LoadDictionary();

        // LG_Factory.Current.m_currentBatch.Jobs.Enqueue(new LG_ZoneJob_CreateExpandFromData());

        L.LogInfo("Plugin loaded!");
    }
}

[HarmonyPatch(typeof(Builder), nameof(Builder.Build))]
public static class BuildResetter
{
    [HarmonyPrefix]
    public static void Prefix()
    {
        LG_ZoneJob_CreateExpandFromDataPatch.built_geos.Clear();
        LG_ZoneJob_CreateExpandFromDataPatch.built_geos.Add(new LG_GridPosition(0, 0));
    }
}