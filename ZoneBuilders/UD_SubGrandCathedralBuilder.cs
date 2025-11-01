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
                nameof(PlaceRandomScrapWallMoundInCell),
            };
            List<object> dontList = new()
            {
                'X',    // Trace
                nameof(PaintCell),
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

        public string EmptyMaterial;
        public string FloorMaterial;

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

        public string RandomScrapMoundBlueprint;
        public string RandomScrapWallBlueprint;

        public Cell StiltWellCell;

        public int CurrentStratum;
        public int StrataFromBottom;
        public int StrataFromTop;
        public bool IsFinalStratum;

        public UD_SubGrandCathedralBuilder()
        {
            EmptyMaterial = "Air";
            FloorMaterial = "SolidAir";

            ZoneBelow = null;
            Regions = new();

            StiltWellCell = null;

            CurrentStratum = -1;
            StrataFromBottom = -1;
            StrataFromTop = -1;
            IsFinalStratum = false;

            RandomScrapMoundBlueprint = "RandomScrapMound";
            RandomScrapWallBlueprint = "RandomScrapWallOrUnlikelyScrapCorpseSometimesGigantic";
        }



        public static void PaintCell(
            Cell C,
            string Floor = null,
            string TileColor = null,
            string DetailColor = null,
            string Tile = null,
            bool Overwrite = true,
            bool OverrideFloorColors = false)
        {
            if (C == null)
            {
                return;
            }

            bool doDebug = getDoDebug(nameof(PaintCell));
            Debug.Logger.GetIndent(out int indent);
            Debug.Logger.Entry(Verbosity.Max,
                "(" + C.ParentZone.Z + ")[" + C.DebugName + "] " + nameof(PaintCell),
                Indent: indent + 1, Toggle: doDebug);

            string paintColorString = TileColor ?? "y";
            string paintTile = Tile ?? WeightedFloorTiles.SampleCosmetic();
            string paintDetailColor = DetailColor ?? "k";
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
            Debug.Logger.SetIndent(indent);
        }

        public static void PaintZoneFloors(Zone Zone, Zone ZoneBelow, bool IsFinalStratum, string FloorMaterial)
        {
            bool doDebug = getDoDebug(nameof(PaintZoneFloors));
            Debug.Logger.GetIndent(out int indent);
            Debug.Logger.Entry(Verbosity.Max,
                nameof(UD_SubGrandCathedralBuilder) + "." + nameof(PaintZoneFloors) + "(" +
                nameof(Zone.Z) + ": " + Zone.Z + ", " +
                nameof(IsFinalStratum) + ": " + IsFinalStratum + ", " +
                nameof(FloorMaterial) + ": " + FloorMaterial + ")",
                Indent: indent + 1, Toggle: doDebug);

            Debug.Logger.Divider(Verbosity.Max, HONLY, 40, Indent: indent + 1, Toggle: getDoDebug(nameof(PaintCell)));
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
                    PaintCell(
                        C: cell,
                        Floor: doFloorMaterial ? FloorMaterial : null,
                        TileColor: TileColor,
                        DetailColor: DetailColor,
                        Overwrite: true,
                        OverrideFloorColors: true);
                }
            }
            Debug.Logger.Divider(Verbosity.Max, HONLY, 40, Indent: indent + 1, Toggle: doDebug);
            Debug.Logger.SetIndent(indent);
        }

        bool CellShouldBeSolidified(Cell Cell)
        {
            return !Cell.HasObjectInheritsFrom("BaseScrapWall")
                && !Cell.HasObjectWithBlueprint(EmptyMaterial)
                && !Cell.HasObjectWithBlueprint("Air")
                && !Cell.HasObjectWithBlueprint("SolidAir");
        }
        static bool HasWallThatNotScrapWall(Cell C) => C.HasObjectInheritsFrom("Wall") && !C.HasObjectInheritsFrom("BaseScrapWall");
        static bool WantToPlaceWallInSubregionCell(
            Cell Cell,
            List<Cell> SubregionCells,
            int ChanceIfOutter,
            int ChanceOtherwise)
        {
            Debug.Logger.GetIndent(out int indent);
            bool doDebug = getDoDebug(nameof(WantToPlaceWallInSubregionCell));
            Debug.Logger.LoopItem(Verbosity.Max,
                "(" + Cell.ParentZone.Z + ")[" + Cell.DebugName + "] " + nameof(WantToPlaceWallInSubregionCell) + ", " +
                nameof(ChanceIfOutter) + ": " + ChanceIfOutter + ", " + 
                nameof(ChanceOtherwise) + ": " + ChanceOtherwise,
                Indent: indent + 1, Toggle: doDebug);

            if (Cell.AnyAdjacentCell(Utils.HasWidget))
            {
                Debug.Logger.CheckNah(Verbosity.Max, nameof(Cell.AnyAdjacentCell) + " " + nameof(Utils.HasWidget),
                    Indent: indent + 2, Toggle: doDebug);
                Debug.Logger.SetIndent(indent);
                return false;
            }
            if (Cell.HasObjectWithPart((Parts.StairsDown s) => s.PullDown))
            {
                Debug.Logger.CheckNah(Verbosity.Max, 
                    nameof(Cell.HasObjectWithPart) + " " + nameof(Parts.StairsDown) + "." +
                    nameof(Parts.StairsDown.PullDown) + " is " + true,
                    Indent: indent + 2, Toggle: doDebug);
                Debug.Logger.SetIndent(indent);
                return false;
            }
            if (Cell.HasObjectWithPart((LiquidVolume l) => l.Volume > 9))
            {
                Debug.Logger.CheckNah(Verbosity.Max, 
                    nameof(Cell.HasObjectWithPart) + " " + nameof(LiquidVolume) + "." +
                    nameof(LiquidVolume.Volume) + " is GTE" + 10,
                    Indent: indent + 2, Toggle: doDebug);
                Debug.Logger.SetIndent(indent);
                return false;
            }
            if (Cell.IsOuterCell(c => SubregionCells.Contains(c))
                && ChanceIfOutter.in100())
            {
                Debug.Logger.CheckYeh(Verbosity.Max,
                    nameof(UD_SacredWellHole.Extensions.IsOuterCell) + " Basis",
                    nameof(SubregionCells) + "." + nameof(List<Cell>.Contains),
                    Indent: indent + 2, Toggle: doDebug);
                Debug.Logger.SetIndent(indent);
                return true;
            }
            if (Cell.AnyAdjacentCell(HasWallThatNotScrapWall)
                && ChanceIfOutter.in100())
            {
                Debug.Logger.CheckYeh(Verbosity.Max, nameof(Cell.AnyAdjacentCell) + " " + nameof(HasWallThatNotScrapWall),
                    Indent: indent + 2, Toggle: doDebug);
                Debug.Logger.SetIndent(indent);
                return true;
            }
            bool otherwise = ChanceOtherwise.in100();
            Debug.Logger.LoopItem(Verbosity.Max, nameof(ChanceOtherwise) + " in100",
                Good: otherwise, Indent: indent + 2, Toggle: doDebug);
            Debug.Logger.SetIndent(indent);
            return otherwise;
        }

        public static GameObject PlaceRandomScrapWallMoundInCell(
            Cell Cell,
            int ChanceScrapMound,
            string RandomScrapMoundBlueprint,
            string RandomScrapWallBlueprint,
            string Label = null,
            int ExtraIndent = 0,
            bool? doDebugOverride = null)
        {
            bool doDebug = doDebugOverride.HasValue ? doDebugOverride.GetValueOrDefault() : getDoDebug(nameof(PlaceRandomScrapWallMoundInCell));
            GameObject placedObject = null;
            if (ChanceScrapMound.in100())
            {
                placedObject = Cell.AddObject(RandomScrapMoundBlueprint);
            }
            else
            {
                placedObject = Cell.AddObject(RandomScrapWallBlueprint);
            }
            if (!Label.IsNullOrEmpty())
            {
                Label += " ";
            }
            Debug.Logger.GetIndent(out int indent);
            Debug.Logger.LoopItem(Verbosity.Max,
                Label + "(" + Cell.ParentZone.Z + ")[" + Cell.DebugName + "] Placed", placedObject?.DebugName ?? NULL,
                Indent: indent + 1 + ExtraIndent, Toggle: doDebug);
            Debug.Logger.SetIndent(indent);
            return placedObject;
        }

        public static void ProcessOpenAirCell(
            Cell SubregionCell,
            List<Cell> SubregionCells,
            int ChanceIfOutter,
            int ChanceScrapMound,
            int ChanceOtherwise,
            int StrataFromBottom,
            int ForStrataLessThanFromBottom,
            bool IsFinalStratum,
            string FloorMaterial,
            string EmptyMaterial,
            string RandomScrapMoundBlueprint,
            string RandomScrapWallBlueprint,
            string Label = null,
            int ExtraIndent = 0)
        {
            if (SubregionCell == null
                || SubregionCells.IsNullOrEmpty()
                || FloorMaterial.IsNullOrEmpty()
                || EmptyMaterial.IsNullOrEmpty()
                || RandomScrapMoundBlueprint.IsNullOrEmpty()
                || RandomScrapWallBlueprint.IsNullOrEmpty())
            {
                return;
            }

            SubregionCell.Clear();
            if (!IsFinalStratum)
            {
                if (StrataFromBottom < ForStrataLessThanFromBottom)
                {
                    SubregionCell.AddObject(FloorMaterial);
                }
                else
                {
                    SubregionCell.AddObject(EmptyMaterial);
                }
            }
            if (StrataFromBottom < ForStrataLessThanFromBottom)
            {
                if (WantToPlaceWallInSubregionCell(SubregionCell, SubregionCells, ChanceIfOutter, ChanceOtherwise))
                {
                    PlaceRandomScrapWallMoundInCell(
                        Cell: SubregionCell,
                        ChanceScrapMound: ChanceScrapMound,
                        RandomScrapMoundBlueprint: RandomScrapMoundBlueprint,
                        RandomScrapWallBlueprint: RandomScrapWallBlueprint,
                        Label: Label,
                        ExtraIndent: ExtraIndent);
                }
            }
        }
        public void ProcessOpenAirCell(
            Cell SubregionCell,
            List<Cell> SubregionCells,
            int ChanceIfOutter,
            int ChanceScrapMound,
            int ChanceOtherwise,
            int ForStrataLessThanFromBottom = 6,
            string Label = null,
            int ExtraIndent = 0)
        {
            ProcessOpenAirCell(
                SubregionCell: SubregionCell,
                SubregionCells: SubregionCells,
                ChanceIfOutter: ChanceIfOutter,
                ChanceScrapMound: ChanceScrapMound,
                ChanceOtherwise: ChanceOtherwise,
                StrataFromBottom: StrataFromBottom,
                ForStrataLessThanFromBottom: ForStrataLessThanFromBottom,
                IsFinalStratum: IsFinalStratum,
                FloorMaterial: FloorMaterial,
                EmptyMaterial: EmptyMaterial,
                RandomScrapMoundBlueprint: RandomScrapMoundBlueprint,
                RandomScrapWallBlueprint: RandomScrapWallBlueprint,
                Label: Label,
                ExtraIndent: ExtraIndent);
        }

        public bool BuildZone(Zone Z)
        {
            Debug.Logger.Header(Verbosity.Max,
                $"{nameof(UD_SubGrandCathedralBuilder)}",
                $"{nameof(BuildZone)}({nameof(Z)}: {Z.ZoneID})",
                Toggle: doDebug);
            Debug.Logger.ResetIndent(out int indent);

            zone = Z;
            CurrentStratum = zone.Z;

            StiltWellCell = zone.GetCell(StiltWellLocation);
            StrataFromBottom = LowestWellStratum - CurrentStratum;
            StrataFromTop = zone.Z - 10;
            IsFinalStratum = StrataFromBottom == 0;

            if (StrataFromBottom < 5)
            {
                RandomScrapWallBlueprint = "RandomScrapWallOrScrapCorpseSometimesGigantic";
            }

            ZoneManager zoneManager = The.ZoneManager;

            EmptyMaterial = StrataFromBottom > 10 ? EmptyMaterial : "QuantumAir";
            FloorMaterial = StrataFromBottom > 10 ? FloorMaterial : "CompactedScrapFloor";

            List<string> cathedralSubStrata = UD_SubStiltWorldBuilderExtension.CathedralSubStrata;
            string zoneBelowID = null;
            if (cathedralSubStrata.Count < StrataFromTop)
            {
                zoneBelowID = cathedralSubStrata[StrataFromTop];
            }
            ZoneBelow = !IsFinalStratum ? zoneManager.GetZone(zoneBelowID) : null;

            Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(StrataFromBottom)}: {StrataFromBottom}, {nameof(StrataFromTop)}: {StrataFromTop}",
                Indent: indent + 1, Toggle: getDoDebug());

            PaintZoneFloors(zone, ZoneBelow, IsFinalStratum, !IsFinalStratum ? "BlackMarbleWalkway" : null);

            UDSW_MostlySolidMaterial mostlySolidBuilder = new(Material: null, Materials: new() { "SolidAir", "Sandstone" }, ClearFirst: true);
            if (StrataFromTop < 6)
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
            if (IsFinalStratum)
            {
                mostlySolidBuilder.Materials.Remove("SolidAir");
                Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(mostlySolidBuilder)}", $"Removed Solid Air from {nameof(mostlySolidBuilder.Materials)}",
                    Indent: indent + 1, Toggle: getDoDebug());
            }
            mostlySolidBuilder.Cells.RemoveAll(c => c == StiltWellCell);
            mostlySolidBuilder.BuildZone(zone);

            if (StrataFromTop < 6)
            {
                Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(Cell)}.{nameof(Cell.Clear)}()",
                    Indent: indent + 1, Toggle: getDoDebug());

                StiltWellCell.Clear(Combat: true);
                if (StrataFromTop == 5)
                {
                    StiltWellCell.AddObject("SelfSacrificeHelper");
                    StiltWellCell.AddObject("SpawnBlocker");
                }
            }
            if (StrataFromTop < 5)
            {
                StiltWellCell.AddObject(EmptyMaterial);

                Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(Cell)}.{nameof(Cell.AddObject)}({EmptyMaterial})",
                    Indent: indent + 1, Toggle: getDoDebug());
            }
            StiltWellCell.AddObject("FlyingWhitelistArea");
            StiltWellCell.AddObject("DaylightCell");

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
            if (StrataFromTop > 2 && StrataFromTop < 11)
            {
                airRadius = getAirRadiusFromStratum(StrataFromTop, StrataFromTop);
            }
            else
            if (StrataFromTop > 10)
            {
                airRadius = getAirRadiusFromStratum(StrataFromTop, StrataFromBottom + 4);
            }

            Debug.Logger.LoopItem(Verbosity.Max, nameof(airRadius), airRadius.ToString(),
                Indent: indent + 1, Toggle: getDoDebug());

            Dictionary<string, List<Cell>> openAirRegion = StiltWellCell.GetCircleRegion(airRadius);
            if (!openAirRegion.IsNullOrEmpty())
            {
                if (openAirRegion.ContainsKey(Inner))
                {
                    openAirRegion[Inner].RemoveAll(c => c == StiltWellCell);
                    if (StrataFromBottom < 10)
                    {
                        openAirRegion[Inner].RemoveAll(c => !c.HasObjectWithBlueprint("Sandstone"));
                    }
                }

                List<Cell> openAirCells = Event.NewCellList(openAirRegion.AllCellsInRegion());
                List<Cell> cellsToAddToOuter = Event.NewCellList();

                int chanceIfOutter = Math.Max(20, Math.Min((9 - StrataFromBottom) * 10, 90));
                int chanceOtherwise = Math.Max(0, Math.Min((7 - StrataFromBottom) * 10, 70));
                int chanceScrapMound = Math.Max(20, Math.Min(StrataFromBottom * 2, 30));
                if (StrataFromBottom < 6)
                {
                    bool doDebugScrapWallMoundChances = getDoDebug("ScrapWallMound Chances");

                    Debug.Logger.LoopItem(Verbosity.Max,
                        nameof(openAirRegion) + " ScrapWallMound Chances (" + 
                        nameof(StrataFromBottom) + ": " + StrataFromBottom + ")...",
                        Indent: indent + 1, Toggle: doDebugScrapWallMoundChances);

                    Debug.Logger.LoopItem(Verbosity.Max, nameof(chanceIfOutter), chanceIfOutter.ToString() + "%",
                        Indent: indent + 2, Toggle: doDebugScrapWallMoundChances);
                    Debug.Logger.LoopItem(Verbosity.Max, nameof(chanceOtherwise), chanceOtherwise.ToString() + "%",
                        Indent: indent + 2, Toggle: doDebugScrapWallMoundChances);
                    Debug.Logger.LoopItem(Verbosity.Max, nameof(chanceScrapMound), chanceScrapMound.ToString() + "%",
                        Indent: indent + 2, Toggle: doDebugScrapWallMoundChances);
                }

                List<Cell> cellsAlreadyRolled = Event.NewCellList();

                bool doDebugSubregionCells = getDoDebug("SubregionCells");
                Debug.Logger.LoopItem(Verbosity.Max, Z.Z + ") " + nameof(openAirRegion),
                    Indent: indent + 2, Toggle: doDebugSubregionCells);
                Debug.Logger.Divider(Verbosity.Max, HONLY, 40, Indent: indent + 2, Toggle: doDebugSubregionCells);
                foreach ((string subregionLabel, List<Cell> subregionCells) in openAirRegion)
                {
                    Debug.Logger.LoopItem(Verbosity.Max, Z.Z + ") " + nameof(subregionLabel), subregionLabel,
                        Indent: indent + 3, Toggle: doDebugSubregionCells);
                    foreach (Cell cell in subregionCells)
                    {
                        ProcessOpenAirCell(
                            SubregionCell: cell,
                            SubregionCells: subregionCells,
                            ChanceIfOutter: chanceIfOutter,
                            ChanceScrapMound: chanceScrapMound,
                            ChanceOtherwise: chanceOtherwise,
                            ForStrataLessThanFromBottom: 6,
                            Label: subregionLabel);

                        if (subregionLabel == Outer)
                        {
                            foreach (Cell adjacentCell in cell.GetAdjacentCells())
                            {
                                if (!openAirCells.Contains(adjacentCell))
                                {
                                    int errosionRollSuccess = 4;
                                    if (StrataFromBottom < 3)
                                    {
                                        errosionRollSuccess = 5;
                                    }
                                    if (!cellsAlreadyRolled.Contains(adjacentCell)
                                        && !cellsToAddToOuter.Contains(adjacentCell)
                                        && Stat.RollCached("1d9") < errosionRollSuccess)
                                    {
                                        ProcessOpenAirCell(
                                            SubregionCell: adjacentCell,
                                            SubregionCells: subregionCells,
                                            ChanceIfOutter: chanceIfOutter,
                                            ChanceScrapMound: chanceScrapMound,
                                            ChanceOtherwise: chanceOtherwise,
                                            ForStrataLessThanFromBottom: 5,
                                            Label: subregionLabel,
                                            ExtraIndent: 1);

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
                    Debug.Logger.Divider(Verbosity.Max, HONLY, 40, Indent: indent + 2, Toggle: doDebugSubregionCells);
                }
                if (!cellsToAddToOuter.IsNullOrEmpty())
                {
                    openAirRegion[Outer].AddRange(cellsToAddToOuter);
                }
                /*
                if (strataFromBottom < 9)
                {
                    bool doDebugBreakScrap = getDoDebug();

                    Debug.Logger.LoopItem(Verbosity.Max, $"Breaking/Deleting select Scrap Walls and Mounds",
                        Indent: indent + 1, Toggle: doDebugBreakScrap);

                    Debug.Logger.Divider(Verbosity.Max, HONLY, 40, Indent: indent + 2, Toggle: doDebugBreakScrap);
                    foreach (Cell scrapWallCell in zone.GetCellsWithObject(GO => GO.GetBlueprint().InheritsFrom("BaseScrapWall")))
                    {
                        bool deletedOrDestroyed = DeleteOrDestroyScrapWallInCell(scrapWallCell);

                        Debug.Logger.LoopItem(Verbosity.Max, $"({currentStratum})[{scrapWallCell.Location}]",
                            Good: deletedOrDestroyed, Indent: indent + 2, Toggle: doDebugBreakScrap);
                    }
                    Debug.Logger.Divider(Verbosity.Max, HONLY, 40, Indent: indent + 2, Toggle: doDebugBreakScrap);
                }
                */
            }

            if (IsFinalStratum)
            {
                bool doDebugFinalStratum = getDoDebug();
                Debug.Logger.LoopItem(Verbosity.Max, $"Rolling Tier 3-6 Relic by way of ExplodingDie and setting {nameof(HolyPlace)}",
                    Indent: indent + 1, Toggle: doDebugFinalStratum);

                int tier = new DieRoll("1d3").Explode(3, 1, 6);
                if (RelicGenerator.GenerateRelic(Tier: tier, RandomName: true) is GameObject stiltWellRelic)
                {
                    StiltWellCell.Clear(Combat: true).AddObject(stiltWellRelic);

                    Debug.Logger.CheckYeh(Verbosity.Max, $"{nameof(stiltWellRelic)} (T:{tier})", $"{stiltWellRelic.DebugName}",
                        Indent: indent + 2, Toggle: doDebugFinalStratum);
                }
                else
                {
                    StiltWellCell.Clear(Combat: true).AddPopulation("Artifact 8R");

                    Debug.Logger.CheckNah(Verbosity.Max, $"Relic Failed, Adding from population", $"Artifact 8R",
                        Indent: indent + 2, Toggle: doDebugFinalStratum);
                }
                if (GameObjectFactory.Factory.CreateObject("Six Day Stilt Recoiler") is GameObject stiltRecoiler)
                {
                    StiltWellCell.AddObject(stiltRecoiler);

                    Debug.Logger.CheckYeh(Verbosity.Max, $"{nameof(stiltRecoiler)}", $"{stiltRecoiler.DebugName}",
                        Indent: indent + 2, Toggle: doDebugFinalStratum);
                }
                for (int i = 0; i < Stat.RandomCosmetic(1, 3); i++)
                {
                    int wedgeValue = Math.Max(1, Stat.RandomCosmetic(2, 3) - i);
                    string creditWedgeBlueprint = $"CyberneticsCreditWedge" + (wedgeValue == 1 ? "" : wedgeValue);
                    if (GameObjectFactory.Factory.CreateObject(creditWedgeBlueprint) is GameObject creditWedge)
                    {
                        StiltWellCell.AddObject(creditWedge);

                        Debug.Logger.CheckYeh(Verbosity.Max, $"{nameof(creditWedge)}", $"{creditWedge.DebugName}",
                            Indent: indent + 2, Toggle: doDebugFinalStratum);
                    }
                }
                if (StiltWellCell.AddObject("HolyPlaceWidget").TryGetPart(out HolyPlace holyPlace))
                {
                    holyPlace.Faction = "Mechanimists";
                }
                Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(holyPlace)}{nameof(holyPlace.Faction)}", $"{holyPlace?.Faction ?? NULL}",
                    Good: holyPlace?.Faction == "Mechanimists", Indent: indent + 2, Toggle: doDebugFinalStratum);
            }

            // PaintZoneFloors(zone, ZoneBelow, isFinalStratum, floorMaterial);

            Debug.Logger.SetIndent(indent);
            Debug.Logger.Footer(Verbosity.Max, 
                $"{nameof(UD_SubGrandCathedralBuilder)}", 
                $"{nameof(BuildZone)}({nameof(Z)}: {Z.ZoneID})", 
                Toggle: doDebug);
            return true;
        }
    }
}
