using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using LevelGeneration.Core;
using LevelGeneration;

namespace LGTunerAdvanced.Patches;

[HarmonyPatch(typeof(Builder), nameof(Builder.Build))]
public static class BuildResetter
{
    [HarmonyPrefix]
    public static void Prefix()
    {
        LG_ZoneJob_CreateExpandFromDataPatch.built_geos.Clear();
        LG_ZoneJob_CreateExpandFromDataPatch.blocked_expanders.Clear();
        LG_ZoneJob_CreateExpandFromDataPatch.built_geos.Add(new LG_GridPosition(0, 0));
    }
}
