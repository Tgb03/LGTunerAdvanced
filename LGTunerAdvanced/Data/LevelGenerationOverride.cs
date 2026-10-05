using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AirNavigation;
using Expedition;
using GameData;
using LevelGeneration;
using LevelGeneration.Core;
using UnityEngine;

namespace LGTunerAdvanced.Data;

[Serializable]
internal class LevelGenerationOverride
{
    public uint LevelLayoutID;
    public List<ZoneOverride> ZoneOverrides = [];
    public List<GeoOverride> GeoOverrides = [];
    public List<Complex> ExtraComplexResourceToLoad = [];

    public bool TryGrabGeoAtPos(LG_GridPosition position, out GeoOverride result)
    {
        foreach (var geoOverride in GeoOverrides)
        {
            if (geoOverride.Position.x == position.x && geoOverride.Position.z == position.z) {
                result = geoOverride;
                return true;
            }
        }

        result = null;
        return false;
    }
}

[Serializable]
internal class ZoneOverride
{
    public List<AreaOverride> AreaOverrides = [];
}

[Serializable]
internal class AreaOverride
{
    public LG_GridPosition PreviousCellPosition;
    public int PreviousCellInternalAreaID;
    public int LG_ExpanderID;
}

[Serializable]
internal class GeoOverride
{
    public LG_GridPosition Position;
    public string Geomorph;
    public float Rotation;
    public int Altitude;
}
