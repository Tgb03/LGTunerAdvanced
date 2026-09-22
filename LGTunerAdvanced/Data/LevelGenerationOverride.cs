using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public List<TileOverride> TileOverrides;
}

[Serializable]
internal class ZoneOverride
{
    public List<AreaOverride> AreaOverrides;
}

[Serializable]
internal class AreaOverride
{
    public LG_GridPosition TileCellPosition;
    public string InternalAreaID;
}

[Serializable]
internal class TileOverride
{
    public LG_GridPosition TileCellPosition;
    public string Geomorph;
}
