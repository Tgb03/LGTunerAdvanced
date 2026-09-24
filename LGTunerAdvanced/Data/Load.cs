using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LevelGeneration;
using LevelGeneration.Core;

namespace LGTunerAdvanced.Data;

internal class LoadLevelGenerationData
{
    public static Dictionary<uint, LevelGenerationOverride> levelGenerationOverrides = new Dictionary<uint, LevelGenerationOverride>();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        IncludeFields = true,
    };

    public static void LoadDictionary()
    {
        string path = BepInEx.Paths.PluginPath +
            "/Custom" +
            $"/{Plugin.MOD_NAME}";
        
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        Plugin.L.LogInfo($"Loading LevelGenerationOverrides from: {path}");

        foreach (string fileName in Directory.GetFiles(path)) {
            if (fileName.EndsWith("schema.json")) { continue; }

            try
            {
                string text = File.ReadAllText(fileName);

                LevelGenerationOverride level = JsonSerializer.Deserialize<LevelGenerationOverride>(text, _jsonOptions);
                levelGenerationOverrides[level.LevelLayoutID] = level;

                Plugin.L.LogInfo($"     Loaded {level.LevelLayoutID} from {fileName}");
            }
            catch (Exception e) {
                Plugin.L.LogError(e);
                continue;
            };
        }
    }

    private static uint GetLayoutID(LG_Zone zone)
    {
        if (!zone.IsMainDimension)
        {
            return zone.Dimension.DimensionData.LevelLayoutData;
        }

        return zone.Layer.m_type switch
        {
            LG_LayerType.MainLayer => Builder.LevelGenExpedition.LevelLayoutData,
            LG_LayerType.SecondaryLayer => Builder.LevelGenExpedition.SecondaryLayout,
            LG_LayerType.ThirdLayer => Builder.LevelGenExpedition.ThirdLayout,
            _ => throw new ArgumentException($"LG_LayerType of LG_Zone with id: {zone.ID} is out of bounds.")
        };
    }

#nullable enable
    public static bool TryGrabZoneOverride(LG_Zone zone, out ZoneOverride? zoneOverride)
    {
        uint LevelLayoutID = GetLayoutID(zone);

        if (levelGenerationOverrides.TryGetValue(LevelLayoutID, out LevelGenerationOverride? levelGenerationOverride))
        {
            if (zone.IDinLayer >= levelGenerationOverride.ZoneOverrides.Count)
            {
                zoneOverride = null;
                return false;
            }

            zoneOverride = levelGenerationOverride.ZoneOverrides[zone.IDinLayer];
        }
        else
        {
            zoneOverride = null;
        }

        return zoneOverride != null;
    }

    public static bool TryGrabGeoOverride(LG_Zone zone, LG_GridPosition position, out GeoOverride? geoOverride)
    {
        uint LevelLayoutID = GetLayoutID(zone);

        if (levelGenerationOverrides.TryGetValue(LevelLayoutID, out LevelGenerationOverride? levelGenerationOverride))
        {
            foreach (var attempt in levelGenerationOverride.GeoOverrides)
            {
                if (attempt.Position.x == position.x && attempt.Position.z == position.z)
                {
                    geoOverride = attempt;
                    return true;
                }
            }
        }

        geoOverride = null;
        return false;
    }
#nullable disable
}
