using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using LGTunerAdvanced.Data;
using UnityEngine;

namespace LGTunerAdvanced.Patches;


[HarmonyPatch(typeof(LG_LevelBuilder), nameof(LG_LevelBuilder.PlaceRoot))]
internal static class Inject_BuildGeomorph
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(nameof(LG_LevelBuilder.PlaceRoot))]
    private static void Pre_PlaceRoot(
        LG_Tile tile, 
        ref GameObject tilePrefab, 
        ref bool forceAlignToVector, 
        ref Vector3 alignVector, 
        LG_Zone zone
        )
    {
        forceAlignToVector = alignVector != Vector3.zero;

        if (tile == null) { return; }
        if (tile.m_shape == null) { return; }
        if (!LoadLevelGenerationData.TryGrabGeoOverride(zone, tile.m_shape.m_gridPosition, out GeoOverride geoOverride)) { return; }

        var tempPrefab = AssetAPI.GetLoadedAsset(geoOverride.Geomorph)?.Cast<GameObject>(); 
        if (tempPrefab != null)
        {
            tilePrefab = tempPrefab;
        }

    }
}
