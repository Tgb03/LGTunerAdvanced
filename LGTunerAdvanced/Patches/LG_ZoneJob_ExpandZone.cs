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
            if (GrabExpander(zone, zoneOverride, out LG_ZoneExpander newExpander))
            {
                blocked_expanders.Add(newExpander.Pointer);
                Plugin.L.LogMessage("   Succeeded in finding connection for area");

                buildFromExpander = newExpander;
                buildFromArea = buildFromExpander.m_linksFrom;
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
            Plugin.L.LogMessage($"ExpandZone in {zone.ID} originally returned {__result}, status: {zoneOverride.AreaOverrides.Count} vs {zone.m_areas.Count}.");

            if (zone.m_areas.Count < zoneOverride.AreaOverrides.Count)
            {
                __result = LG_ZoneJob_CreateExpandFromData.CoverageResult.NotEnough;
            } else
            {
                __result = LG_ZoneJob_CreateExpandFromData.CoverageResult.PlacedCustomGeomorph;
            }
        }
    }

    private static bool GrabExpander(LG_Zone zone, ZoneOverride zoneOverride, out LG_ZoneExpander lgZoneExpander) 
    {
        if (zone == null || zoneOverride == null)
        {
            Plugin.L.LogError($"   Something ended up null and could not proceed.");

            lgZoneExpander = null;
            return false; 
        }

        if (zone.m_areas.Count >= zoneOverride.AreaOverrides.Count)
        {
            Plugin.L.LogError($"   zone.m_areas.Count > zoneOverride.AreaOverrides.Count: {zone.m_areas.Count} > {zoneOverride.AreaOverrides.Count}");

            lgZoneExpander = null;
            return false;
        }

        try
        {
            Plugin.L.LogWarning($"Zone AREA COUNT NOW: {zone.m_areas.Count}");
            AreaOverride areaOverride = zoneOverride.AreaOverrides[zone.m_areas.Count];

#nullable enable
            // get previous node:
            LG_Cell previous_cell = zone.Dimension.Grid.GetCell(GetRealPos(areaOverride.PreviousCellPosition));
            LG_Area previous_area = previous_cell.m_grouping.m_geoRoot.m_areas[areaOverride.PreviousCellInternalAreaID];

            // check if the next tile is not setup
            if (!built_geos.Contains(areaOverride.TileCellPosition))
            {
                LG_PlugDir dir = GetDir(areaOverride.PreviousCellPosition, areaOverride.TileCellPosition);

                // greenlight first case that builds into this tile
                foreach (var expander in previous_area.m_geomorph.m_plugs)
                {
                    if (blocked_expanders.Contains(expander.Pointer)) { continue; }
                    
                    // I WANT:
                    // previous_cell.m_grouping.m_geoRoot.m_plugs[0].
                    if (expander.m_dir == dir) {
                        built_geos.Add(areaOverride.TileCellPosition);
                        lgZoneExpander = expander;
                        return true;
                    }
                }

                Plugin.L.LogError("Failed to find expander for plug.");
                lgZoneExpander = null;
                return false;
            } 

            LG_Cell current_cell = zone.Dimension.Grid.GetCell(GetRealPos(areaOverride.TileCellPosition));
            LG_Area current_area = current_cell.m_grouping.m_geoRoot.m_areas[areaOverride.InternalAreaID];

            foreach (var p in blocked_expanders)
            {
                Plugin.L.LogError($"        {p}");
            }
            Plugin.L.LogError("");

            foreach (var expander in previous_area.m_zoneExpanders)
            {
                if (blocked_expanders.Contains(expander.Pointer)) { continue; }
                if (expander.m_isZoneBuildBlocked) { continue; }

                Plugin.L.LogDebug($"area is {previous_area.name} check: {expander.m_linksFrom.name} and {expander.m_linksTo.name} with target {current_area.name}");

                if (expander.GetOppositeArea(previous_area) == current_area)
                {
                    lgZoneExpander = expander;
                    return true;
                }
            }

            Plugin.L.LogError($"Failed to find in zone expander for {current_area.name}");
            lgZoneExpander = null;
            return false;
#nullable disable
        }
        catch (Exception e) {
            Plugin.L.LogError(e);
        }

        lgZoneExpander = null;
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
