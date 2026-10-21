# LGTunerAdvanced

Changes GTFO to use a custom level builder rather than vanilla RNG heavy system.

## How it works: 

It overrides the expansion of each zone with the declared Tile position and room identification in the geomorph asset.
One thing to note is this mod does not allow changing the first area of any dimension because the game handles that
specific area in a vastly different way.

## Note in regards to original LGTuner

There are currently 2 LGTuner versions, one made by Flowaria, the original, and one by Hirnukuono. This mod aims to 
do allow doing everything that the original LGTuner by Flowaria does but in a different more controllable way. However
this mod will NOT and does NOT do everything that Hirnukuono's version does as his version contains a lot of bugfixes and
changes to geomorphs that I will not include as they fall out of the scope of this project.

As such, if your level in a rundown requires LGTuner, it will work. If you have developed a level using LGTuner and want to
continue developing it using LGTunerAdvanced you may have difficulties as it gets a bit weird with the geomorph overwritting
but it should be fine as long as LGTuner and LGTunerAdvanced are not told to have different geos in the same position. If
you want to develop a new level with LGTunerAdvanced only in a rundown that already contains LGTuner, everything will work as expected.
If you want to develop a level that uses LGTuner only in a rundown that already contains LGTunerAdvanced, everything will 
also work as expected.

If you are instead using Hirnukuono's LGTuner the only difference will be that all the bug fixes and geomorph changes of LGTuner
will be applied no matter what. LGTuner also does some optimizations for loading geomorphs which LGTunerAdvanced does not do so it
is possible to use this fact to create the GeoOverrides in the LGTuner config and the ZoneOverrides in the LGTunerAdvanced config.

## How to use:

- Download the mod.
- Run the game once.
- Open `YourRundownFolder/Custom/LGTunerAdvanced` folder
- Optionally add the `schema.json` file to this folder and link it to be used by any json files in it.
- Create json file with any name
- Write down a data entry with the following format

## File structure:

In the Resources folder of the repository is `schema.json` which can be used in combination with your preferred code editor.

General format: 

```json
{
  "$schema": "schema.json",
  "LevelLayoutID": 495148404, // R1A1
  "ZoneOverrides": [
    {
      // whether or not to ignore this zone completely in the algorithm and use
      // vanilla default, false since we want to dictate exactly how this zone generates
      "IgnoreZone": false,
      // override in first generated zone in this level layout
      "AreaOverrides": [
        {
          // this one gets ignored as it is the first zone of the dimension
          "PreviousCellPosition": {
            "x": 0,
            "z": 0
          },
          "PreviousCellInternalAreaID": 0,
          "LG_ExpanderID": 0
        },
        {
          // the previous room we want to start from is in (0, 0)
          "PreviousCellPosition": {
            "x": 0,
            "z": 0
          },
          // said geomorph also only has 1 area which means internal ID is 0
          "PreviousCellInternalAreaID": 0,
          // we want to use the LG_Expander with id 0 in the array.
          "LG_ExpanderID": 0
        },
        {
          // we know generate from the (0, 1) tile
          "PreviousCellPosition": {
            "x": 0,
            "z": 1
          },
          // area ID is 0 as we only have 1 room generated in this geomorph so far
          "PreviousCellInternalAreaID": 0,
          // we want to use the LG_Expander with id 1 this time to generate next room.
          "LG_ExpanderID": 1
        }
      ]
    }
  ],
  // here we override the geomorphs every time they spawn
  "GeoOverrides": [
    {
      // tile position where you need the overwrite
      "Position": {
        "x": 0,
        "z": 1
      },
      // rotation of tile
      "Rotation": 90,
      // geomorph intended to be used
      "Geomorph": "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Storage/geo_64x64_mining_storage_HA_06.prefab",
      // altitude at which the geomorph must spawn. 0 is "Mid"
      "Altitude": 0
    }
  ],
  // here we tell the game to load ALL the different geomorphs.
  // exact same behaviour as LGTuner
  "ExtraComplexResourceToLoad": [
    "Mining",
    "Service",
    "Tech"
  ]
}
```

### Extra stuff:

A config file is generated on first boot. If you turn on `RenderExpanders` in game it will show you expander 
ids for the room you are in allowing you to create the json file for the level faster without the need for 
Unity or external helpers.
