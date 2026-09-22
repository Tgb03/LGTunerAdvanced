using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LevelGeneration;

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

#nullable enable
    public static bool TryGrab(LG_Zone zone, out ZoneOverride? zoneOverride)
    {
        // TODO: Fix dimensions, right now only works with REALITY dimension level layouts.

        uint LevelLayoutID = zone.Layer.m_type switch
        {
            LG_LayerType.MainLayer => Builder.LevelGenExpedition.LevelLayoutData,
            LG_LayerType.SecondaryLayer => Builder.LevelGenExpedition.SecondaryLayout,
            LG_LayerType.ThirdLayer => Builder.LevelGenExpedition.ThirdLayout,
            _ => throw new ArgumentException($"LG_LayerType of LG_Zone with id: {zone.ID} is out of bounds.")
        };

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
#nullable disable
}
