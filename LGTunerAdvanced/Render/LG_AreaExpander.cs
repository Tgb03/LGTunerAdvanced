using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppInterop.Runtime.Injection;
using LevelGeneration.Core;
using LGTunerAdvanced.Data;
using Player;
using UnityEngine;

namespace LGTunerAdvanced.Render;

internal class LG_AreaExpanderRender: MonoBehaviour
{
    private static PlayerAgent agent;

    public LG_AreaExpanderRender(IntPtr ptr) : base(ptr) { }

    public LG_AreaExpanderRender() { }

    public static void Register()
    {
        ClassInjector.RegisterTypeInIl2Cpp<LG_AreaExpanderRender>();
    }

    public static void AddRenderer()
    {
        var playerAgent = PlayerManager.GetLocalPlayerAgent();
        var component = playerAgent.gameObject.GetComponent<LG_AreaExpanderRender>();
        if (component == null) {
            playerAgent.gameObject.AddComponent<LG_AreaExpanderRender>();
        }
    }

    public void Start()
    {
        agent = PlayerManager.GetLocalPlayerAgent();
    }

    public void Update()
    {
        if (!enabled) { return; }

        var area = agent.m_courseNode.m_area;
        if (area == null || area.m_zoneExpanders == null) { return; }
        var geomorph = area.m_geomorph;
        var area_id_in_geo = 0;
        foreach (var geoArea in geomorph.m_areas)
        {
            if (geoArea.Pointer == area.Pointer) { break; }

            area_id_in_geo++;
        }

        LG_GridPosition gPos = geomorph.m_tile.m_shape.m_gridPosition;
        Fig.DrawText(
                $"Position: ({gPos.x - 20}, {gPos.z - 20})\nInternalAreaID: {area_id_in_geo}", 
                area.transform.position,
                Quaternion.LookRotation(agent.transform.position - area.transform.position) * Quaternion.Euler(0f, 180f, 0f),
                new Vector3(1f, 1f, 1f),
                LGTunerAdvancedConfig.AreaTextColor
            );

        for (int expander_id = 0; expander_id < area.m_zoneExpanders.Count; expander_id++) {
            var expander = area.m_zoneExpanders[expander_id];
            if (expander == null) { continue; }
            var color = expander.m_isZoneBuildBlocked ? Color.red : Color.white;

            Fig.DrawText(
                    expander_id.ToString(), 
                    expander.transform.position, 
                    expander.transform.rotation,
                    new Vector3(5, 5, 5), 
                    LGTunerAdvancedConfig.ExpanderColor
                );
        }
    }
}
