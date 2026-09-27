using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using LevelGeneration.Core;
using LGTunerAdvanced.Data;
using UnityEngine;
using UnityEngine.Assertions;

namespace LGTunerAdvanced.Patches;



[HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.ExpandZone))]
public static class LG_ZoneJob_CreateExpandFromDataPatch
{
    internal static HashSet<LG_GridPosition> built_geos = [];
    internal static HashSet<IntPtr> blocked_expanders = [];

    [HarmonyPrefix]
    public static void Prefix(
        LG_Zone zone,
        ref LG_Area buildFromArea,
        ref LG_ZoneExpander buildFromExpander,
        uint seed)
    {
        if (LoadLevelGenerationData.TryGrabZoneOverride(zone, out ZoneOverride zoneOverride))
        {
            if (GrabExpander(zone, zoneOverride, out LG_ZoneExpander newExpander, out LG_Area newBuildFromArea))
            {
                blocked_expanders.Add(newExpander.Pointer);
                Plugin.L.LogMessage("   Succeeded in finding connection for area");

                buildFromExpander = newExpander;
                buildFromArea = newBuildFromArea;
            }
        }
    }

    [HarmonyPostfix]
    public static void Postfix(
        LG_Zone zone,
        LG_Area buildFromArea,
        LG_ZoneExpander buildFromExpander,
        uint seed,
        ref LG_Area area,
        ref LG_ZoneJob_CreateExpandFromData.CoverageResult __result)
    {
        if (LoadLevelGenerationData.TryGrabZoneOverride(zone, out ZoneOverride zoneOverride))
        {
            if (zone.m_areas.Count < zoneOverride.AreaOverrides.Count)
            {
                __result = LG_ZoneJob_CreateExpandFromData.CoverageResult.NotEnough;
            } else
            {
                __result = LG_ZoneJob_CreateExpandFromData.CoverageResult.PlacedCustomGeomorph;
            }
        }
    }

    private static bool GrabExpander(LG_Zone zone, ZoneOverride zoneOverride, out LG_ZoneExpander lgZoneExpander, out LG_Area buildFromArea) 
    {
        if (zone == null || zoneOverride == null)
        {
            Plugin.L.LogError($"   Something ended up null and could not proceed.");

            lgZoneExpander = null;
            buildFromArea = null;
            return false; 
        }

        if (zone.m_areas.Count < 0 || zone.m_areas.Count >= zoneOverride.AreaOverrides.Count)
        {
            Plugin.L.LogError($"   zone.m_areas.Count > zoneOverride.AreaOverrides.Count: {zone.m_areas.Count} > {zoneOverride.AreaOverrides.Count}");

            lgZoneExpander = null;
            buildFromArea = null;
            return false;
        }

        try
        {
            AreaOverride areaOverride = zoneOverride.AreaOverrides[zone.m_areas.Count];

#nullable enable
            // get previous node:
            LG_Cell previous_cell = zone.Dimension.Grid.GetCell(GetRealPos(areaOverride.PreviousCellPosition));
            LG_Area previous_area = previous_cell.m_grouping.m_geoRoot.m_areas[areaOverride.PreviousCellInternalAreaID];

            if (areaOverride.LG_ExpanderID < 0 || areaOverride.LG_ExpanderID >= previous_area.m_zoneExpanders.Count)
            {
                Plugin.L.LogError($"   areaOverride.LG_ExpanderID >= previous_area.m_zoneExpanders.Count: {areaOverride.LG_ExpanderID} > {previous_area.m_zoneExpanders.Count}");
                
                lgZoneExpander = null;
                buildFromArea = null;
                return false;
            }

            if (blocked_expanders.Contains(previous_area.m_zoneExpanders[areaOverride.LG_ExpanderID].Pointer))
            {
                Plugin.L.LogError($"   LG_ZoneExpander was already parsed and blocked. This means it cannot be reused");

                lgZoneExpander = null;
                buildFromArea = null;
                return false;
            }

            lgZoneExpander = previous_area.m_zoneExpanders[areaOverride.LG_ExpanderID];
            buildFromArea = previous_area;
            return true;
#nullable disable
        }
        catch (Exception e) {
            Plugin.L.LogError(e);
        }

        lgZoneExpander = null;
        buildFromArea = null;
        return false;
    }

    private static bool PosEquals(LG_GridPosition pos1, LG_GridPosition pos2) {
        return pos1.x == pos2.x && pos1.z == pos2.z;
    }

    private static LG_PlugDir GetDir(LG_GridPosition start, LG_GridPosition end) {
        if (end.x > start.x && end.z == start.z) {
            return LG_PlugDir.Right;
        }

        if (end.x < start.x && end.z == start.z)
        {
            return LG_PlugDir.Left;
        }

        if (end.x == start.x && end.z > start.z) {
            return LG_PlugDir.Up;
        }

        if (end.x == start.x && end.z < start.z) {
            return LG_PlugDir.Down;
        }

        return LG_PlugDir.NotDefined;
    }

    private static LG_GridPosition GetRealPos(LG_GridPosition position)
    {
        position.x += 20;
        position.z += 20;
        return position;
    }
}
