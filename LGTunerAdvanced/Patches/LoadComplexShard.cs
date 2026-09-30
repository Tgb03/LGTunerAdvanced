using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetShards;
using HarmonyLib;
using LevelGeneration;
using LGTunerAdvanced.Data;

namespace LGTunerAdvanced.Patches;

[HarmonyPatch(typeof(LG_LoadComplexDataSetResourcesJob), nameof(LG_LoadComplexDataSetResourcesJob.Build))]
internal class LoadComplexShard
{
    private static int _waitingShared = 0;

    [HarmonyWrapSafe]
    [HarmonyPrefix]
    private static void Prefix(LG_LoadComplexDataSetResourcesJob __instance)
    {
        if (__instance.m_loadingStarted) return;
        if (!LoadLevelGenerationData.TryGrabLastQueried(out LevelGenerationOverride levelOverride)) return;

        foreach (var complex in levelOverride.ExtraComplexResourceToLoad)
        {
            var shardToLoad = complex switch
            {
                Expedition.Complex.Mining => AssetBundleName.Complex_Mining,
                Expedition.Complex.Service => AssetBundleName.Complex_Service,
                Expedition.Complex.Tech => AssetBundleName.Complex_Tech,
                _ => AssetBundleName.None,
            };

            if (shardToLoad != AssetBundleName.None)
            {
                AssetShardManager.LoadAllShardsForBundleAsync(shardToLoad, new Action(Loaded));
                _waitingShared++;
            }
        }
    }

    private static void Loaded()
    {
        _waitingShared--;
    }

    [HarmonyWrapSafe]
    [HarmonyPostfix]
    private static void Postfix(LG_LoadComplexDataSetResourcesJob __instance, ref bool __result)
    {
        if (_waitingShared > 0)
        {
            __result = false;
        }
    }
}
