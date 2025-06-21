using Genkit;

using System;
using System.Collections.Generic;

using UD_SacredWellHole;

using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders
{
    public class UD_SubGrandCathedralBuilder : ZoneBuilderSandbox
    {
        public static string Inner = $"{nameof(Inner)}";
        public static string Outer = $"{nameof(Outer)}";
        

        public Dictionary<string, Dictionary<string, List<Cell>>> Regions;

        public Location2D StiltWellLocation;
        public List<Location2D> SolidJunkPileCellsBelow;
        public bool WantScrappy;

        public string EmptyMaterial => UD_SubStiltBuilderExtension.EmptyMaterial;

        public UD_SubGrandCathedralBuilder()
        {
            StiltWellLocation = new(38, 12);
        }
        public UD_SubGrandCathedralBuilder(Location2D StiltWellLocation = null)
            : this ()
        {
            this.StiltWellLocation = StiltWellLocation ?? new(38, 12);
        }
        public bool BuildZone(Zone Z)
        {
            zone = Z;

            ZoneManager zoneManager = The.ZoneManager;

            UnityEngine.Debug.LogError($"{nameof(UD_SubGrandCathedralBuilder)}, {nameof(zone)}: {zone.ZoneID}");
            
            zone.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("SixDayStiltTile"));

            Cell stiltWellCell = zone.GetCell(StiltWellLocation);
            int strataFromBottom = 20 - zone.Z;
            int strataFromTop = 10 - strataFromBottom;
            string floorMaterial = strataFromBottom > 0 ? "BlackMarbleWalkway" : null;

            UnityEngine.Debug.LogError($"    " +
                $"zone.Z: {zone.Z}, " +
                $"{nameof(strataFromBottom)}: {strataFromBottom}, " +
                $"{nameof(strataFromTop)}: {strataFromTop}");

            UnityEngine.Debug.LogError($"    {nameof(floorMaterial)}: {floorMaterial}");

            foreach (Cell cell in zone.GetCells())
            {
                if (!cell.HasObject(GO => GO.GetBlueprint().InheritsFrom("Stairs")))
                {
                    string TileColor = Stat.RollCached("1d4") switch
                    {
                        4 => "w",
                        3 => "r",
                        2 => "c",
                        1 => "g",
                        _ => "y",
                    };
                    string DetailColor = Stat.RollCached("1d4") switch
                    {
                        4 => "w",
                        3 => "r",
                        2 => "c",
                        1 => "g",
                        _ => "y",
                    };
                    List<GameObject> floors = Event.NewGameObjectList(cell.GetObjectsThatInheritFrom("Floor"));
                    foreach (GameObject floor in floors)
                    {
                        cell.RemoveObject(floor);
                    }
                    PaintCell(cell, floorMaterial, TileColor, DetailColor, Overwrite: true);
                }
            }

            MostlySolidMaterial mostlySolidBuilder = new(Material: "Sandstone");
            if (strataFromTop < 6)
            {
                UnityEngine.Debug.LogError($"    {nameof(mostlySolidBuilder)}: All Cells");
                mostlySolidBuilder.Cells.AddRange(Z.GetCells());
            }
            else
            {
                UnityEngine.Debug.LogError($"    {nameof(mostlySolidBuilder)}: Filtered Cells");
                mostlySolidBuilder.Cells.AddRange(
                    zone.GetCells(c => 
                        !c.HasObject(GO => 
                               GO.GetBlueprint().InheritsFrom("BaseScrapWall")
                            || GO.GetBlueprint().InheritsFrom("Widget")
                            || GO.Blueprint == "Air"
                            || GO.Blueprint == "Timecube"
                            )));
            }
            mostlySolidBuilder.Cells.RemoveAll(c => c == stiltWellCell);
            mostlySolidBuilder.BuildZone(zone);
            
            if (strataFromTop < 6)
            {
                UnityEngine.Debug.LogError($"    {nameof(Cell)}.{nameof(Cell.Clear)}()");
                stiltWellCell.Clear(Combat: true);
            }
            if (strataFromTop < 5)
            {
                UnityEngine.Debug.LogError($"    {nameof(Cell)}.{nameof(Cell.AddObject)}({EmptyMaterial})");
                stiltWellCell.AddObject(EmptyMaterial);
            }
            stiltWellCell.AddObject("FlyingWhitelistArea");
            stiltWellCell.AddObject("DaylightCell");

            foreach (Cell daylightCell in zone.GetCellsWithObject("DaylightCell"))
            {
                if (daylightCell != null && !daylightCell.HasObjectWithBlueprint("FlyingWhitelistArea"))
                {
                    daylightCell.AddObject("FlyingWhitelistArea");
                }
            }

            int airRadius = Math.Max(0, (int)(strataFromTop * 1.85));

            Dictionary<string, List<Cell>> openAirRegion = stiltWellCell.GetCircleRegion(airRadius, Filter: c => c != stiltWellCell);
            if (!openAirRegion.IsNullOrEmpty())
            {
                if (openAirRegion.ContainsKey(Inner))
                {
                    openAirRegion[Inner].RemoveAll(c => c == stiltWellCell);
                    if (strataFromBottom < 5)
                    {
                        openAirRegion[Inner].RemoveAll(c => !c.HasObjectWithBlueprint("Sandstone"));
                    }
                }
                List<Cell> openAirCells = Event.NewCellList(openAirRegion.AllCellsInRegion());
                List<Cell> cellsToAddToOuter = Event.NewCellList();
                foreach ((string subregionLabel, List<Cell> subregionCells) in openAirRegion)
                {
                    if (subregionLabel == Inner || subregionLabel == Outer)
                    {
                        List<Cell> cellsAlreadyRolled = Event.NewCellList();
                        foreach (Cell cell in subregionCells)
                        {
                            cell.Clear();
                            if (strataFromBottom > 0 && subregionLabel == Inner)
                            {
                                cell.AddObject(EmptyMaterial);
                            }
                            if (subregionLabel == Outer)
                            {
                                if (strataFromBottom > 0)
                                {
                                    cell.AddObject(EmptyMaterial);
                                }
                                foreach (Cell adjacentCell in cell.GetAdjacentCells())
                                {
                                    if (!openAirCells.Contains(adjacentCell))
                                    {
                                        if (!cellsAlreadyRolled.Contains(adjacentCell) && Stat.RollCached("1d3") == 1)
                                        {
                                            adjacentCell.Clear();
                                            if (strataFromBottom > 0)
                                            {
                                                adjacentCell.AddObject(EmptyMaterial);
                                            }
                                            if (!cellsToAddToOuter.Contains(adjacentCell))
                                            {
                                                cellsToAddToOuter.Add(adjacentCell);
                                            }
                                        }
                                        if (!cellsToAddToOuter.Contains(adjacentCell))
                                        {
                                            cellsAlreadyRolled.Add(adjacentCell);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (!cellsToAddToOuter.IsNullOrEmpty())
                {
                    openAirRegion[Outer].AddRange(cellsToAddToOuter);
                }
                if (strataFromBottom < 4)
                {
                    List<Cell> scrapWallCells = Event.NewCellList(zone.GetCellsWithObject(GO => GO.GetBlueprint().InheritsFrom("BaseScrapWall")));
                    if (!scrapWallCells.IsNullOrEmpty())
                    {
                        foreach (Cell scrapWallCell in scrapWallCells)
                        {
                            if (scrapWallCell.IsInnerCell(
                                Basis: c => c.HasObject(
                                    GO => GO.GetBlueprint().InheritsFrom("BaseScrapWall")
                                    )) || Stat.RollCached("1d10") == 3)
                            {
                                GameObject scrapWall = scrapWallCell.GetFirstObject("BaseScrapWall");
                                if (scrapWall != null && Stat.RollCached("1d4") == 1)
                                {
                                    if (Stat.RollCached("1d2") == 1)
                                    {
                                        scrapWallCell.Clear(alsoExclude: GO => GO != scrapWall);
                                    }
                                    else
                                    {
                                        scrapWall.Die();
                                    }
                                }
                                ;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public static void PaintCell(Cell C, string Floor = null, string TileColor = null, string DetailtColor = null, string Tile = null, bool Overwrite = true)
        {
            string paintColorString = TileColor ?? "y";
            string paintTile = Tile ?? "Tiles/tile-dirt1.png";
            string paintDetailColor = DetailtColor ?? "k";
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
            if (Overwrite || C.PaintTile.IsNullOrEmpty())
            {
                C.PaintColorString = $"&{paintColorString}";
                C.PaintTile = paintTile;
                C.PaintDetailColor = paintDetailColor;
                C.PaintTileColor = $"&{paintTileColor}";
                C.PaintRenderString = paintRenderString;
            }
            floorSample?.Obliterate();
        }
    }
}
