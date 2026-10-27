using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using LevelGeneration.Core;
using LGTunerAdvanced.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LGTunerAdvanced.Patches;

[HarmonyPatch(typeof(LG_BuildPlugBaseJob), nameof(LG_BuildPlugBaseJob.SpawnPlug))]
internal class BuildPlugPatch
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    private static void Prefix(LG_Plug plug, ref GameObject prefab) {
        LG_Zone zone = plug.m_linksFrom.m_zone;
        LG_GridPosition position = plug.m_linksFrom.m_geomorph.m_tile.m_shape.m_gridPosition;

        if (!LoadLevelGenerationData.TryGrabGeoOverride(zone, position, out GeoOverride geoOverride))
        {
            return;
        }

        string newPlugLink = plug.m_dir switch {  
            LG_PlugDir.Up => geoOverride.NorthPlug, 
            LG_PlugDir.Right => geoOverride.EastPlug, 
            LG_PlugDir.Down => geoOverride.SouthPlug, 
            LG_PlugDir.Left => geoOverride.WestPlug, 
            _ => ""
        };

        if (newPlugLink == "") { return; }

        GameObject plugPrefab = AssetAPI.GetLoadedAsset(newPlugLink).Cast<GameObject>();
        prefab = plugPrefab;
        Plugin.L.LogMessage($"Plug prefab replaced with: {newPlugLink}");
    }
}
