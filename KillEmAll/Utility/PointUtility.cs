﻿using Hackathon.Public;
using KillEmAll.Enums;
using KillEmAll.Utility.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillEmAll.Utility
{


    public class PointUtility : IPointUtility
    {
        private IUtilityCache _cache;
        private const int WALL_SIZE = 1;
        private const int ROUNDING_PRECISION = 1;

        public PointUtility(IUtilityCache cache)
        {
            _cache = cache;
        }

        public float DistanceBetween(PointF point1, PointF point2, bool useCache = false)
        {
            string key = null;
            if (useCache)
            {
                key = _cache.GenerateKey(GetType(), nameof(DistanceBetween), point1.X, point1.Y, point2.X, point2.Y);
                if (_cache.TryGet(key, out float value))
                    return value;
            }

            var result = (point1.X - point2.X) * (point1.X - point2.X) + (point1.Y - point2.Y) * (point1.Y - point2.Y);

            if (useCache)
                _cache.Add(key, result);

            return result;
        }

        // target should be a wall
        public bool IsInBetween(PointF start, PointF end, PointF target, bool useCache = false)
        {
            // because wall positions are always given as whole numbers, round down the soldiers position.
            // nope
            start = new PointF((float)Math.Round(start.X, ROUNDING_PRECISION), (float)Math.Round(start.Y, ROUNDING_PRECISION));
            end = new PointF((float)Math.Round(end.X, ROUNDING_PRECISION), (float)Math.Round(end.Y, ROUNDING_PRECISION));

            //DEBUG
            if (target.X == 12 && target.Y == 12)
                Console.WriteLine();

            string key = null;
            if (useCache)
            {
                // FLOOR THE KEYS IF WE ARE CHECKING FOR WALLS
                key = _cache.GenerateKey(GetType(), nameof(IsInBetween), start.X, start.Y, end.X, end.Y, target.X, target.Y);
                if (_cache.TryGet(key, out bool value))
                {
                    if (value)
                        Console.WriteLine();
                    return value;
                }
            }

            var soldierChangeInX = end.X - start.X;
            var soldierChangeInY = end.Y - start.Y;

            var relativePos = GetRelativePosition(target, 1, start, 0);

            // GET THE CLOSEST 2 CORNERS OF THE WALL TO THE SOLDIER
            PointF corner1 = null;
            PointF corner2 = null;
            if (relativePos.HasFlag(RelativePosition.ABOVE | RelativePosition.RIGHT))
            {
                corner1 = new PointF(target.X, target.Y + WALL_SIZE);
                corner2 = new PointF(target.X + WALL_SIZE, target.Y);
            }
            else if (relativePos.HasFlag(RelativePosition.ABOVE | RelativePosition.LEFT))
            {
                corner1 = new PointF(target.X, target.Y);
                corner2 = new PointF(target.X + WALL_SIZE, target.Y + WALL_SIZE);
            }
            else if (relativePos.HasFlag(RelativePosition.BELOW | RelativePosition.RIGHT))
            {
                corner1 = new PointF(target.X, target.Y);
                corner2 = new PointF(target.X + WALL_SIZE, target.Y + WALL_SIZE);
            }
            else if (relativePos.HasFlag(RelativePosition.BELOW | RelativePosition.LEFT))
            {
                corner1 = new PointF(target.X, target.Y + WALL_SIZE);
                corner2 = new PointF(target.X + WALL_SIZE, target.Y);
            }
            else if (relativePos.HasFlag(RelativePosition.ABOVE))
            {
                corner1 = new PointF(target.X, target.Y + WALL_SIZE);
                corner2 = new PointF(target.X + WALL_SIZE, target.Y + WALL_SIZE);
            }
            else if (relativePos.HasFlag(RelativePosition.BELOW))
            {
                corner1 = new PointF(target.X, target.Y);
                corner2 = new PointF(target.X + WALL_SIZE, target.Y);
            }
            else if (relativePos.HasFlag(RelativePosition.LEFT))
            {
                corner1 = new PointF(target.X, target.Y);
                corner2 = new PointF(target.X, target.Y + WALL_SIZE);
            }
            else if (relativePos.HasFlag(RelativePosition.RIGHT))
            {
                corner1 = new PointF(target.X + WALL_SIZE, target.Y);
                corner2 = new PointF(target.X + WALL_SIZE, target.Y + WALL_SIZE);
            }
            else if (relativePos.HasFlag(RelativePosition.SAME))
            {
                if (target.X > start.X)
                {
                    if (target.Y > start.Y)
                    {
                        corner1 = new PointF(target.X, target.Y + WALL_SIZE);
                        corner2 = new PointF(target.X + WALL_SIZE, target.Y);
                    }
                    else
                    {
                        corner1 = new PointF(target.X, target.Y);
                        corner2 = new PointF(target.X + WALL_SIZE, target.Y + WALL_SIZE);
                    }
                }
                else
                {
                    corner1 = new PointF(target.X, target.Y);
                    corner2 = new PointF(target.X + WALL_SIZE, target.Y + WALL_SIZE);
                }
            }

            var corner1Slope = (corner1.Y - start.Y) / (corner1.X - start.X);
            var corner2Slope = (corner2.Y - start.Y) / (corner2.X - start.X);
            var soldierSlope = soldierChangeInY / soldierChangeInX;

            var isOnLine = false;
            if (corner1Slope <= 0 && corner2Slope <= 0)
            {
                if (corner1Slope > corner2Slope)
                {
                    if (soldierSlope >= corner2Slope && soldierSlope <= corner1Slope)
                        isOnLine = true;
                }
                else
                {
                    if (soldierSlope >= corner1Slope && soldierSlope <= corner2Slope)
                        isOnLine = true;
                }
            }else if (corner2Slope > 0 && corner1Slope > 0)
            {
                if (corner2Slope <= corner1Slope)
                {
                    if (soldierSlope >= corner2Slope && soldierSlope <= corner1Slope)
                        isOnLine = true;
                }
                else
                {
                    if (soldierSlope <= corner2Slope && soldierSlope >= corner1Slope)
                        isOnLine = true;
                }
            }else if (corner2Slope < 0)
            {
                if (soldierSlope >= corner1Slope)
                    isOnLine = true;
                else if (soldierSlope <= corner2Slope)
                    isOnLine = true;
            }else if (corner1Slope < 0)
            {
                if (soldierSlope <= corner1Slope)
                    isOnLine = true;
                else if (soldierSlope >= corner2Slope)
                    isOnLine = true;
            }
          
            if (!isOnLine)
                return false;

            // if the wall is on the line, we have to calculate if it's between our soldier and the enemy
            bool betweenOnXAxis = false;
            bool betweenOnYAxis = false;

            if (start.X >= end.X)
            {
                if (target.X >= end.X && target.X <= start.X)
                    betweenOnXAxis = true;
            }
            else
            {
                if (target.X >= start.X && target.X <= end.X)
                    betweenOnXAxis = true;
            }

            if (start.Y >= end.Y)
            {
                if (target.Y <= start.Y && target.Y >= end.Y)
                    betweenOnYAxis = true;
            }
            else
            {
                if (target.Y >= start.Y && target.Y <= end.Y)
                    betweenOnYAxis = true;
            }
            var result = betweenOnYAxis && betweenOnXAxis;

            if (useCache)
                _cache.Add(key, result);

            return result;
        }

        public RelativePosition GetRelativePosition(PointF point1, int size1, PointF point2, int size2)
        {
            var verticalPos = RelativePosition.UNDEFINED;
            var horizontalPos = RelativePosition.UNDEFINED;

            if (point1.Y + size1 < point2.Y)
                verticalPos = RelativePosition.ABOVE;
            else if (point1.Y > point2.Y)
                verticalPos = RelativePosition.BELOW;

            if (point1.X + size1 < point2.X)
                horizontalPos = RelativePosition.RIGHT;
            else if (point1.X > point2.X)
                horizontalPos = RelativePosition.LEFT;

            if (horizontalPos == RelativePosition.UNDEFINED && verticalPos == RelativePosition.UNDEFINED)
                return RelativePosition.SAME;

            // if no vertical position was found, it must be positioned horizontally
            if (verticalPos == RelativePosition.UNDEFINED)
                return horizontalPos;

            // at this point vertical position is not undefined. if there's no horizontal position return the vertical one
            if (horizontalPos == RelativePosition.UNDEFINED)
                return verticalPos;

            // if we get here then we must have both vertical and horizontal positions
            return horizontalPos | verticalPos;
        }

        private float CalculateSlope(float x1, float y1, float x2, float y2)
        {
            return (y2 - y1) / (x2 - x1);
        }

        public double GetAngleBetween(PointF currentPoint, PointF targetPoint, bool useCache = false)
        {
            return Math.Atan2(targetPoint.Y - currentPoint.Y, targetPoint.X - currentPoint.X);
        }
    }
}