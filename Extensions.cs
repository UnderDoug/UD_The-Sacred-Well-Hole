using System;
using System.Collections.Generic;
using System.Text;

using Genkit;

using XRL.World;

namespace UD_SacredWellHole
{
    public static class Extensions
    {
        public static Dictionary<string, List<Cell>> GetHutRegion(this Zone Z, Rect2D R, bool Round = false)
        {
            string Inner = $"{nameof(Inner)}";
            string Outer = $"{nameof(Outer)}";
            string Corners = $"{nameof(Corners)}";
            string NorthEdge = $"{nameof(NorthEdge)}";
            string SouthEdge = $"{nameof(SouthEdge)}";
            string EastEdge = $"{nameof(EastEdge)}";
            string WestEdge = $"{nameof(WestEdge)}";
            string Door = $"{nameof(Door)}";
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
    }
}
