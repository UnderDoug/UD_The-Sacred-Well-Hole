using Genkit;
using System;
using System.Collections.Generic;

using XRL.Rules;
using XRL.World.Parts;
using XRL.World.WorldBuilders;

using UD_Modding_Toolbox;

using Verbosity = UD_Modding_Toolbox.UD_Logger.Verbosity;

using UD_SacredWellHole;

using static UD_SacredWellHole.Const;
using static UD_SacredWellHole.Options;

using Debug = UD_SacredWellHole.Debug;

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
        public string FloorMaterial = "SolidAir";

        public static List<string> ZonesPrebuilding = new();

        public static int LowestWellStratum => UD_SubStiltWorldBuilderExtension.LowestWellStratum;

        public Zone ZoneBelow = null;

        public static Raffle<string> WeightedFloorTiles => new()
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
            ZoneBelow = null;
            Regions = new();
        }

        bool CellShouldBeSolidified(Cell Cell)
        {
            return !Cell.HasObjectInheritsFrom("BaseScrapWall")
                && !Cell.HasObjectWithBlueprint(EmptyMaterial)
                && !Cell.HasObjectWithBlueprint("Air")
                && !Cell.HasObjectWithBlueprint("SolidAir");
        }

        static bool WantToPlaceWallInSubregionCell(
            Cell Cell,
            List<Cell> SubregionCells,
            int ChanceIfOutter,
            int ChanceOtherwise)
        {
            if (Cell.AnyAdjacentCell(Utils.HasWidget))
            {
                return false;
            }
            if (Cell.IsOuterCell(c => SubregionCells.Contains(c)) && ChanceIfOutter.in10())
            {
                return true;
            }
            if (Cell.AnyAdjacentCell(c => c.HasObjectInheritsFrom("Wall")) && ChanceIfOutter.in10())
            {
                return true;
            }
            return ChanceOtherwise.in10();
        }

        static void PlaceRandomScrapWallMoundInCell(Cell Cell, int ChanceScrapMound, string RandomScrapMoundBlueprint, string RandomScrapWallBlueprint)
        {
            if (ChanceScrapMound.in10())
            {
                Cell.AddObject(RandomScrapMoundBlueprint);
            }
            else
            {
                Cell.AddObject(RandomScrapWallBlueprint);
            }
        }

        static bool DeleteOrDestroyScrapWallInCell(Cell scrapWallCell)
        {
            if (scrapWallCell?.GetFirstObject(GO => GO.GetBlueprint().InheritsFrom("BaseScrapWall")) is not GameObject scrapWall)
            {
                return false;
            }

            bool isInnerCell = scrapWallCell.IsInnerCell(Basis: c => c.HasObjectWithBlueprintEndsWith("ScrapWall"), false);

            if ((isInnerCell || 3.in10()) && 1.ChanceIn(3))
            {
                if (1.ChanceIn(2))
                {
                    scrapWallCell.RemoveObject(scrapWall);
                }
                else
                {
                    scrapWall.Die();
                }
                return true;
            }
            return false;
        }

        public bool BuildZone(Zone Z)
        {
            Debug.Logger.Header(Verbosity.Max, $"{nameof(UD_SubGrandCathedralBuilder)}", $"{nameof(BuildZone)}({nameof(Z)}: {Z.ZoneID})", Toggle: doDebug);
            Debug.Logger.ResetIndent(out int indent);

            zone = Z;
            int currentStratum = zone.Z;

            ZonesPrebuilding.Add(zone.ZoneID);

            EmptyMaterial = currentStratum < 15 ? EmptyMaterial : "QuantumAir";

            FloorMaterial = currentStratum < 21 ? EmptyMaterial : "CompactedScrapFloor";

            ZoneManager zoneManager = The.ZoneManager;

            string randomScrapMoundBlueprint = "RandomScrapMound";
            string randomScrapWallBlueprint = "RandomScrapWallSometimesGigantic";

            Cell stiltWellCell = zone.GetCell(StiltWellLocation);
            int strataFromBottom = LowestWellStratum - currentStratum;
            int strataFromTop = zone.Z - 10;
            bool isFinalStratum = strataFromBottom == 0;
            string floorMaterial = !isFinalStratum ? "BlackMarbleWalkway" : null;

            Dictionary<int, string> cathedralSubStrata = UD_SubStiltWorldBuilderExtension.CathedralSubStrata;
            string zoneBelowID = null;
            if (cathedralSubStrata.ContainsKey(strataFromTop + 1))
            {
                zoneBelowID = cathedralSubStrata[strataFromTop + 1];
            }
            ZoneBelow = !isFinalStratum ? zoneManager.GetZone(zoneBelowID) : null;

            Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(strataFromBottom)}: {strataFromBottom}, {nameof(strataFromTop)}: {strataFromTop}",
                Indent: indent + 1, Toggle: getDoDebug());

            Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(floorMaterial)}: {floorMaterial}, {nameof(strataFromTop)}: {strataFromTop}",
                Indent: indent + 1, Toggle: getDoDebug());

            // PaintZoneFloors(zone, ZoneBelow, isFinalStratum, floorMaterial);

            UDSW_MostlySolidMaterial mostlySolidBuilder = new(Material: null, Materials: new() { "SolidAir", "Sandstone" }, ClearFirst: true);
            if (strataFromTop < 6)
            {
                Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(mostlySolidBuilder)}", $"All Cells, Count: {mostlySolidBuilder.Cells.Count}", Indent: indent + 1, Toggle: getDoDebug());
                mostlySolidBuilder.Cells.AddRange(zone.GetCells());
            }
            else
            {
                List<Cell> cellsToSolidify = Event.NewCellList(zone.GetCells(CellShouldBeSolidified));
                mostlySolidBuilder.Cells.AddRange(cellsToSolidify);

                Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(mostlySolidBuilder)}", $"Filtered Cells, Count: {mostlySolidBuilder.Cells.Count}",
                    Indent: indent + 1, Toggle: getDoDebug());
            }
            if (isFinalStratum)
            {
                mostlySolidBuilder.Materials.Remove("SolidAir");
                Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(mostlySolidBuilder)}", $"Removed Solid Air from {nameof(mostlySolidBuilder.Materials)}",
                    Indent: indent + 1, Toggle: getDoDebug());
            }
            mostlySolidBuilder.Cells.RemoveAll(c => c == stiltWellCell);
            mostlySolidBuilder.BuildZone(zone);

            if (strataFromTop < 6)
            {
                Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(Cell)}.{nameof(Cell.Clear)}()",
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

                Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(Cell)}.{nameof(Cell.AddObject)}({EmptyMaterial})",
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
                airRadius = getAirRadiusFromStratum(strataFromTop, strataFromBottom + 4);
            }

            Debug.Logger.LoopItem(Verbosity.Max, nameof(airRadius), airRadius.ToString(),
                Indent: indent + 1, Toggle: getDoDebug());

            Dictionary<string, List<Cell>> openAirRegion = stiltWellCell.GetCircleRegion(airRadius);
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

                int chanceIfOutter = Math.Max(2, Math.Min(12 - strataFromBottom, 9));
                int chanceOtherwise = Math.Max(0, Math.Min(10 - strataFromBottom, 7));
                int chanceScrapMound = Math.Max(2, Math.Min(strataFromBottom / 5, 3));
                if (strataFromBottom < 6)
                {
                    bool doDebugScrapWallMoundChances = getDoDebug("ScrapWallMound Chances");

                    Debug.Logger.LoopItem(Verbosity.Max, nameof(openAirRegion) + " ScrapWallMound Chances (" + nameof(strataFromBottom) + ": " + strataFromBottom + ")...",
                        Indent: indent + 1, Toggle: doDebugScrapWallMoundChances);
                    Debug.Logger.LoopItem(Verbosity.Max, nameof(chanceIfOutter), chanceIfOutter.ToString(),
                        Indent: indent + 2, Toggle: doDebugScrapWallMoundChances);
                    Debug.Logger.LoopItem(Verbosity.Max, nameof(chanceOtherwise), chanceOtherwise.ToString(),
                        Indent: indent + 2, Toggle: doDebugScrapWallMoundChances);
                    Debug.Logger.LoopItem(Verbosity.Max, nameof(chanceScrapMound), chanceScrapMound.ToString(),
                        Indent: indent + 2, Toggle: doDebugScrapWallMoundChances);
                }

                List<Cell> cellsAlreadyRolled = Event.NewCellList();
                foreach ((string subregionLabel, List<Cell> subregionCells) in openAirRegion)
                {
                    foreach (Cell cell in subregionCells)
                    {
                        cell.Clear();
                        if (!isFinalStratum)
                        {
                            if (strataFromBottom < 7)
                            {
                                cell.AddObject(FloorMaterial);
                            }
                            else
                            {
                                cell.AddObject(EmptyMaterial);
                            }
                        }
                        if (strataFromBottom < 7)
                        {
                            if (WantToPlaceWallInSubregionCell(cell, subregionCells, chanceIfOutter, chanceOtherwise))
                            {
                                PlaceRandomScrapWallMoundInCell(cell, chanceScrapMound, randomScrapMoundBlueprint, randomScrapWallBlueprint);
                            }
                            else
                            if (strataFromBottom > 5 && subregionLabel == Outer && chanceIfOutter.in10())
                            {
                                PlaceRandomScrapWallMoundInCell(cell, chanceScrapMound, randomScrapMoundBlueprint, randomScrapWallBlueprint);
                            }
                        }
                        if (subregionLabel == Outer)
                        {
                            foreach (Cell adjacentCell in cell.GetAdjacentCells())
                            {
                                if (!openAirCells.Contains(adjacentCell))
                                {
                                    int errosionRollSuccess = 3;
                                    if (strataFromBottom < 3)
                                    {
                                        errosionRollSuccess = 5;
                                    }
                                    if (!cellsAlreadyRolled.Contains(adjacentCell)
                                        && Stat.RollCached("1d8") < errosionRollSuccess)
                                    {
                                        adjacentCell.Clear();
                                        if (strataFromTop < 10)
                                        {
                                            adjacentCell.AddObject(EmptyMaterial);
                                        }
                                        else
                                        {
                                            if (!isFinalStratum)
                                            {
                                                adjacentCell.AddObject(EmptyMaterial);
                                            }
                                            PlaceRandomScrapWallMoundInCell(cell, chanceScrapMound, randomScrapMoundBlueprint, randomScrapWallBlueprint);
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
                if (!cellsToAddToOuter.IsNullOrEmpty())
                {
                    openAirRegion[Outer].AddRange(cellsToAddToOuter);
                }
                if (strataFromBottom < 9)
                {
                    bool doDebugBreakScrap = getDoDebug();

                    Debug.Logger.LoopItem(Verbosity.Max, $"Breaking/Deleting select Scrap Walls and Mounds",
                        Indent: indent + 1, Toggle: doDebugBreakScrap);

                    foreach (Cell scrapWallCell in zone.GetCellsWithObject(GO => GO.GetBlueprint().InheritsFrom("BaseScrapWall")))
                    {
                        Debug.Logger.Divider(Verbosity.Max, HONLY, 40, Indent: indent + 2, Toggle: doDebugBreakScrap);
                        Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(scrapWallCell)}: ({currentStratum})[{scrapWallCell.Location}]",
                            Indent: indent + 2, Toggle: doDebugBreakScrap);

                        if (DeleteOrDestroyScrapWallInCell(scrapWallCell))
                        {
                            Debug.Logger.CheckYeh(Verbosity.Max, $"{nameof(scrapWallCell)} did do {nameof(DeleteOrDestroyScrapWallInCell)}",
                                Indent: indent + 3, Toggle: doDebugBreakScrap);
                        }
                        else
                        {
                            Debug.Logger.CheckNah(Verbosity.Max, $"{nameof(scrapWallCell)} didn't do {nameof(DeleteOrDestroyScrapWallInCell)}",
                                Indent: indent + 3, Toggle: doDebugBreakScrap);
                        }
                    }
                    Debug.Logger.Divider(Verbosity.Max, HONLY, 40, Indent: indent + 2, Toggle: doDebugBreakScrap);
                }

                bool doHighlighting = true;
                if (doHighlighting)
                {
                    foreach ((string label, List<Cell> region) in openAirRegion)
                    {
                        if (label == Inner)
                        {
                            foreach (Cell regionCell in region)
                            {
                                regionCell.HighlightGreen(Solid: false);
                            }
                        }
                        if (label == Outer)
                        {
                            foreach (Cell regionCell in region)
                            {
                                regionCell.HighlightPurple(Solid: false);
                            }
                        }
                    }
                }
            }

            if (isFinalStratum)
            {
                bool doDebugFinalStratum = getDoDebug();
                Debug.Logger.LoopItem(Verbosity.Max, $"Rolling Tier 3-6 Relic by way of ExplodingDie and setting {nameof(HolyPlace)}",
                    Indent: indent + 1, Toggle: doDebugFinalStratum);

                int tier = new DieRoll("1d3").Explode(3, 1, 6);
                if (RelicGenerator.GenerateRelic(Tier: tier, RandomName: true) is GameObject stiltWellRelic)
                {
                    stiltWellCell.Clear(Combat: true).AddObject(stiltWellRelic);

                    Debug.Logger.CheckYeh(Verbosity.Max, $"{nameof(stiltWellRelic)} (T:{tier})", $"{stiltWellRelic.DebugName}",
                        Indent: indent + 2, Toggle: doDebugFinalStratum);
                }
                else
                {
                    stiltWellCell.Clear(Combat: true).AddPopulation("Artifact 8R");

                    Debug.Logger.CheckNah(Verbosity.Max, $"Relic Failed, Adding from population", $"Artifact 8R",
                        Indent: indent + 2, Toggle: doDebugFinalStratum);
                }
                if (GameObjectFactory.Factory.CreateObject("Six Day Stilt Recoiler") is GameObject stiltRecoiler)
                {
                    stiltWellCell.AddObject(stiltRecoiler);

                    Debug.Logger.CheckYeh(Verbosity.Max, $"{nameof(stiltRecoiler)}", $"{stiltRecoiler.DebugName}",
                        Indent: indent + 2, Toggle: doDebugFinalStratum);
                }
                for (int i = 0; i < Stat.RandomCosmetic(1, 3); i++)
                {
                    int wedgeValue = Math.Max(1, Stat.RandomCosmetic(2, 3) - i);
                    string creditWedgeBlueprint = $"CyberneticsCreditWedge" + (wedgeValue == 1 ? "" : wedgeValue);
                    if (GameObjectFactory.Factory.CreateObject(creditWedgeBlueprint) is GameObject creditWedge)
                    {
                        stiltWellCell.AddObject(creditWedge);

                        Debug.Logger.CheckYeh(Verbosity.Max, $"{nameof(creditWedge)}", $"{creditWedge.DebugName}",
                            Indent: indent + 2, Toggle: doDebugFinalStratum);
                    }
                }
                if (stiltWellCell.AddObject("HolyPlaceWidget").TryGetPart(out HolyPlace holyPlace))
                {
                    holyPlace.Faction = "Mechanimists";
                }
                Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(holyPlace)}{nameof(holyPlace.Faction)}", $"{holyPlace?.Faction ?? NULL}",
                    Good: holyPlace?.Faction == "Mechanimists", Indent: indent + 2, Toggle: doDebugFinalStratum);
            }

            PaintZoneFloors(zone, ZoneBelow, isFinalStratum, floorMaterial);

            Debug.Logger.SetIndent(indent);
            Debug.Logger.Footer(Verbosity.Max, $"{nameof(UD_SubGrandCathedralBuilder)}", $"{nameof(BuildZone)}({nameof(Z)}: {Z.ZoneID})", Toggle: doDebug);
            return true;
        }

        public static void PaintCell(Cell C, string Floor = null, string TileColor = null, string DetailtColor = null, string Tile = null, bool Overwrite = true, bool OverrideFloorColors = false)
        {
            string paintColorString = TileColor ?? "y";
            string paintTile = Tile ?? WeightedFloorTiles.SampleCosmetic();
            string paintDetailColor = DetailtColor ?? "k";
            string paintTileColor = paintColorString;
            string paintRenderString = "ú";
            GameObject floorSample = GameObject.CreateSample(Floor);
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

        public static void PaintZoneFloors(Zone Zone, Zone ZoneBelow, bool IsFinalStratum, string FloorMaterial)
        {
            foreach (Cell cell in Zone.GetCells())
            {
                if (cell.ParentZone == Zone)
                {
                    bool cellHasSandstoneWall = cell.HasObjectWithBlueprint("Sandstone");
                    Cell cellBelow = null;
                    if (!IsFinalStratum && ZoneBelow != null)
                    {
                        cellBelow = ZoneBelow.GetCell(cell.Location);
                    }
                    GameObject wallBelow = cellBelow?.GetFirstObject(GO => GO.InheritsFrom("BaseScrapWall"));
                    bool cellHasScrapWallBelow = wallBelow != null;

                    string TileColor = cellHasSandstoneWall ? null : Stat.RollCached("1d6") switch
                    {
                        5 => "w",
                        4 => "r",
                        3 => "c",
                        2 => "g",
                        1 => "K",
                        _ => "y",
                    };
                    string DetailColor = cellHasSandstoneWall ? null : Stat.RollCached("1d6") switch
                    {
                        5 => "w",
                        4 => "r",
                        3 => "c",
                        2 => "g",
                        1 => "K",
                        _ => "y",
                    };

                    if (wallBelow?.Render is Render renderBelow)
                    {
                        TileColor = renderBelow.TileColor;
                        DetailColor = renderBelow.DetailColor;
                    }

                    foreach (GameObject floor in cell.GetObjectsViaEventList(GO => GO.InheritsFrom("Floor")))
                    {
                        if (!floor.HasPart<SolidAir>() && !floor.HasPart<QuantumAir>())
                        {
                            cell.RemoveObject(floor);
                        }
                        else
                        if (!cellHasSandstoneWall)
                        {
                            continue;
                        }
                        else
                        if (!cellHasScrapWallBelow && floor.Render is Render render)
                        {
                            render.Tile = WeightedFloorTiles.SampleCosmetic();
                            render.TileColor = TileColor;
                            render.DetailColor = DetailColor;
                            if (!render.DisplayName.Contains("scrap"))
                            {
                                render.DisplayName = "scrappy " + render.DisplayName;
                            }
                        }
                    }
                    bool doFloorMaterial = cellHasScrapWallBelow;
                    PaintCell(cell, (doFloorMaterial ? FloorMaterial : null), TileColor, DetailColor, Overwrite: true, OverrideFloorColors: true);
                }
            }
        }
    }
}
