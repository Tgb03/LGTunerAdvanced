# LGTunerAdvanced

Changes GTFO to use a custom level builder rather than vanilla RNG heavy system.

## How it works: 

It overrides the expansion of each zone with the declared Tile position and room identification in the geomorph asset.
One thing to note is this mod does not allow changing the first area of any dimension because the game handles that
specific area in a vastly different way.

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
          "PreviousCellInternalAreaID": 5,
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
          "PreviousCellInternalAreaID": 5,
          // we want to use the LG_Expander with id 1 this time to generate next zone.
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
      "Geomorph": "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Storage/geo_64x64_mining_storage_HA_06.prefab"
    }
  ]
}
```