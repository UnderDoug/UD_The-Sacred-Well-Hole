using System;
using System.Collections.Generic;
using System.Text;

using Genkit;

using XRL.Rules;
using XRL.World;
using XRL.World.Parts;
using XRL.World.ZoneBuilders;

using static UD_SacredWellHole.Options;

namespace UD_SacredWellHole
{
    public static class Extensions
    {
        private static bool doDebug => getClassDoDebug(nameof(Extensions));
        public static bool getDoDebug(string MethodName)
        {
            if (MethodName == nameof(PullInsideFromEdges))
                return false;

            if (MethodName == nameof(PullInsideFromEdge))
                return false;

            if (MethodName == nameof(Explode))
                return true;

            return doDebug;
        }
        public static string Inner = $"{nameof(Inner)}";
        public static string Outer = $"{nameof(Outer)}";
        public static string Corners = $"{nameof(Corners)}";
        public static string NorthEdge = $"{nameof(NorthEdge)}";
        public static string SouthEdge = $"{nameof(SouthEdge)}";
        public static string EastEdge = $"{nameof(EastEdge)}";
        public static string WestEdge = $"{nameof(WestEdge)}";
        public static string Door = $"{nameof(Door)}";

        public static T Sample<T>(this Dictionary<T, int> WeightedList)
            where T : class
        {
            T Output = default;
            int weightMax = 0;
            foreach ((_, int entryWeight) in WeightedList)
            {
                weightMax += entryWeight;
            }
            int ticket = Stat.Roll(1, weightMax);
            int weightCurrent = 0;
            foreach ((T entryT, int entryWeight) in WeightedList)
            {
                weightCurrent += entryWeight;
                if (ticket <= weightCurrent)
                {
                    Output = entryT;
                    break;
                }
            }
            return Output;
        }
        public static T Draw<T>(this Dictionary<T, int> WeightedList)
            where T : class
        {
            T ticket = WeightedList.Sample();
            if (--WeightedList[ticket] == 0)
            {
                WeightedList.Remove(ticket);
            }
            return ticket;
        }
        public static void AddTicket<T>(this Dictionary<T, int> WeightedList, T Ticket)
            where T : class
        {
            if (WeightedList.ContainsKey(Ticket))
            {
                WeightedList[Ticket]++;
            }
            else
            {
                WeightedList.Add(Ticket, 1);
            }
        }

        public static Dictionary<string, List<Cell>> GetCircleRegion(this Cell FromCell, int Radius = 3, bool IncludePerimerter = true, List<Cell> ExcludeCells = null, Predicate<Cell> Filter = null)
        {
            Dictionary<string, List<Cell>> Region = new()
            {
                { Inner, Event.NewCellList() },
            };
            Region[Inner] = new(FromCell.GetCellsInACosmeticCircle(IncludePerimerter ? Radius - 1 : Radius, true, ExcludeCells, Filter))
            {
                FromCell,
            };
            if (IncludePerimerter)
            {
                Region.Add(Outer, Event.NewCellList());
                foreach (Cell outerCell in FromCell.GetCellsInACosmeticCircle(Radius, true, ExcludeCells, Filter))
                {
                    if (!Region[Inner].Contains(outerCell))
                    {
                        Region[Outer].Add(outerCell);
                    }
                }
            }
            return Region;
        }
        public static IEnumerable<Cell> GetCellsInACosmeticCircle(this Cell Cell, int Radius, bool Silent = true, List<Cell> ExcludeCells = null, Predicate<Cell> Filter = null)
        {
            int yradius = (int)Math.Max(1.0, (double)Radius * 0.66);
            float radius_squared = Radius * Radius;
            for (int x = Cell.X - Radius; x <= Cell.X + Radius; x++)
            {
                for (int y = Cell.Y - yradius; y <= Cell.Y + yradius; y++)
                {
                    float xd = Math.Abs(x - Cell.X);
                    float yd = (float)Math.Abs(y - Cell.Y) * 1.3333f;
                    float d = xd * xd + yd * yd;
                    if (!Silent) UnityEngine.Debug.Log("xd: " + xd + " yd:" + yd + " d=" + d);
                    if (d <= radius_squared && Cell.ParentZone.GetCell(x, y) != null)
                    {
                        Cell cellOut = Cell.ParentZone.GetCell(x, y);
                        if (ExcludeCells == null || !ExcludeCells.Contains(cellOut) && Filter == null || Filter(cellOut))
                        {
                            yield return cellOut;
                        }
                    }
                }
            }
        }

        public static Dictionary<string, List<Cell>> GetHutRegion(this Zone Z, Rect2D R, bool Round = false)
        {
            Dictionary<string, List<Cell>> Region = new()
            {
                { Inner, new() },
                { Outer, new() },
                { Corners, new() },
                { NorthEdge, new() },
                { SouthEdge, new() },
                { EastEdge, new() },
                { WestEdge, new() },
                { Door, new() },
            };
            Rect2D r = R.ReduceBy(1, 1);
            Cell cell;
            for (int i = r.y1; i <= r.y2; i++)
            {
                for (int j = r.x1; j <= r.x2; j++)
                {
                    if ((cell = Z.GetCell(j, i)) == null)
                        continue;

                    Region[Inner].Add(cell);
                }
            }
            if (Round)
            {
                for (int k = R.x1 + 1; k <= R.x2 - 1; k++)
                {
                    if ((cell = Z.GetCell(k, R.y1)) != null)
                        Region[Outer].Add(cell);
                    if ((cell = Z.GetCell(k, R.y2)) != null)
                        Region[Outer].Add(cell);
                }
                for (int l = R.y1 + 1; l <= R.y2 - 1; l++)
                {
                    if ((cell = Z.GetCell(R.x1, l)) != null)
                        Region[Outer].Add(cell);
                    if ((cell = Z.GetCell(R.x2, l)) != null)
                        Region[Outer].Add(cell);
                }

                if ((cell = Z.GetCell(R.x1 + 1, R.y1 + 1)) != null)
                    Region[Outer].Add(cell);
                if ((cell = Z.GetCell(R.x2 - 1, R.y1 + 1)) != null)
                    Region[Outer].Add(cell);
                if ((cell = Z.GetCell(R.x1 + 1, R.y2 - 1)) != null)
                    Region[Outer].Add(cell);
                if ((cell = Z.GetCell(R.x2 - 1, R.y2 - 1)) != null)
                    Region[Outer].Add(cell);
            }
            else
            {
                for (int m = R.x1; m <= R.x2; m++)
                {
                    if ((cell = Z.GetCell(m, R.y1)) != null)
                        Region[Outer].Add(cell);
                    if ((cell = Z.GetCell(m, R.y2)) != null)
                        Region[Outer].Add(cell);
                }
                for (int n = R.y1; n <= R.y2; n++)
                {
                    if ((cell = Z.GetCell(R.x1, n)) != null)
                        Region[Outer].Add(cell);
                    if ((cell = Z.GetCell(R.x2, n)) != null)
                        Region[Outer].Add(cell);
                }
            }

            foreach (Cell outerCell in Region[Outer])
            {
                if (outerCell.Y == R.y1) Region[NorthEdge].Add(outerCell);
                if (outerCell.Y == R.y2) Region[SouthEdge].Add(outerCell);
                if (outerCell.X == R.x2) Region[EastEdge].Add(outerCell);
                if (outerCell.X == R.x1) Region[WestEdge].Add(outerCell);
                if ((outerCell.X == R.x1 || outerCell.X == R.x2) && (outerCell.Y == R.y1 || outerCell.Y == R.y2))
                    Region[Corners].Add(outerCell);
            }
            if (R.Door != null)
            {
                string doorSide = R.GetCellSide(R.Door) switch
                {
                    "N" => NorthEdge,
                    "S" => SouthEdge,
                    "E" => EastEdge,
                    "W" => WestEdge,
                    _ => null,
                };
                if (doorSide != null)
                {
                    Cell newDoor = Region[doorSide].GetRandomElement();

                    Dictionary<string, List<Cell>> Edges = new()
                    {
                        { NorthEdge, Region[NorthEdge] },
                        { SouthEdge, Region[SouthEdge] },
                        { EastEdge, Region[EastEdge] },
                        { WestEdge, Region[WestEdge] },
                    };
                    Point2D newDoor2D = newDoor.PullInsideFromEdges(Edges, doorSide);
                    R.Door.x = newDoor2D.x;
                    R.Door.y = newDoor2D.y;
                }
                Cell door = Z.GetCell(R.Door);
                Region[Door].Add(door);
            }
            return Region;
        }

        public static Point2D PullInsideFromEdges(this Cell Cell, Dictionary<string, List<Cell>> Edges, string DoorSide = "")
        {
            Point2D output = new(Cell.X, Cell.Y);
            foreach ((string Side, List<Cell> Edge) in Edges)
            {
                if (DoorSide == "" || Side == DoorSide)
                    output = Cell.PullInsideFromEdge(Edge);
            }
            return output;
        }
        public static Point2D PullInsideFromEdge(this Cell Cell, List<Cell> Edge)
        {
            Point2D output = new(Cell.X, Cell.Y);
            if (Edge != null && Edge.Contains(Cell))
            {
                List<int> Xs = new();
                List<int> Ys = new();
                string XsString = string.Empty;
                string YsString = string.Empty;
                foreach (Cell cell in Edge)
                {
                    if (!Xs.Contains(cell.X)) Xs.Add(cell.X);
                    if (!Ys.Contains(cell.Y)) Ys.Add(cell.Y);
                }
                foreach (int X in Xs)
                {
                    XsString += XsString == "" ? $"{X}" : $",{X}";
                }
                foreach (int Y in Ys)
                {
                    YsString += YsString == "" ? $"{Y}" : $",{Y}";
                }
                if (Xs.Count > 1 && Ys.Count > 1)
                {
                    return output;
                }
                bool edgeIsLat = Xs.Count > 1;
                int max = int.MinValue;
                int min = int.MaxValue;
                if (edgeIsLat)
                {
                    foreach (int x in Xs)
                    {
                        max = Math.Max(max, x);
                        min = Math.Min(min, x);
                    }
                    if (output.x <= min)
                    {
                        output.x = min + 1;
                    }
                    if (output.x >= max)
                    {
                        output.x = max - 1;
                    }
                }
                else
                {
                    foreach (int y in Ys)
                    {
                        max = Math.Max(max, y);
                        min = Math.Min(min, y);
                    }
                    if (output.y <= min)
                    {
                        output.y = min + 1;
                    }
                    if (output.y >= max)
                    {
                        output.y = max - 1;
                    }
                }
            }

            return output;
        }

        public static IEnumerable<Location2D> RegionFromString(this string String)
        {
            if (!String.IsNullOrEmpty() && String.Length > 2 && String.Contains(',') && !String.EndsWith(',') && !String.EndsWith(','))
            {
                if (!String.Contains(';'))
                {
                    string[] strings = String.Split(',');
                    if (int.TryParse(strings[0], out int locX) && int.TryParse(strings[1], out int locY))
                    {
                        yield return new Location2D(locX, locY);
                    }

                }
                else
                {
                    foreach (string locationString in String.Split(';'))
                    {
                        string[] locationStrings = locationString.Split(',');
                        if (int.TryParse(locationStrings[0], out int locX) && int.TryParse(locationStrings[1], out int locY))
                        {
                            yield return new Location2D(locX, locY);
                        }
                    }
                }
            }
            yield break;
        }

        public static string StringFromRegion(this IEnumerable<Location2D> Region)
        {
            if (!Region.IsNullOrEmpty())
            {
                string output = "";
                foreach (Location2D location in Region)
                {
                    if (!output.IsNullOrEmpty())
                    {
                        output += ";";
                    }
                    output += location.ToString();
                }
            }
            return null;
        }

        public static IEnumerable<Location2D> LocationRegionFromGameObjectList(this IEnumerable<GameObject> ObjectList)
        {
            if (!ObjectList.IsNullOrEmpty())
            {
                foreach (GameObject Object in ObjectList)
                {
                    yield return Object.CurrentCell.Location;
                }
            }
            yield break;
        }
        public static IEnumerable<Cell> CellRegionFromLocationRegion(this Zone Z, IEnumerable<Location2D> Region)
        {
            if (Z != null && !Region.IsNullOrEmpty())
            {
                foreach (Location2D location in Region)
                {
                    yield return Z.GetCell(location);
                }
            }
            yield break;
        }

        public static IEnumerable<Location2D> LocationRegionFromCellRegion(this IEnumerable<Cell> Region)
        {
            if (!Region.IsNullOrEmpty())
            {
                foreach (Cell cell in Region)
                {
                    yield return cell.Location;
                }
            }
            yield break;
        }

        public static IEnumerable<Cell> AllCellsInRegion(this Dictionary<string, List<Cell>> Region)
        {
            if (Region != null)
            {
                foreach ((string _, List<Cell> cells) in Region)
                {
                    if (!cells.IsNullOrEmpty())
                    {
                        foreach (Cell cell in cells)
                        {
                            yield return cell;
                        }
                    }
                }
            }
            yield break;
        }

        public static Cell PaintCell(this Cell C, string Floor = null, string TileColor = null, string DetailtColor = null, string Tile = null, bool Overwrite = true, bool OverrideFloorColors = false)
        {
            UD_SubGrandCathedralBuilder.PaintCell(C, Floor, TileColor, DetailtColor, Tile, Overwrite, OverrideFloorColors);
            return C;
        }

        public static bool IsOuterCell(this Cell C, Predicate<Cell> Basis)
        {
            if (C != null)
            {
                List<Cell> adjacentCells = Event.NewCellList(C.GetAdjacentCells());
                int adjacentBasisCells = 0;
                if (!adjacentCells.IsNullOrEmpty())
                {
                    foreach (Cell adjacentCell  in adjacentCells)
                    {
                        if (Basis(adjacentCell))
                        {
                            adjacentBasisCells++;
                        }
                    }
                    return adjacentBasisCells < adjacentCells.Count;
                }
            }
            return false;
        }

        public static bool IsInnerCell(this Cell C, Predicate<Cell> Basis, bool Strict = true)
        {
            if (C != null)
            {
                List<Cell> adjacentCells = Event.NewCellList(C.GetAdjacentCells());
                int adjacentCardinalCells = 0;
                int adjacentOrdinalCells = 0;
                int adjacentCardinalBasisCells = 0;
                int adjacentOrdinalBasisCells = 0;
                if (!adjacentCells.IsNullOrEmpty())
                {
                    foreach (Cell adjacentCell in adjacentCells)
                    {
                        bool isCardinal = C.X == adjacentCell.X || C.Y == adjacentCell.Y;
                        if (isCardinal)
                        {
                            adjacentCardinalCells++;
                        }
                        else
                        {
                            adjacentOrdinalCells++;
                        }
                        if (Basis(adjacentCell))
                        {
                            if (isCardinal)
                            {
                                adjacentCardinalBasisCells++;
                            }
                            else
                            {
                                adjacentOrdinalBasisCells++;
                            }
                        }
                    }
                    bool cardinalsMatch = adjacentCardinalCells == adjacentCardinalBasisCells;
                    bool ordinalsMatch = adjacentOrdinalCells == adjacentOrdinalBasisCells;
                    return cardinalsMatch && (ordinalsMatch || (!Strict && adjacentOrdinalCells - 1 == adjacentOrdinalBasisCells));
                }
            }
            return false;
        }

        public static int Explode(this DieRoll DieRoll, int Start = 0, int Step = 0, int Limit = 0)
        {
            bool doDebug = getDoDebug(nameof(Explode));
            int indent = Debug.LastIndent;
            Debug.Entry(4,
                $"* {nameof(DieRoll)}."
                + $"{nameof(Explode)}("
                + $"\"{DieRoll}\", "
                + $"{nameof(Start)}: {Start}, "
                + $"{nameof(Step)}: {Step}, "
                + $"{nameof(Limit)}: {Limit})",
                Indent: indent + 1, Toggle: doDebug);

            if (DieRoll == null || DieRoll.Max() < 2)
            {
                Debug.CheckNah(4, $"No {nameof(DieRoll)} or max roll is less than 2", Indent: indent + 2, Toggle: doDebug);
                Debug.LastIndent = indent;
                return Start;
            }

            if (Limit != 0 && Start > Limit)
            {
                Start = Limit;
                Debug.CheckYeh(4, $"{nameof(Limit)} exceeded, returning", Indent: indent + 2, Toggle: doDebug);
                Debug.LastIndent = indent;
                return Start;
            }

            int result = DieRoll.Resolve();
            if (result == DieRoll.Max())
            {
                if (Step == 0)
                {
                    Start += result;
                }
                else
                {
                    Start += Step;
                }
                Debug.CheckYeh(4, $"{nameof(DieRoll)} exploded, we go again!", Indent: indent + 2, Toggle: doDebug);
                int explode = DieRoll.Explode(Start, Step, Limit);
                Debug.LoopItem(4, $"<] returning {explode}...", Indent: indent + 2, Toggle: doDebug);
                Debug.LastIndent = indent;
                return explode;
            }
            if (Step == 0)
            {
                Start += result;
            }
            Debug.CheckYeh(4, $"{nameof(DieRoll)} didn't explode, returning {Start}", Indent: indent + 2, Toggle: doDebug);
            Debug.LastIndent = indent;
            return Start;
        }

        public static int Explode(this string DieRoll, int Start = 0, int Step = 0, int Limit = 0)
        {
            DieRoll dieRoll = new(DieRoll);
            return dieRoll.Explode(Start, Step, Limit);
        }

        public static bool InheritsFrom(this GameObject Object, string Blueprint)
        {
            return Object.Blueprint == Blueprint || Object.GetBlueprint().InheritsFrom(Blueprint);
        }

        public static bool IsAssignableFrom(this IPart @this, IPart Part)
        {
            return @this.GetType().IsAssignableFrom(Part.GetType());
        }

        public static bool InheritsFrom(this IPart @this, Type Type)
        {
            return Type.IsAssignableFrom(@this.GetType());
        }
        public static bool InheritsFrom<T>(this IPart @this)
            where T : IModification
        {
            return typeof(T).IsAssignableFrom(@this.GetType());
        }

        public static IEnumerable<Cell> GetAdjacentCellsInGeneralDirection(this Cell Cell, string Direction)
        {
            List<string> directions = new();
            List<string> adjacentDirections = new();
            if (Direction.IsNullOrEmpty() || Cell == null)
            {
                yield break;
            }
            if (Direction.Length > 1)
            {
                foreach (char c in Direction)
                {
                    directions.Add(c.ToString());
                }
            }
            else
            {
                directions.Add(Direction);
            }
            foreach (Cell adjacentCell in Cell.GetAdjacentCells())
            {
                string adjacentCellDirection = Cell.GetDirectionFromCell(adjacentCell);
                if (adjacentCellDirection.Length > 1)
                {
                    foreach (char c in adjacentCellDirection)
                    {
                        adjacentDirections.Add(c.ToString());
                    }
                }
                else
                {
                    adjacentDirections.Add(adjacentCellDirection);
                }
                foreach (string adjacentDirection in adjacentDirections)
                {
                    if (directions.Contains(adjacentDirection))
                    {
                        yield return adjacentCell;
                    }
                }
            }
            yield break;
        }

        public static bool HasObjectInheritsFrom(this Cell Cell, string Blueprint)
        {
            if (!Blueprint.IsNullOrEmpty())
            {
                foreach (GameObject objectInCell in Cell.GetObjectsInCell())
                {
                    if (objectInCell.InheritsFrom(Blueprint))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool HasObjectInDirectionInheritsFrom(this Cell Cell, string Direction, string Blueprint, bool BuiltOnly = true)
        {
            return Cell.GetCellFromDirection(Direction, BuiltOnly).HasObjectInheritsFrom(Blueprint);
        }
    }
}
