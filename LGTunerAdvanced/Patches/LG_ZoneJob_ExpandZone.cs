using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using LevelGeneration;
using LevelGeneration.Core;
using LGTunerAdvanced.Data;

namespace LGTunerAdvanced.Patches;



[HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.ExpandZone))]
public static class LG_ZoneJob_CreateExpandFromDataPatch
{
    internal static HashSet<LG_GridPosition> built_geos = new();

    [HarmonyPrefix]
    public static void Prefix(
        LG_Zone zone,
        ref LG_Area buildFromArea,
        ref LG_ZoneExpander buildFromExpander,
        uint seed)
    {
        if (LoadLevelGenerationData.TryGrab(zone, out ZoneOverride zoneOverride))
        {
            if (GrabExpander(zone, zoneOverride, out LG_ZoneExpander newExpander))
            {
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
        if (LoadLevelGenerationData.TryGrab(zone, out ZoneOverride zoneOverride))
        {
            Plugin.L.LogMessage($"ExpandZone in {zone.ID} originally returned {__result}, status: {zoneOverride.AreaOverrides.Count} vs {zone.m_areas.Count}.");

            if (zone.m_areas.Count - 1 < zoneOverride.AreaOverrides.Count)
            {
                __result = LG_ZoneJob_CreateExpandFromData.CoverageResult.NotEnough;
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

        if (zone.m_areas.Count > zoneOverride.AreaOverrides.Count)
        {
            Plugin.L.LogError($"   zone.m_areas.Count > zoneOverride.AreaOverrides.Count: {zone.m_areas.Count} > {zoneOverride.AreaOverrides.Count}");

            lgZoneExpander = null;
            return false;
        }

        try
        {
            Plugin.L.LogWarning($"Zone AREA COUNT NOW: {zone.m_areas.Count}");
            AreaOverride areaOverride = zoneOverride.AreaOverrides[zone.m_areas.Count - 1];

            if (!built_geos.Contains(areaOverride.TileCellPosition))
            {

            }

            uint checked_count = 0;
            foreach (LG_Tile tile in zone.Dimension.Tiles)
            {
                if (tile == null) { continue; }

                foreach (LG_Area area in tile.m_geoRoot.m_areas)
                {
                    if (area == null) { continue; }

                    foreach (LG_ZoneExpander zoneExpander in area.m_zoneExpanders)
                    {
                        if (zoneExpander == null) { continue; }

                        checked_count += 1;
                        Plugin.L.LogWarning($"Link from name: {zoneExpander.m_linksFrom.name} to {zoneExpander.m_linksTo.name}");
                        
                        if (zoneExpander.m_linksTo.name == areaOverride.InternalAreaID)
                        {
                            lgZoneExpander = zoneExpander;
                            return true;
                        }
                    }
                }
            }

            Plugin.L.LogError($"   Did not find a single eligible LG_ZoneExpander. Checked: {checked_count} options");
        }
        catch (Exception e) {
            Plugin.L.LogError(e);
        }

        lgZoneExpander = null;
        return false;
    }
}
