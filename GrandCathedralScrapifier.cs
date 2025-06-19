using System;
using System.Collections.Generic;
using System.Linq;

using Genkit;
using Qud.API;

using XRL.Rules;
using XRL.World.AI.Pathfinding;
using XRL.World.Effects;
using XRL.World.ObjectBuilders;
using XRL.World.Parts;

using static XRL.Core.XRLCore;

namespace XRL.World.ZoneBuilders
{
    public class GrandCathedralScrapifier : ZoneBuilderSandbox
    {
        public const string INNER = "Inner";
        public const string OUTER = "Outer";
        public const string DOOR = "Door";
        public const string POPULATION = "Population";

        public Dictionary<string, Dictionary<string, List<Cell>>> Regions;

        public string GiantID;

        public GrandCathedralScrapifier()
        {
            Regions = new();
            GiantID = "";
        }

        public bool BuildZone(Zone Z)
        {
            zone = Z;
            foreach (GameObject item in zone.GetObjectsThatInheritFrom("Item"))
            {
                if (item != null)
                {
                    // Damage, Rust, or Break most of the items in the well.
                    string randomSeed = Z.ZoneID + item.CurrentCell.Location + item.ID;
                    switch (Stat.SeededRandom(randomSeed, 0, 7000) % 12)
                    {
                        case 0:
                            break;
                        case 1:
                        case 2:
                        case 3:
                            Statistic hitpoints = item.GetStat("Hitpoints");
                            hitpoints.Penalty = hitpoints.BaseValue - 1;
                            break;
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                            item.ApplyEffect(new Rusted());
                            break;
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                        default:
                            item.ApplyEffect(new Broken());
                            break;
                    }
                }
                /*
                foreach (PopulationResult item in PopulationManager.Generate(ContentsTable, "zonetier", Z.NewTier.ToString()))
                {

                    for (int num = 0; num < item.Number; num++)
                    {
                        GameObject gameObject = GameObjectFactory.Factory.CreateObject(item.Blueprint);
                        if (!PlaceObjectInArea(Z, new LocationList(popArea), gameObject, 0, 0, item.Hint))
                        {
                        }
                        else
                        {
                            if (isUnique && item.Blueprint == "Gigantic Oven")
                            {
                                giantOvenCell = gameObject?.CurrentCell;
                            }
                        }
                    }
                }
                */
            }

            return true;
        } //!-- public bool BuildZone(Zone Z)

        public static void PaintCell(Cell C, string Floor = null, bool Overwrite = true)
        {
            string paintColorString = "&y";
            string paintTile = "Tiles/tile-dirt1.png";
            string paintDetailColor = "k";
            string paintTileColor = paintColorString;
            string paintRenderString = "ú";
            GameObject floorSample = GameObjectFactory.Factory.CreateSampleObject(Floor);
            if (floorSample != null && floorSample.TryGetPart(out Render floorRender))
            {
                paintColorString = floorRender.ColorString;
                paintTile = floorRender.Tile;
                paintDetailColor = floorRender.DetailColor;
                paintTileColor = floorRender.TileColor;
                paintRenderString = floorRender.RenderString;
            }
            if (Overwrite || string.IsNullOrEmpty(C.PaintTile))
            {
                C.PaintColorString = paintColorString;
                C.PaintTile = paintTile;
                C.PaintDetailColor = paintDetailColor;
                C.PaintTileColor = paintTileColor;
                C.PaintRenderString = paintRenderString;
            }
        }
    } //!-- public class GiantAbodePopulator : ZoneBuilderSandbox
}
