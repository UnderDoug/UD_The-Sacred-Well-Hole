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
        public const string JUNKPILE_SOLID_CELLS_PROP = "Junk Pile Solid Cells List";
        public const string STILT_WELL_REGION = "Stilt Well";
        public const string OPEN_AIR_REGION = "Open Air";
        public const string SOLID_JUNKPILE_BELOW_REGION = "Solid Junk Pile Below";
        public const string JUNKPILE_REGION = "Junk Pile";

        public static string Inner = $"{nameof(Inner)}";
        public static string Outer = $"{nameof(Outer)}";
        public static string Corners = $"{nameof(Corners)}";
        public static string NorthEdge = $"{nameof(NorthEdge)}";
        public static string SouthEdge = $"{nameof(SouthEdge)}";
        public static string EastEdge = $"{nameof(EastEdge)}";
        public static string WestEdge = $"{nameof(WestEdge)}";
        public static string Door = $"{nameof(Door)}";

        public Dictionary<string, Dictionary<string, List<Cell>>> Regions;

        public Location2D StiltWellLocation;
        public List<Location2D> SolidJunkPileCellsBelow;
        public bool WantScrappy;

        public string EmptyMaterial => UD_SubStiltBuilderExtension.EmptyMaterial;

        public UD_SubGrandCathedralBuilder()
        {
            StiltWellLocation = new(38, 12);
            SolidJunkPileCellsBelow = new();
            WantScrappy = false;
            Regions = new()
            {
                { STILT_WELL_REGION, new() { { nameof(StiltWellLocation), new() }, } },
                { OPEN_AIR_REGION, new() },
                { SOLID_JUNKPILE_BELOW_REGION, new() },
                { JUNKPILE_REGION, new() },
            };
        }
        public UD_SubGrandCathedralBuilder(Location2D StiltWellLocation = null, List<Location2D> SolidJunkPileCellsBelow = null, bool? WantScrappy = null)
            : this ()
        {
            this.StiltWellLocation = StiltWellLocation ?? new(38, 12);
            this.SolidJunkPileCellsBelow = SolidJunkPileCellsBelow ?? new();
            this.WantScrappy = WantScrappy ?? false;
        }

        public IEnumerable<Location2D> BuildZone(Zone Z, Location2D StiltWellLocation = null, List<Location2D> SolidJunkPileCellsBelow = null, bool? WantScrappy = null)
        {
            UnityEngine.Debug.LogError($"{nameof(UD_SubGrandCathedralBuilder)}.{nameof(BuildZone)}()");
            this.StiltWellLocation = StiltWellLocation ?? this.StiltWellLocation;
            this.SolidJunkPileCellsBelow = SolidJunkPileCellsBelow ?? this.SolidJunkPileCellsBelow;
            this.WantScrappy = WantScrappy ?? this.WantScrappy;

            bool zoneBuilt = BuildZone(Z);
            foreach (Location2D solidJunkLocation in this.SolidJunkPileCellsBelow)
            {
                yield return solidJunkLocation;
            }
            yield break;
        }
        public bool BuildZone(Zone Z)
        {
            zone = Z;

            ZoneManager zoneManager = The.ZoneManager;

            SolidJunkPileCellsBelow = new(zoneManager
                .GetZone(zone.ZoneWorld, zone.wX, zone.wY, zone.X, zone.Y, zone.Z + 1)
                .GetObjectsThatInheritFrom("MachineWall")
                .LocationRegionFromGameObjectList());

            UnityEngine.Debug.LogError($"{nameof(UD_SubGrandCathedralBuilder)}, {nameof(zone)}: {zone.ZoneID}");

            string junkPileSolidCellsListString = SolidJunkPileCellsBelow.StringFromRegion();
            UnityEngine.Debug.LogError($"    {nameof(SolidJunkPileCellsBelow)}: {junkPileSolidCellsListString}");
            new SolidMaterial("Sandstone").BuildZone(zone);

            Cell stiltWellCell = zone.GetCell(StiltWellLocation).Clear();
            Regions[STILT_WELL_REGION][nameof(StiltWellLocation)] = new() { stiltWellCell };

            bool cellInRegion(Cell cell, string region)
            {
                return Regions[region][Inner].Contains(cell)
                    || Regions[region][Outer].Contains(cell);
            }

            int strataFromBottom = 20 - zone.Z;
            int strataFromTop = 10 - strataFromBottom;
            int strataFrom5 = Math.Abs(5 - strataFromBottom);
            if (strataFromBottom > 4)
            {
                stiltWellCell.AddObject("Air");
            }
            int airRadius = Math.Max(0, (int)(strataFromTop * 1.75));
            int junkPileRadius = Math.Max(0, airRadius - 3);
            bool wantOpenAir = airRadius > 0;
            bool haveSolidBelow = !SolidJunkPileCellsBelow.IsNullOrEmpty();
            bool wantJunkPile = junkPileRadius > 0 && strataFromTop > 4;

            List<Cell> solidBelow = new();
            if (haveSolidBelow)
            {
                Regions[SOLID_JUNKPILE_BELOW_REGION].Add(Inner, new(zone.CellRegionFromLocationRegion(SolidJunkPileCellsBelow)));
                solidBelow = Regions[SOLID_JUNKPILE_BELOW_REGION].AllCellsInRegion() as List<Cell>;
            }

            if (wantJunkPile)
            {
                Regions[JUNKPILE_REGION] = stiltWellCell.GetCircleRegion(junkPileRadius, Filter: c => !haveSolidBelow || solidBelow.Contains(c));
                Regions[JUNKPILE_REGION][Inner]?.Remove(stiltWellCell);
                List<Cell> solidBelowCells = new(Regions[SOLID_JUNKPILE_BELOW_REGION].AllCellsInRegion());
                if (haveSolidBelow)
                {
                    foreach ((string subregionLabel, List<Cell> Cells) in Regions[JUNKPILE_REGION])
                    {
                        List<Cell> subregionCellsToRemove = new();
                        if (!Cells.IsNullOrEmpty())
                        {
                            Cells.RemoveAll(c => !solidBelowCells.Contains(c));
                        }
                    }
                    List<Cell> junkPileInner = new(Regions[JUNKPILE_REGION][Inner]);
                    foreach (Cell cell in junkPileInner)
                    {
                        List<Cell> adjacentCellsInRegion = cell.GetAdjacentCells();
                        if (adjacentCellsInRegion.Count != adjacentCellsInRegion.RemoveAll(c => cellInRegion(c, OPEN_AIR_REGION)))
                        {
                            Regions[JUNKPILE_REGION][Inner].Remove(cell);
                            Regions[JUNKPILE_REGION][Outer].Add(cell);
                        }
                    }
                }
            }

            if (wantOpenAir)
            {
                Regions[OPEN_AIR_REGION] = stiltWellCell.GetCircleRegion(airRadius, Filter: c => !haveSolidBelow || !solidBelow.Contains(c));
                List<Cell> solidBelowCells = new(Regions[SOLID_JUNKPILE_BELOW_REGION].AllCellsInRegion());
                if (haveSolidBelow)
                {
                    foreach ((string subregionLabel, List<Cell> Cells) in Regions[OPEN_AIR_REGION])
                    {
                        if (!Cells.IsNullOrEmpty())
                        {
                            Regions[OPEN_AIR_REGION][subregionLabel].RemoveAll(c => solidBelowCells.Contains(c));
                        }
                    }
                    List<Cell> junkPileInner = new(Regions[OPEN_AIR_REGION][Inner]);
                    foreach (Cell cell in junkPileInner)
                    {
                        List<Cell> adjacentCellsInRegion = cell.GetAdjacentCells();
                        if (adjacentCellsInRegion.Count != adjacentCellsInRegion.RemoveAll(c => cellInRegion(c, OPEN_AIR_REGION)))
                        {
                            Regions[OPEN_AIR_REGION][Inner].Remove(cell);
                            Regions[OPEN_AIR_REGION][Outer].Add(cell);
                        }
                    }
                }
            }
            if (!Regions[OPEN_AIR_REGION].IsNullOrEmpty() && Regions[OPEN_AIR_REGION].ContainsKey(Inner))
            {
                Regions[OPEN_AIR_REGION][Inner].Remove(stiltWellCell);
            }

            if (wantOpenAir && !Regions[OPEN_AIR_REGION].IsNullOrEmpty())
            {
                List<Cell> openAirCells = new(Regions[OPEN_AIR_REGION].AllCellsInRegion());
                List<Cell> cellsToAddToOuter = new();
                foreach ((string subregionLabel, List<Cell> subregionCells) in Regions[OPEN_AIR_REGION])
                {
                    if (subregionLabel == Inner || subregionLabel == Outer)
                    {
                        List<Cell> cellsAlreadyRolled = new();
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
                    Regions[OPEN_AIR_REGION][Outer].AddRange(cellsToAddToOuter);
                }
            }

            if (wantJunkPile && !Regions[JUNKPILE_REGION].IsNullOrEmpty())
            {
                List<Cell> junkPileCells = new(Regions[JUNKPILE_REGION].AllCellsInRegion());
                List<Cell> cellsToRemoveFromOuter = new();
                foreach ((string subregionLabel, List<Cell> subregionCells) in Regions[JUNKPILE_REGION])
                {
                    if (subregionLabel == Inner || subregionLabel == Outer)
                    {
                        List<Cell> cellsAlreadyRolled = new();
                        foreach (Cell cell in subregionCells)
                        {
                            string table = "JunkOrMachineWallOrNothing";
                            if (subregionLabel == Inner)
                            {
                                table = "JunkOrMachineWallButProbablyWall";
                            }
                            if (!cell.GetCardinalAdjacentCellsWhere(c => c.HasObject(EmptyMaterial)).IsNullOrEmpty())
                            {
                                PopulationResult populationResult = PopulationManager.GenerateOne(table);
                                if (populationResult != null && populationResult.Blueprint != "nothing")
                                {
                                    cell.Clear().AddObject(populationResult.Blueprint);
                                }
                            }
                            else
                            {
                                if (!cellsToRemoveFromOuter.Contains(cell))
                                {
                                    cellsAlreadyRolled.Add(cell);
                                }
                            }
                        }
                    }
                }
                if (!cellsToRemoveFromOuter.IsNullOrEmpty())
                {
                    Regions[JUNKPILE_REGION][Outer].RemoveAll(c => cellsToRemoveFromOuter.Contains(c));
                }
                junkPileCells = new(Regions[JUNKPILE_REGION].AllCellsInRegion());
                foreach ((string regionLabel, List<Cell> _) in Regions[SOLID_JUNKPILE_BELOW_REGION])
                {
                    Regions[SOLID_JUNKPILE_BELOW_REGION][regionLabel].RemoveAll(c => junkPileCells.Contains(c));
                }
            }
            if (strataFromBottom == 0 && stiltWellCell != null)
            {
                stiltWellCell?.AddObject("Timecube");
            }
            else if (strataFromBottom < 6 && stiltWellCell != null)
            {
                PopulationResult populationResult = PopulationManager.GenerateOne("RandomMachineWall");
                if (populationResult != null && populationResult.Blueprint != "nothing")
                {
                    stiltWellCell.Clear().AddObject(populationResult.Blueprint);
                }
            }
            if (haveSolidBelow)
            {
                List<Cell> solidBelowCells = new(Regions[SOLID_JUNKPILE_BELOW_REGION].AllCellsInRegion());
                foreach (Cell floorCell in solidBelowCells)
                {
                    PopulationResult populationResult = PopulationManager.GenerateOne("JunkOrGarbage");
                    floorCell.Clear().AddObject(populationResult.Blueprint);
                }
            }

            List<string> junkPileWallBlueprints = PopulationManager.GetEach("RandomMachineWall");
            UnityEngine.Debug.LogError($"    {nameof(junkPileWallBlueprints)}");
            foreach (string blueprint in junkPileWallBlueprints)
            {
                UnityEngine.Debug.LogError($"        {blueprint}");
            }
            foreach (Cell solidCell in zone.GetCells())
            {
                if (solidCell.HasObjectWithBlueprintStartsWith("MachineWall"))
                {
                    SolidJunkPileCellsBelow.Add(solidCell.Location);
                }
            }

            junkPileSolidCellsListString = SolidJunkPileCellsBelow.StringFromRegion();
            UnityEngine.Debug.LogError($"    {nameof(junkPileSolidCellsListString)}: {junkPileSolidCellsListString}");
            zone.SetZoneProperty(JUNKPILE_SOLID_CELLS_PROP, junkPileSolidCellsListString);

            UnityEngine.Debug.LogError($"    {nameof(WantScrappy)}: {WantScrappy}");

            if (WantScrappy)
            {
                new UD_SubGrandCathedralScrapifier(strataFromBottom == 0).BuildZone(zone);
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
