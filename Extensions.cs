using System;
using System.Collections.Generic;
using System.Text;

using Genkit;

using XRL.World;

namespace UD_SacredWellHole
{
    public static class Extensions
    {
        public static string Inner = $"{nameof(Inner)}";
        public static string Outer = $"{nameof(Outer)}";
        public static string Corners = $"{nameof(Corners)}";
        public static string NorthEdge = $"{nameof(NorthEdge)}";
        public static string SouthEdge = $"{nameof(SouthEdge)}";
        public static string EastEdge = $"{nameof(EastEdge)}";
        public static string WestEdge = $"{nameof(WestEdge)}";
        public static string Door = $"{nameof(Door)}";

        public static Dictionary<string, List<Cell>> GetCircleRegion(this Cell FromCell, int Radius = 3, bool IncludePerimerter = true, List<Cell> ExcludeCells = null, Predicate<Cell> Filter = null)
        {
            Dictionary<string, List<Cell>> Region = new()
            {
                { Inner, new() },
            };
            Region[Inner] = new(FromCell.GetCellsInACosmeticCircle(IncludePerimerter ? Radius - 1 : Radius, true, ExcludeCells, Filter))
            {
                FromCell,
            };
            if (IncludePerimerter)
            {
                Region.Add(Outer, new());
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
                    float num = Math.Abs(x - Cell.X);
                    float num2 = (float)Math.Abs(y - Cell.Y) * 1.3333f;
                    float num3 = num * num + num2 * num2;
                    if (!Silent) UnityEngine.Debug.Log("xd: " + num + " yd:" + num2 + " d=" + num3);
                    if (num3 <= radius_squared && Cell.ParentZone.GetCell(x, y) != null)
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
    }
}
