using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AirNavigation;
using GameData;
using LevelGeneration;
using LevelGeneration.Core;
using UnityEngine;

namespace LGTunerAdvanced.Data;

[Serializable]
internal class LevelGenerationOverride
{
    public uint LevelLayoutID;
    public List<ZoneOverride> ZoneOverrides;
    public List<GeoOverride> GeoOverrides;
}

[Serializable]
internal class ZoneOverride
{
    public List<AreaOverride> AreaOverrides;
}

[Serializable]
internal class AreaOverride
{
    public LG_GridPosition PreviousCellPosition;
    public int PreviousCellInternalAreaID;
    public LG_GridPosition TileCellPosition;
    public int InternalAreaID;
}

[Serializable]
internal class GeoOverride
{
    public LG_GridPosition Position;
    public string Geomorph;
    public float Rotation;
}
