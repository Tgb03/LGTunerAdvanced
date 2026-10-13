using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using LGTunerAdvanced.Data;

namespace LGTunerAdvanced.Patches;


[HarmonyPatch(typeof(RundownManager), nameof(RundownManager.MasterSelectActiveExpedition))]
internal class SelectActiveExpeditionPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        uint levelLayoutID = RundownManager.ActiveExpedition.LevelLayoutData;
        LoadLevelGenerationData.SetLayoutID(levelLayoutID);
    }
}

