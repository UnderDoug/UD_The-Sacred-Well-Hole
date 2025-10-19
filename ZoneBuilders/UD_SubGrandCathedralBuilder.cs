using Genkit;
using System;
using System.Collections.Generic;

using XRL.Rules;
using XRL.World.Parts;
using XRL.World.WorldBuilders;

using UD_SacredWellHole;

using static UD_SacredWellHole.Const;
using static UD_SacredWellHole.Options;

namespace XRL.World.ZoneBuilders
{
    public class UD_SubGrandCathedralBuilder : ZoneBuilderSandbox
    {
        private static bool doDebug => getClassDoDebug(nameof(UD_SubGrandCathedralBuilder));
        private static bool getDoDebug(object what = null)
        {
            List<object> doList = new()
            {
                'V',    // Vomit
            };
            List<object> dontList = new()
            {
                'X',    // Trace
            };

            if (what != null && doList.Contains(what))
                return true;

            if (what != null && dontList.Contains(what))
                return false;

            return doDebug;
        }

        public static string Inner = $"{nameof(Inner)}";
        public static string Outer = $"{nameof(Outer)}";

        public Dictionary<string, Dictionary<string, List<Cell>>> Regions;

        public Location2D StiltWellLocation => UD_SubStiltWorldBuilderExtension.StiltWellLocation;

        public string EmptyMaterial = "Air";

        public UD_SubGrandCathedralBuilder()
        {
        }
        public bool BuildZone(Zone Z)
        {
            Debug.Header(4, $"{nameof(UD_SubGrandCathedralBuilder)}", $"{nameof(BuildZone)}({nameof(Z)}: {Z.ZoneID})", Toggle: doDebug);
            int indent = Debug.LastIndent;

            zone = Z;

            EmptyMaterial = zone.Z < 15 ? EmptyMaterial : "QuantumAir";

            ZoneManager zoneManager = The.ZoneManager;

            // zone.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("SixDayStiltTile"));

            Cell stiltWellCell = zone.GetCell(StiltWellLocation);
            int strataFromBottom = 20 - zone.Z;
            int strataFromTop = 10 - strataFromBottom;
            string floorMaterial = strataFromBottom > 0 ? "BlackMarbleWalkway" : null;

            Debug.LoopItem(4, $"{nameof(strataFromBottom)}: {strataFromBottom}, {nameof(strataFromTop)}: {strataFromTop}", Indent: indent + 1, Toggle: getDoDebug());
            Debug.LoopItem(4, $"{nameof(floorMaterial)}: {floorMaterial}, {nameof(strataFromTop)}: {strataFromTop}", Indent: indent + 1, Toggle: getDoDebug());

            List<Cell> allCells = Event.NewCellList(zone.GetCells());
            foreach (Cell cell in allCells)
            {
                if (!cell.HasObject(GO => GO.InheritsFrom("Stairs") || GO.HasPart<QuantumAir>() || GO.HasPart<SolidAir>()) && cell.ParentZone == zone)
                {
                    string TileColor = cell.HasObjectWithBlueprint("Sandstone") ? null : Stat.RollCached("1d6") switch
                    {
                        5 => "w",
                        4 => "r",
                        3 => "c",
                        2 => "g",
                        1 => "K",
                        _ => "y",
                    };
                    string DetailColor = cell.HasObjectWithBlueprint("Sandstone") ? null : Stat.RollCached("1d6") switch
                    {
                        5 => "w",
                        4 => "r",
                        3 => "c",
                        2 => "g",
                        1 => "K",
                        _ => "y",
                    };
                    List<GameObject> floors = Event.NewGameObjectList(cell.GetObjectsThatInheritFrom("Floor"));
                    floors.RemoveAll(GO => GO.HasPart<SolidAir>() || GO.HasPart<QuantumAir>());
                    if (!floors.IsNullOrEmpty())
                    {
                        foreach (GameObject floor in floors)
                        {
                            cell.RemoveObject(floor);
                        }
                    }
                    Cell cellBelow = cell.GetCellFromDirection("D", BuiltOnly: false);
                    GameObject wallBelow = cellBelow.GetFirstObject(GO => GO.InheritsFrom("BaseScrapWall"));
                    if (wallBelow != null && wallBelow.Render != null)
                    {
                        TileColor = wallBelow.Render.TileColor;
                        DetailColor = wallBelow.Render.DetailColor;
                    }
                    bool doFloorMaterial = cellBelow != null && cellBelow.HasObjectWithBlueprintEndsWith("ScrapWall");
                    PaintCell(cell, (doFloorMaterial ? floorMaterial : null), TileColor, DetailColor, Overwrite: true, OverrideFloorColors: true);
                }
            }

            UDSW_MostlySolidMaterial mostlySolidBuilder = new(Material: null, Materials: new() { "SolidAir", "Sandstone" }, ClearFirst: true);
            if (strataFromTop < 6)
            {
                Debug.LoopItem(4, $"{nameof(mostlySolidBuilder)}", $"All Cells, Count: {mostlySolidBuilder.Cells.Count}", Indent: indent + 1, Toggle: getDoDebug());
                mostlySolidBuilder.Cells.AddRange(zone.GetCells());
            }
            else
            {
                List<Cell> solidCells = Event.NewCellList(zone.GetCells());
                solidCells.RemoveAll(c => c.HasObjectInheritsFrom("BaseScrapWall"));
                solidCells.RemoveAll(c => c.HasObjectWithBlueprint(EmptyMaterial));
                solidCells.RemoveAll(c => c.HasObjectWithBlueprint("Air"));
                solidCells.RemoveAll(c => c.HasObjectWithBlueprint("SolidAir"));
                mostlySolidBuilder.Cells.AddRange(solidCells);
                Debug.LoopItem(4, $"{nameof(mostlySolidBuilder)}", $"Filtered Cells, Count: {mostlySolidBuilder.Cells.Count}", Indent: indent + 1, Toggle: getDoDebug());
            }
            if (strataFromBottom == 0)
            {
                mostlySolidBuilder.Materials.Remove("SolidAir");
                Debug.LoopItem(4, $"{nameof(mostlySolidBuilder)}", $"Removed Solid Air from {nameof(mostlySolidBuilder.Materials)}", Indent: indent + 1, Toggle: getDoDebug());
            }
            mostlySolidBuilder.Cells.RemoveAll(c => c == stiltWellCell);
            mostlySolidBuilder.BuildZone(zone);

            if (strataFromTop < 6)
            {
                Debug.LoopItem(4, $"{nameof(Cell)}.{nameof(Cell.Clear)}()", Indent: indent + 1, Toggle: getDoDebug());
                stiltWellCell.Clear(Combat: true);
                if (strataFromTop == 5)
                {
                    stiltWellCell.AddObject("SelfSacrificeHelper");
                    stiltWellCell.AddObject("SpawnBlocker");
                }
            }
            if (strataFromTop < 5)
            {
                Debug.LoopItem(4, $"{nameof(Cell)}.{nameof(Cell.AddObject)}({EmptyMaterial})", Indent: indent + 1, Toggle: getDoDebug());
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

            int airRadius = strataFromTop < 3 ? 1 : Math.Max(1, (int)((strataFromTop - 1) * (strataFromTop * 0.2125f)));

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
                            if (strataFromBottom == 0 && subregionLabel == Inner)
                            {
                                if (cell.IsOuterCell(c => subregionCells.Contains(c)) && !cell.AnyAdjacentCell(c => c.HasObject(GO => GO.GetBlueprint().InheritsFrom("Widget"))) && 3.in10())
                                {
                                    if (2.in10())
                                    {
                                        cell.AddObject("RandomScrapMound");
                                    }
                                    else
                                    {
                                        cell.AddObject("RandomScrapWallSometimesGigantic");
                                    }
                                }
                            }
                            if (subregionLabel == Outer)
                            {
                                if (strataFromBottom > 0)
                                {
                                    cell.AddObject(EmptyMaterial);
                                }
                                else if (strataFromBottom == 0)
                                {
                                    if (2.in10())
                                    {
                                        cell.AddObject("RandomScrapMound");
                                    }
                                    else
                                    {
                                        cell.AddObject("RandomScrapWallSometimesGigantic");
                                    }
                                }
                                foreach (Cell adjacentCell in cell.GetAdjacentCells())
                                {
                                    if (!openAirCells.Contains(adjacentCell))
                                    {
                                        Cell cellAbove = adjacentCell.GetCellFromDirection("U", BuiltOnly: false);
                                        if (false && cellAbove != null && strataFromTop > 1 && strataFromBottom > 1 && !cellAbove.HasObjectWithBlueprint("Sandstone"))
                                        {
                                            adjacentCell.Clear().AddObject(EmptyMaterial);
                                            cellsToAddToOuter.Add(adjacentCell);
                                            cellsAlreadyRolled.Add(adjacentCell);
                                            continue;
                                        }
                                        if (!cellsAlreadyRolled.Contains(adjacentCell) && Stat.RollCached("1d3") == 1)
                                        {
                                            adjacentCell.Clear();
                                            if (strataFromBottom > 0)
                                            {
                                                adjacentCell.AddObject(EmptyMaterial);
                                            }
                                            else if (strataFromBottom == 0)
                                            {
                                                if (2.in10())
                                                {
                                                    adjacentCell.AddObject("RandomScrapMound");
                                                }
                                                else
                                                {
                                                    adjacentCell.AddObject("RandomScrapWallSometimesGigantic");
                                                }
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
                        Debug.LoopItem(4, $"Breaking/Deleting select Scrap Walls and Mounds", Indent: indent + 1, Toggle: getDoDebug());
                        foreach (Cell scrapWallCell in scrapWallCells)
                        {
                            Debug.Divider(4, HONLY, 40, Indent: indent + 2, Toggle: getDoDebug());
                            Debug.LoopItem(4, $"{nameof(scrapWallCell)}: [{scrapWallCell.Location}]", Indent: indent + 2, Toggle: getDoDebug());
                            GameObject scrapWall = scrapWallCell.GetFirstObject(GO => GO.GetBlueprint().InheritsFrom("BaseScrapWall"));
                            if (scrapWall != null && scrapWallCell.IsInnerCell(Basis: c => c.HasObjectWithBlueprintEndsWith("ScrapWall"), false)
                                || Stat.RollCached("1d10") == 3)
                            {
                                Debug.CheckYeh(4, $"{nameof(scrapWallCell)} is Inner (or by 3in10)", Indent: indent + 3, Toggle: getDoDebug());
                                if (Stat.RollCached("1d3") == 1)
                                {
                                    Debug.CheckYeh(4, $"{nameof(scrapWall)} located and 1in3 successful", Indent: indent + 3, Toggle: getDoDebug());
                                    if (Stat.RollCached("1d2") == 1)
                                    {
                                        Debug.LoopItem(4, $"{scrapWall?.DebugName ?? NULL}", $"Removed", Indent: indent + 4, Toggle: getDoDebug());
                                        scrapWallCell.RemoveObject(scrapWall);
                                    }
                                    else
                                    {
                                        Debug.LoopItem(4, $"{scrapWall?.DebugName ?? NULL}", $"Kilt", Indent: indent + 4, Toggle: getDoDebug());
                                        scrapWall.Die();
                                    }
                                }
                                else
                                {
                                    Debug.CheckNah(4, $"{nameof(scrapWall)} not located or 1in3 failed", Indent: indent + 3, Toggle: getDoDebug());
                                }
                            }
                        }
                        Debug.Divider(4, HONLY, 40, Indent: indent + 2, Toggle: getDoDebug());
                    }
                }
            }

            if (zone.Z == 20)
            {
                Debug.LoopItem(4, $"Rolling Tier 2-5 Relic by way of ExplodingDie and setting {nameof(HolyPlace)}", Indent: indent + 1, Toggle: getDoDebug());

                int tier = new DieRoll("1d3").Explode(3, 1, 6);
                GameObject stiltWellRelic = RelicGenerator.GenerateRelic(Tier: tier);
                if (stiltWellRelic != null)
                {
                    Debug.CheckYeh(4, $"{nameof(stiltWellRelic)}", $"{stiltWellRelic.DebugName}", Indent: indent + 2, Toggle: getDoDebug());
                    stiltWellCell.Clear(Combat: true).AddObject(stiltWellRelic);
                }
                else
                {
                    stiltWellCell.AddPopulation("Artifact 8R");
                    Debug.CheckNah(4, $"Relic Failed, Adding from population", $"Artifact 8R", Indent: indent + 2, Toggle: getDoDebug());
                }
                GameObject stiltRecoiler = GameObjectFactory.Factory.CreateObject("Six Day Stilt Recoiler");
                if (stiltRecoiler != null)
                {
                    Debug.CheckYeh(4, $"{nameof(stiltRecoiler)}", $"{stiltRecoiler.DebugName}", Indent: indent + 2, Toggle: getDoDebug());
                    stiltWellCell.AddObject(stiltRecoiler);
                }
                for (int i = 0; i < Stat.RandomCosmetic(1, 3); i++)
                {
                    int wedgeValue = Math.Max(1, Stat.RandomCosmetic(2, 3) - i);
                    string creditWedgeBlueprint = $"CyberneticsCreditWedge" + (wedgeValue == 1 ? "" : wedgeValue);
                    GameObject creditWedge = GameObjectFactory.Factory.CreateObject(creditWedgeBlueprint);
                    if (creditWedge != null)
                    {
                        Debug.CheckYeh(4, $"{nameof(creditWedge)}", $"{creditWedge.DebugName}", Indent: indent + 2, Toggle: getDoDebug());
                        stiltWellCell.AddObject(creditWedge);
                    }
                }
                if (stiltWellCell.AddObject("HolyPlaceWidget").TryGetPart(out HolyPlace holyPlace))
                {
                    holyPlace.Faction = "Mechanimists";
                }
                Debug.LoopItem(4, $"{nameof(holyPlace)}{nameof(holyPlace.Faction)}", $"{holyPlace?.Faction ?? NULL}",
                    Good: holyPlace?.Faction == "Mechanimists", Indent: indent + 2, Toggle: getDoDebug());
            }

            Debug.Footer(4, $"{nameof(UD_SubGrandCathedralBuilder)}", $"{nameof(BuildZone)}({nameof(Z)}: {Z.ZoneID})", Toggle: doDebug);
            return true;
        }

        public static void PaintCell(Cell C, string Floor = null, string TileColor = null, string DetailtColor = null, string Tile = null, bool Overwrite = true, bool OverrideFloorColors = false)
        {
            Dictionary<string, int> weightedTiles = new()
            {
                { "Tiles/Tile-Dirt1.png", 10 },
                { "Terrain/sw_ground_dots1.png", 1 },
                { "Terrain/sw_ground_dots2.png", 1 },
                { "Terrain/sw_ground_dots3.png", 1 },
                { "Terrain/sw_ground_dots4.png", 1 },
                { "Tiles/sw_floor_dirty1.bmp", 1 },
                { "Tiles/sw_floor_dirty2.bmp", 1 },
                { "Tiles/sw_floor_dirty3.bmp", 1 },
                { "Tiles/sw_floor_dirty4.bmp", 1 },
            };
            string paintColorString = TileColor ?? "y";
            string paintTile = Tile ?? weightedTiles.Sample();
            string paintDetailColor = DetailtColor ?? "k";
            string paintTileColor = paintColorString;
            string paintRenderString = "ú";
            GameObject floorSample = GameObjectFactory.Factory.CreateSampleObject(Floor);
            if (floorSample != null && floorSample.TryGetPart(out Render floorRender))
            {
                if (!OverrideFloorColors)
                {
                    paintColorString = floorRender.ColorString;
                }
                paintTile = floorRender.Tile;
                if (!OverrideFloorColors)
                {
                    paintDetailColor = floorRender.DetailColor;
                    paintTileColor = floorRender.TileColor;
                }
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
