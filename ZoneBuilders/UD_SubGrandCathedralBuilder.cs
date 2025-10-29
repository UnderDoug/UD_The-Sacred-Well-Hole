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

        public static List<string> ZonesPrebuilding = new();

        public static int LowestWellStratum => UD_SubStiltWorldBuilderExtension.LowestWellStratum;

        public static Dictionary<string, int> WeightedFloorTiles => new()
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

        public UD_SubGrandCathedralBuilder()
        {
        }

        public bool BuildZone(Zone Z)
        {
            Debug.Header(4, $"{nameof(UD_SubGrandCathedralBuilder)}", $"{nameof(BuildZone)}({nameof(Z)}: {Z.ZoneID})", Toggle: doDebug);
            int indent = Debug.LastIndent;

            zone = Z;
            int currentStratum = zone.Z;

            ZonesPrebuilding.Add(zone.ZoneID);

            EmptyMaterial = currentStratum < 15 ? EmptyMaterial : "QuantumAir";

            ZoneManager zoneManager = The.ZoneManager;

            // zone.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("SixDayStiltTile"));

            Cell stiltWellCell = zone.GetCell(StiltWellLocation);
            int strataFromBottom = LowestWellStratum - currentStratum;
            int strataFromTop = zone.Z - 10;
            bool isFinalStratum = strataFromBottom == 0;
            string floorMaterial = !isFinalStratum ? "BlackMarbleWalkway" : null;

            Debug.LoopItem(4, $"{nameof(strataFromBottom)}: {strataFromBottom}, {nameof(strataFromTop)}: {strataFromTop}",
                Indent: indent + 1, Toggle: getDoDebug());

            Debug.LoopItem(4, $"{nameof(floorMaterial)}: {floorMaterial}, {nameof(strataFromTop)}: {strataFromTop}",
                Indent: indent + 1, Toggle: getDoDebug());

            foreach (Cell cell in zone.GetCells())
            {
                if (cell.ParentZone == zone)
                    // && !cell.HasObject(GO => GO.InheritsFrom("Stairs") || GO.HasPart<QuantumAir>() || GO.HasPart<SolidAir>()))
                {
                    bool hasSandstoneWall = cell.HasObjectWithBlueprint("Sandstone");
                    string TileColor = hasSandstoneWall ? null : Stat.RollCached("1d6") switch
                    {
                        5 => "w",
                        4 => "r",
                        3 => "c",
                        2 => "g",
                        1 => "K",
                        _ => "y",
                    };
                    string DetailColor = hasSandstoneWall ? null : Stat.RollCached("1d6") switch
                    {
                        5 => "w",
                        4 => "r",
                        3 => "c",
                        2 => "g",
                        1 => "K",
                        _ => "y",
                    };
                    List<GameObject> realFloors = Event.NewGameObjectList(cell.GetObjectsThatInheritFrom("Floor"));
                    realFloors.RemoveAll(GO => GO.HasPart<SolidAir>() || GO.HasPart<QuantumAir>());
                    if (!realFloors.IsNullOrEmpty())
                    {
                        foreach (GameObject realFloor in realFloors)
                        {
                            cell.RemoveObject(realFloor);
                        }
                    }
                    List<GameObject> falseFloors = Event.NewGameObjectList(cell.GetObjectsThatInheritFrom("Floor"));
                    foreach (GameObject falseFloor in falseFloors)
                    {
                        falseFloor.Render.Tile = WeightedFloorTiles.Sample();
                        falseFloor.Render.TileColor = TileColor;
                        falseFloor.Render.DetailColor = DetailColor;
                        falseFloor.Render.DisplayName = "scrappy " + falseFloor.Render.DisplayName;
                    }
                    Cell cellBelow = null;
                    if (!isFinalStratum)
                    {
                        cell.GetCellFromDirection("D", BuiltOnly: false);
                    }
                    if (cellBelow?.GetFirstObject(GO => GO.InheritsFrom("BaseScrapWall")) is GameObject wallBelow
                        && wallBelow.Render != null)
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

                Debug.LoopItem(4, $"{nameof(mostlySolidBuilder)}", $"Filtered Cells, Count: {mostlySolidBuilder.Cells.Count}",
                    Indent: indent + 1, Toggle: getDoDebug());
            }
            if (isFinalStratum)
            {
                mostlySolidBuilder.Materials.Remove("SolidAir");
                Debug.LoopItem(4, $"{nameof(mostlySolidBuilder)}", $"Removed Solid Air from {nameof(mostlySolidBuilder.Materials)}",
                    Indent: indent + 1, Toggle: getDoDebug());
            }
            mostlySolidBuilder.Cells.RemoveAll(c => c == stiltWellCell);
            mostlySolidBuilder.BuildZone(zone);

            if (strataFromTop < 6)
            {
                Debug.LoopItem(4, $"{nameof(Cell)}.{nameof(Cell.Clear)}()",
                    Indent: indent + 1, Toggle: getDoDebug());

                stiltWellCell.Clear(Combat: true);
                if (strataFromTop == 5)
                {
                    stiltWellCell.AddObject("SelfSacrificeHelper");
                    stiltWellCell.AddObject("SpawnBlocker");
                }
            }
            if (strataFromTop < 5)
            {
                stiltWellCell.AddObject(EmptyMaterial);

                Debug.LoopItem(4, $"{nameof(Cell)}.{nameof(Cell.AddObject)}({EmptyMaterial})",
                    Indent: indent + 1, Toggle: getDoDebug());
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

            static int getAirRadiusFromStratum(int StrataFromTop, int StrataFromTopOrBottom)
            {
                int stratumOffset = Math.Max(1, StrataFromTop - 1);
                float stratumMulti = StrataFromTopOrBottom * 0.2125f;
                return Math.Max(1, (int)(stratumOffset * stratumMulti));
            }
            int airRadius = 1;
            if (strataFromTop > 2 && strataFromTop < 11)
            {
                airRadius = getAirRadiusFromStratum(strataFromTop, strataFromTop);
            }
            else
            if (strataFromTop > 10)
            {
                airRadius = getAirRadiusFromStratum(strataFromTop, strataFromBottom + 3);
            }

            Dictionary<string, List<Cell>> openAirRegion = stiltWellCell.GetCircleRegion(airRadius, Filter: c => c != stiltWellCell);
            if (!openAirRegion.IsNullOrEmpty())
            {
                if (openAirRegion.ContainsKey(Inner))
                {
                    openAirRegion[Inner].RemoveAll(c => c == stiltWellCell);
                    if (strataFromBottom < 10)
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
                                if (cell.IsOuterCell(c => subregionCells.Contains(c))
                                    && !cell.AnyAdjacentCell(c => c.HasObject(GO => GO.GetBlueprint().InheritsFrom("Widget")))
                                    && 3.in10())
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
                                        if (false 
                                            && cellAbove != null 
                                            && strataFromTop > 1 
                                            && strataFromBottom > 1 
                                            && !cellAbove.HasObjectWithBlueprint("Sandstone"))
                                        {
                                            adjacentCell.Clear().AddObject(EmptyMaterial);
                                            cellsToAddToOuter.Add(adjacentCell);
                                            cellsAlreadyRolled.Add(adjacentCell);
                                            continue;
                                        }
                                        int errosionRollSuccess = 2;
                                        if (strataFromBottom < 5)
                                        {
                                            errosionRollSuccess = 3;
                                        }
                                        if (!cellsAlreadyRolled.Contains(adjacentCell)
                                            && Stat.RollCached("1d3") < errosionRollSuccess)
                                        {
                                            adjacentCell.Clear();
                                            if (strataFromBottom > 5)
                                            {
                                                adjacentCell.AddObject(EmptyMaterial);
                                            }
                                            else
                                            {
                                                int scrapMoundChance = Math.Max(1, Math.Min(strataFromBottom + 1, 4));
                                                if (scrapMoundChance.in10())
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
                if (strataFromBottom < 9)
                {
                    bool doDebugBreakScrap = getDoDebug();

                    Debug.LoopItem(4, $"Breaking/Deleting select Scrap Walls and Mounds",
                        Indent: indent + 1, Toggle: doDebugBreakScrap);

                    foreach (Cell scrapWallCell in zone.GetCellsWithObject(GO => GO.GetBlueprint().InheritsFrom("BaseScrapWall")))
                    {
                        Debug.Divider(4, HONLY, 40, Indent: indent + 2, Toggle: doDebugBreakScrap);
                        Debug.LoopItem(4, $"{nameof(scrapWallCell)}: ({currentStratum})[{scrapWallCell.Location}]",
                            Indent: indent + 2, Toggle: doDebugBreakScrap);

                        GameObject scrapWall = scrapWallCell.GetFirstObject(GO => GO.GetBlueprint().InheritsFrom("BaseScrapWall"));
                        if (scrapWall != null && scrapWallCell.IsInnerCell(Basis: c => c.HasObjectWithBlueprintEndsWith("ScrapWall"), false)
                            || Stat.RollCached("1d10") == 3)
                        {
                            Debug.CheckYeh(4, $"{nameof(scrapWallCell)} is Inner (or by 3in10)",
                                Indent: indent + 3, Toggle: doDebugBreakScrap);

                            if (Stat.RollCached("1d3") == 1)
                            {
                                Debug.CheckYeh(4, $"{nameof(scrapWall)} located and 1in3 successful",
                                    Indent: indent + 3, Toggle: doDebugBreakScrap);

                                if (Stat.RollCached("1d2") == 1)
                                {
                                    Debug.LoopItem(4, $"{scrapWall?.DebugName ?? NULL}", $"Removed",
                                        Indent: indent + 4, Toggle: doDebugBreakScrap);

                                    scrapWallCell.RemoveObject(scrapWall);
                                }
                                else
                                {
                                    Debug.LoopItem(4, $"{scrapWall?.DebugName ?? NULL}", $"Kilt",
                                        Indent: indent + 4, Toggle: doDebugBreakScrap);

                                    scrapWall.Die();
                                }
                            }
                            else
                            {
                                Debug.CheckNah(4, $"{nameof(scrapWall)} not located or 1in3 failed",
                                    Indent: indent + 3, Toggle: doDebugBreakScrap);
                            }
                        }
                    }
                    Debug.Divider(4, HONLY, 40, Indent: indent + 2, Toggle: doDebugBreakScrap);
                }
            }

            if (isFinalStratum)
            {
                bool doDebugFinalStratum = getDoDebug();
                Debug.LoopItem(4, $"Rolling Tier 3-6 Relic by way of ExplodingDie and setting {nameof(HolyPlace)}",
                    Indent: indent + 1, Toggle: doDebugFinalStratum);

                int tier = new DieRoll("1d3").Explode(3, 1, 6);
                GameObject stiltWellRelic = RelicGenerator.GenerateRelic(Tier: tier, RandomName: true);
                if (stiltWellRelic != null)
                {
                    stiltWellCell.Clear(Combat: true).AddObject(stiltWellRelic);

                    Debug.CheckYeh(4, $"{nameof(stiltWellRelic)} (T:{tier})", $"{stiltWellRelic.DebugName}",
                        Indent: indent + 2, Toggle: doDebugFinalStratum);
                }
                else
                {
                    stiltWellCell.AddPopulation("Artifact 8R");

                    Debug.CheckNah(4, $"Relic Failed, Adding from population", $"Artifact 8R",
                        Indent: indent + 2, Toggle: doDebugFinalStratum);
                }
                GameObject stiltRecoiler = GameObjectFactory.Factory.CreateObject("Six Day Stilt Recoiler");
                if (stiltRecoiler != null)
                {
                    stiltWellCell.AddObject(stiltRecoiler);

                    Debug.CheckYeh(4, $"{nameof(stiltRecoiler)}", $"{stiltRecoiler.DebugName}",
                        Indent: indent + 2, Toggle: doDebugFinalStratum);
                }
                for (int i = 0; i < Stat.RandomCosmetic(1, 3); i++)
                {
                    int wedgeValue = Math.Max(1, Stat.RandomCosmetic(2, 3) - i);
                    string creditWedgeBlueprint = $"CyberneticsCreditWedge" + (wedgeValue == 1 ? "" : wedgeValue);
                    GameObject creditWedge = GameObjectFactory.Factory.CreateObject(creditWedgeBlueprint);
                    if (creditWedge != null)
                    {
                        stiltWellCell.AddObject(creditWedge);

                        Debug.CheckYeh(4, $"{nameof(creditWedge)}", $"{creditWedge.DebugName}",
                            Indent: indent + 2, Toggle: doDebugFinalStratum);
                    }
                }
                if (stiltWellCell.AddObject("HolyPlaceWidget").TryGetPart(out HolyPlace holyPlace))
                {
                    holyPlace.Faction = "Mechanimists";
                }
                Debug.LoopItem(4, $"{nameof(holyPlace)}{nameof(holyPlace.Faction)}", $"{holyPlace?.Faction ?? NULL}",
                    Good: holyPlace?.Faction == "Mechanimists", Indent: indent + 2, Toggle: doDebugFinalStratum);
            }

            Debug.Footer(4, $"{nameof(UD_SubGrandCathedralBuilder)}", $"{nameof(BuildZone)}({nameof(Z)}: {Z.ZoneID})", Toggle: doDebug);
            return true;
        }

        public static void PaintCell(Cell C, string Floor = null, string TileColor = null, string DetailtColor = null, string Tile = null, bool Overwrite = true, bool OverrideFloorColors = false)
        {
            string paintColorString = TileColor ?? "y";
            string paintTile = Tile ?? WeightedFloorTiles.Sample();
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
