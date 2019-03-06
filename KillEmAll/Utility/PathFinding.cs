using Hackathon.Public;
using KillEmAll.Enums;
using KillEmAll.Helpers.Interfaces;
using KillEmAll.Utility.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private const int DIAGONAL_G = 14;
        private const int STRAIGHT_G = 10;


        private IWallMapping _map;

        public PathFinding(IWallMapping map)
        {
            _map = map;
        }

        public List<PointF> GetPath(PointF start, PointF target)
        {
            var weightedStart = new WeightedPoint()
            {
                X = (int)start.X,
                Y = (int)start.Y
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

                foreach (var neighbor in neighbors)
                {
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

        private int CalculateHeuristic(WeightedPoint point, int[] target)
        {
            var xDiff = Math.Abs(point.X - target[X]);
            var yDiff = Math.Abs(point.Y - target[Y]);

            return xDiff + yDiff;
        }
    }
}
