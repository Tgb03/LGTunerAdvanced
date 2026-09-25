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

        if (tile == null) {
            Plugin.L.LogError("PlaceRoot failed because tile variable is null");
            return; 
        }
        if (tile.m_shape == null) {
            Plugin.L.LogError("PlaceRoot failed because tile.m_shape variable is null");
            return; 
        }

        if (!LoadLevelGenerationData.TryGrabGeoOverride(zone, tile.m_shape.m_gridPosition, out GeoOverride geoOverride))
        {
            Plugin.L.LogError($"PlaceRoot failed because could not find geoOverride for {tile.m_shape.m_gridPosition.x}, {tile.m_shape.m_gridPosition.z}");
            return; 
        }

        var tempPrefab = AssetAPI.GetLoadedAsset(geoOverride.Geomorph)?.Cast<GameObject>(); 
        if (tempPrefab != null)
        {
            tilePrefab = tempPrefab;
        }
    }

    [HarmonyPostfix]
    [HarmonyWrapSafe]
    [HarmonyPatch(nameof(LG_LevelBuilder.PlaceRoot))]
    private static void Post_PlaceRoot(
        LG_Tile tile,
        LG_Zone zone,
        LG_Geomorph __result
        )
    {
        var tileObject = __result.gameObject;

        if (tile == null)
        {
            Plugin.L.LogError("PlaceRoot failed because tile variable is null");
            return;
        }
        if (tile.m_shape == null)
        {
            Plugin.L.LogError("PlaceRoot failed because tile.m_shape variable is null");
            return;
        }

        if (!LoadLevelGenerationData.TryGrabGeoOverride(zone, tile.m_shape.m_gridPosition, out GeoOverride geoOverride))
        {
            Plugin.L.LogError($"PlaceRoot failed because could not find geoOverride for {tile.m_shape.m_gridPosition.x}, {tile.m_shape.m_gridPosition.z}");
            return;
        }

        tileObject.transform.rotation = Quaternion.Euler(0, geoOverride.Rotation, 0);
        __result.SetPlaced();
    }
}
