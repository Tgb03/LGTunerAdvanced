using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;
using FluffyUnderware.DevTools.Extensions;
using GTFO.API.Utilities;
using UnityEngine;

namespace LGTunerAdvanced.Data;


internal class LGTunerAdvancedConfig
{
    public static ConfigEntry<bool> RenderExpanders { get; private set; }
    public static ConfigEntry<string> ExpanderColorHex { get; private set; }
    public static ConfigEntry<string> AreaTextColorHex { get; private set; }

    public static Color ExpanderColor { get; private set; }
    public static Color AreaTextColor { get; private set; }

    internal static void Initialize(ConfigFile cfg)
    {
        RenderExpanders = cfg.Bind("General", "RenderExpanders", false, "Show LG_AreaExpander visually when in the map.");
        ExpanderColorHex = cfg.Bind("General", "ExpanderColor", ToHex(Color.white), "Color of expanders text");
        AreaTextColorHex = cfg.Bind("General", "AreaTextColor", ToHex(new Color(255, 0, 127)), "Color of area text");
    
        ExpanderColor = ParseColor(ExpanderColorHex.Value, Color.white);
        AreaTextColor = ParseColor(AreaTextColorHex.Value, new Color(255, 0, 127));
    }

    private static Color ParseColor(string hex, Color fallback)
    {
        return ColorUtility.TryParseHtmlString(hex, out Color color)
            ? color
            : fallback;
    }

    private static string ToHex(Color color)
    {
        return "#" + ColorUtility.ToHtmlStringRGB(color);
    }
}
