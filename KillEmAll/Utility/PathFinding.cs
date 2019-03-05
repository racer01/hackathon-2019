using Hackathon.Public;
using KillEmAll.Enums;
using KillEmAll.Helpers.Interfaces;
using KillEmAll.Utility.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillEmAll.Utility
{
    public class WeightedPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int GScore { get; set; }
        public int Heuristic { get; set; }
        public int TotalScore { get; set; }
        public WeightedPoint Parent { get; set; }
        public RelativePosition RelativePosition { get; set; }

        public override int GetHashCode()
        {
            return X.GetHashCode() * 17 + Y.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is WeightedPoint))
                return false;

            var point = (WeightedPoint)obj;
            return X == point.X && Y == point.Y;
        }
    }

    public class PathFinding : IPathFinding
    {
        private const int X = 0;
        private const int Y = 1;
        private readonly int MAP_HEIGHT;
        private readonly int MAP_WIDTH;

        private const int DIAGONAL_G = 14;
        private const int STRAIGHT_G = 10;

        private const int DEFAULT_GSCORE = int.MaxValue;
        private const int DEFAULT_FSCORE = int.MaxValue;

        private int[] _currentPoint = new int[2];

        private IWallMapping _map;

        public PathFinding(IWallMapping map, int width, int height)
        {
            _map = map;
            MAP_HEIGHT = height;
            MAP_WIDTH = width;
        }

        public List<PointF> Asd(PointF start, PointF target)
        {
            var weightedStart = new WeightedPoint()
            {
                X = (int)start.X,
                Y = (int)start.Y
                // WEIGH THIS?
            };

            // The set of nodes already evaluated
            var closedSet = new HashSet<WeightedPoint>();

            // The set of currently discovered nodes that are not evaluated yet.
            // Initially, only the start node is known.
            var openSet = new HashSet<WeightedPoint>() { weightedStart };

            // For each node, which node it can most efficiently be reached from.
            // If a node can be reached from many nodes, cameFrom will eventually contain the
            // most efficient previous step.
            var cameFrom = new Dictionary<WeightedPoint, WeightedPoint>();

            // For each node, the cost of getting from the start node to that node.
            var gScore = new Dictionary<WeightedPoint, int>();

            gScore[weightedStart] = 0;

            // For each node, the total cost of getting from the start node to the goal
            // by passing by that node. That value is partly known, partly heuristic.
            var fScore = new Dictionary<WeightedPoint, int>();

            var startingHeuristic = CalculateHeuristic(weightedStart, new int[] { (int)target.X, (int)target.Y });
            fScore[weightedStart] = startingHeuristic;

            // fScore reverse lookup
            // TREAT THIS THE SAME AS OPEN DICT
            var reverseFScoreLookup = new Dictionary<int, List<WeightedPoint>>() { {startingHeuristic, new List<WeightedPoint>() { weightedStart } } };
            var currentLowestFScore = startingHeuristic;
            var orderedFScores = new List<int>(50) { currentLowestFScore };

            var shouldReorderScores = false;
            while (openSet.Count != 0)
            {
                if (shouldReorderScores)
                    orderedFScores.Sort();

                currentLowestFScore = orderedFScores.First();
                var current = reverseFScoreLookup[currentLowestFScore].Last();

                if (current.X == (int)target.X && current.Y == (int)target.Y)
                    return ReconstructPath(cameFrom, current);

                var fScoreList = reverseFScoreLookup[fScore[current]];
                fScoreList.RemoveAt(fScoreList.Count - 1);
                if (fScoreList.Count == 0)
                {
                    reverseFScoreLookup.Remove(fScore[current]);
                    orderedFScores.Remove(currentLowestFScore);
                }

                openSet.Remove(current);
                closedSet.Add(current);

                var neighbors = _map.GetNeighbourCells(new int[] { current.X, current.Y }, searchType: MapCell.Empty, diagonalCheck: DiagonalCheckType.Never, includeUnknowns: true); // DONT INCLUDE DIAGONAL NEIGHBORS BECAUSE THE SOLDIER GETS STUCK ON THE CORNER OF THE BLOCKS

                for (var i = 0; i < neighbors.Count; i++)
                {
                    var neighbor = neighbors[i];
                    if (closedSet.Contains(neighbor))
                        continue;

                    var tentativeGSore = gScore[current] + DistanceBetween(current, neighbor);

                    var remapScore = false;
                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                    else if (tentativeGSore >= gScore[neighbor])
                        continue;
                    else
                        remapScore = true;

                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGSore;

                    var score = tentativeGSore + CalculateHeuristic(neighbor, new int[] { (int)target.X, (int)target.Y });
                    if (score == 38)
                        Console.WriteLine();

                    if (remapScore)
                    {
                        var oldScore = fScore[neighbor];
                        reverseFScoreLookup[oldScore].Remove(neighbor);
                        if (reverseFScoreLookup[oldScore].Count == 0)
                        {
                            reverseFScoreLookup.Remove(oldScore);
                            orderedFScores.Remove(oldScore);
                        }
                    }

                    fScore[neighbor] = score;

                    if (orderedFScores.Count == 0 || !orderedFScores.Contains(score))
                    {
                        orderedFScores.Add(score);
                        shouldReorderScores = true;
                    }

                    if (reverseFScoreLookup.ContainsKey(score))
                        reverseFScoreLookup[score].Add(neighbor);
                    else
                        reverseFScoreLookup.Add(score, new List<WeightedPoint>() { neighbor });
                }
            }
            return new List<PointF>();
        }

        private void RemoveFromOrderedFScores(ref int[] orderedFScores, int value)
        {
            //for (var i = 0;)
        }

        private void StoreInOrderedFScores(int value, ref int[] orderedFScores)
        {
            if (orderedFScores[0] == 0)
            {
                orderedFScores[0] = value;
                return;
            }

            int prev;
            int current = orderedFScores.FirstOrDefault();
            int next;
            for (var i = 0; i < orderedFScores.Length; i++)
            {
                prev = current;
                current = orderedFScores[i];

                if (orderedFScores[i] < value)
                    continue;

                orderedFScores[i] = value;
                next = current;
                ShiftOrderedFScoreValues(ref orderedFScores, i + 1, current);
                return;
            }
        }

        private void ShiftOrderedFScoreValues(ref int[] orderedFScores, int startingIndex, int prev)
        {
            int current;
            for (var i = startingIndex; i < orderedFScores.Length; i++)
            {
                current = orderedFScores[i];
                if (current == 0)
                {
                    orderedFScores[i] = prev;
                    return;
                }
                
                orderedFScores[i] = prev;
                prev = current;

                if (i == orderedFScores.Length - 1)
                {
                    var newArray = new int[orderedFScores.Length * 2];
                    Array.Copy(orderedFScores, newArray, orderedFScores.Length);
                    orderedFScores = newArray;

                    orderedFScores[i + 1] = prev;
                }
            }
        }

        private void ReallocateOrderedFScoreValues(ref int[] orderedFScores)
        {

        }

        private int DistanceBetween(WeightedPoint origin, WeightedPoint neighbor)
        {
            if (neighbor.Y == origin.Y || neighbor.X == origin.X)
                return STRAIGHT_G;
            return DIAGONAL_G;
        }

        private List<PointF> ReconstructPath(Dictionary<WeightedPoint, WeightedPoint> cameFrom, WeightedPoint current)
        {
            var result = new List<PointF>() { new PointF(current.X, current.Y) };

            WeightedPoint value;
            while (cameFrom.TryGetValue(current, out value))
            {
                result.Add(new PointF(value.X, value.Y));
                current = value;
            }
            return result;
        }


        // A* algorithm
        public bool FindShortestPathToTarget(WeightedPoint current, int[] target, Dictionary<WeightedPoint, WeightedPoint> closedDict, Dictionary<WeightedPoint, WeightedPoint> openDict, List<WeightedPoint> neighborsOrderedByScore, ref List<PointF> path)
        {
            if (current.Parent == null)
                current.GScore = 0;

            if (closedDict.ContainsKey(current))
                return false;

            closedDict.Add(current, current);
            openDict.Remove(current);

            var startingPoint = new int[2] { current.X, current.Y };

            // GET ALL NON-WALL NEIGHBORS
            var neighbors = _map.GetNeighbourCells(startingPoint);

            neighborsOrderedByScore.Add(neighbors[0]);

            // STORE NEIGHBORS BY THEIR SCORE
            //var openCellCount = 0;
            for (int i = 0; i < neighbors.Count; i++)
            {
                var neighbor = neighbors[i];
                neighbor.Parent = current;
                neighbor.GScore = CalculateGScore(current, neighbor);
                neighbor.Heuristic = CalculateHeuristic(neighbor, target);
                neighbor.TotalScore = neighbor.GScore + neighbor.Heuristic;

                if (neighbor.X == target[X] && neighbor.Y == target[Y])
                {
                    //BuildPath(neighbor, ref path);
                    return true;
                }

                WeightedPoint result;
                if (closedDict.TryGetValue(neighbor, out result))
                    continue;

                if (openDict.TryGetValue(neighbor, out result))
                {
                    // IF GSCORE IS BETTER FROM THIS PARENT, OVERRIDE IT
                    if (result.GScore > neighbor.GScore)
                        openDict[neighbor] = neighbor;
                        
                    //openCellCount++;
                    neighborsOrderedByScore.Add(neighbor);
                }
                else
                {
                    neighborsOrderedByScore.Add(neighbor);
                    //openCellCount++;
                    openDict.Add(neighbor, neighbor);
                }
            }

            neighborsOrderedByScore = neighborsOrderedByScore.OrderBy(nb => nb.TotalScore).ToList();

            //if (openCellCount <= 0)
            //    return false;

            // START WITH LOWEST SCORE NEIGHBOR
            for (var i = 0; i < neighborsOrderedByScore.Count; i++)
            {
                if (FindShortestPathToTarget(neighborsOrderedByScore[i], target, new Dictionary<WeightedPoint, WeightedPoint>(closedDict), new Dictionary<WeightedPoint, WeightedPoint>(openDict), new List<WeightedPoint>(), ref path))
                    return true;
            }
            return false;
        }

        //private void BuildPath(WeightedPoint point, ref List<PointF> path)
        //{
        //    if (point.Parent == null)
        //        return;

        //    path.Add(new PointF(point.Parent.X, point.Parent.Y));

        //    BuildPath(point.Parent, ref path);
        //}



        private int CalculateGScore(WeightedPoint origin, WeightedPoint neighbor)
        {
            if (neighbor.RelativePosition.HasFlag(RelativePosition.ABOVE | RelativePosition.RIGHT) 
                    || neighbor.RelativePosition.HasFlag(RelativePosition.ABOVE | RelativePosition.LEFT)
                    || neighbor.RelativePosition.HasFlag(RelativePosition.BELOW | RelativePosition.RIGHT)
                    || neighbor.RelativePosition.HasFlag(RelativePosition.BELOW | RelativePosition.LEFT))
                return origin.GScore + DIAGONAL_G;

            return origin.GScore + STRAIGHT_G;
        }

        private int CalculateHeuristic(WeightedPoint point, int[] target)
        {
            var xDiff = Math.Abs(point.X - target[X]);
            var yDiff = Math.Abs(point.Y - target[Y]);

            return xDiff + yDiff;
        }
    }
}
