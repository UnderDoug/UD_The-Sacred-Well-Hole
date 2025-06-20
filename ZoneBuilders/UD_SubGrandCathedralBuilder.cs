using Genkit;
using Qud.API;
using System;
using System.Collections.Generic;
using System.Linq;
using UD_SacredWellHole;
using XRL.EditorFormats.Screen;
using XRL.Rules;
using XRL.World.AI.Pathfinding;
using XRL.World.Effects;
using XRL.World.ObjectBuilders;
using XRL.World.Parts;
using static XRL.Core.XRLCore;

namespace XRL.World.ZoneBuilders
{
    public class UD_SubGrandCathedralBuilder : ZoneBuilderSandbox
    {
        public const string JUNKPILE_SOLID_CELLS_PROP = "Junk Pile Solid Cells List";

        public Location2D StiltWellLocation;
        public bool WantScrappy;

        public UD_SubGrandCathedralBuilder()
        {
            StiltWellLocation = new(38, 12);
            WantScrappy = false;
        }
        public UD_SubGrandCathedralBuilder(Location2D StiltWellLocation = null, bool? WantScrappy = null)
        {
            this.StiltWellLocation = StiltWellLocation ?? new(38, 12);
            this.WantScrappy = WantScrappy ?? false;
        }

        public bool BuildZone(Zone Z, Location2D StiltWellLocation = null, bool? WantScrappy = null)
        {
            this.StiltWellLocation = StiltWellLocation ?? this.StiltWellLocation;
            this.WantScrappy = WantScrappy ?? this.WantScrappy;

            return BuildZone(Z);
        }
        public bool BuildZone(Zone Z)
        {
            zone = Z;

            UnityEngine.Debug.LogError($"{nameof(UD_SubGrandCathedralBuilder)}, {nameof(zone)}: {zone.ZoneID}");

            new SolidMaterial("Sandstone").BuildZone(zone);

            Cell stillWellCell = zone.GetCell(StiltWellLocation).Clear();
            int strataFromBottom = 20 - zone.Z;
            int strataFromTop = 10 - strataFromBottom;
            int strataFromCenter = Math.Abs(5 - strataFromBottom);
            if (strataFromBottom > 4)
            {
                stillWellCell.AddObject("Air");
            }

            Dictionary<string, List<Cell>> openAirRegion = new();
            int airRadius = strataFromTop;
            int airX1 = stillWellCell.X - airRadius;
            int airY1 = stillWellCell.Y - airRadius;
            int airX2 = stillWellCell.X + airRadius;
            int airY2 = stillWellCell.Y + airRadius; 
            Rect2D airR = new(airX1, airY1, airX2, airY2);
            if (airRadius > 0)
            {
                openAirRegion = zone.GetHutRegion(airR, Round: true);
            }
            openAirRegion.Add(nameof(StiltWellLocation), new() { stillWellCell });


            string junkPileSolidCellsListString = "";
            zone.SetZoneProperty(JUNKPILE_SOLID_CELLS_PROP, junkPileSolidCellsListString);

            UnityEngine.Debug.LogError($"     {nameof(WantScrappy)}: {WantScrappy}");

            if (WantScrappy)
            {
                new UD_SubGrandCathedralScrapifier(true).BuildZone(zone);
            }

            return true;

        }

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
    }
}
