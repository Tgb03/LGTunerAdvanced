using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GTFO.API.Utilities;
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
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static LevelGenerationOverride lastQueried = null;

    public static void SetLayoutID(uint id)
    {
        lastQueried = levelGenerationOverrides[id];
    }

    public static void ResolveLoading()
    {
        string rootPath = BepInEx.Paths.PluginPath;
        string[] directories = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories);
        var matches = directories
            .Where(d => d.Contains(
                Path.Combine("Custom", "LGTunerAdvanced"),
                StringComparison.OrdinalIgnoreCase))
            .OrderBy(Path.GetDirectoryName);

        foreach (string file in matches)
        {
            LoadDictionary(file);
        }
    }

    public static void LoadDictionary(string filePath)
    {
        Plugin.L.LogInfo($"Loading LevelGenerationOverrides from: {filePath}");
        var dir = Directory.CreateDirectory(filePath);

        foreach (var fileInfo in dir.GetFiles()) {
            var extension = fileInfo.Extension;

            if (fileInfo.Name.EndsWith("schema.json")) { continue; }
            if (extension != ".json" && extension != ".jsonc") { continue; }

            try
            {
                string text = File.ReadAllText(fileInfo.FullName);

                LevelGenerationOverride level = JsonSerializer.Deserialize<LevelGenerationOverride>(text, _jsonOptions);
                levelGenerationOverrides[level.LevelLayoutID] = level;

                Plugin.L.LogInfo($"     Loaded {level.LevelLayoutID} from {fileInfo.FullName}");
            }
            catch (Exception e) {
                Plugin.L.LogError(e);
                continue;
            };
        }

        var liveEdit = LiveEdit.CreateListener(filePath, "*.*", true);
        liveEdit.FileChanged += LiveEditFileChanged;
        liveEdit.StartListen();
    }

    private static void LiveEditFileChanged(LiveEditEventArgs e)
    {
        var path = e.FullPath;
        if (!path.Contains("LGTunerAdvanced")) { return; }
        string ext = Path.GetExtension(path).ToLowerInvariant();
        if (ext != ".json" && ext != ".jsonc") { return; }

        try
        {
            string text = File.ReadAllText(path);
            LevelGenerationOverride level = JsonSerializer.Deserialize<LevelGenerationOverride>(text, _jsonOptions);
            levelGenerationOverrides[level.LevelLayoutID] = level;

            Plugin.L.LogInfo($"     Loaded {level.LevelLayoutID} from {e.FileName}");
        }
        catch (Exception error)
        {
            Plugin.L.LogError(error);
        };
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

            if (levelGenerationOverride.ZoneOverrides[zone.IDinLayer].IgnoreZone)
            {
                zoneOverride = null;
                return false;
            }

            zoneOverride = levelGenerationOverride.ZoneOverrides[zone.IDinLayer];
            lastQueried = levelGenerationOverride;
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
                // Plugin.L.LogMessage($"Attempting to get: {position.x - 20} {position.z - 20} vs {attempt.Position.x} {attempt.Position.z}");
                if (attempt.Position.x == position.x - 20 && attempt.Position.z == position.z - 20)
                {
                    geoOverride = attempt;
                    lastQueried = levelGenerationOverride;
                    return true;
                }
            }
        }

        geoOverride = null;
        return false;
    }

    public static bool TryGrabLastQueried(out LevelGenerationOverride? levelOverride)
    {
        levelOverride = lastQueried;
        return levelOverride != null;
    }
#nullable disable
}
