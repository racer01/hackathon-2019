using System;
using Hackathon.Public;
using KillEmAll.Enums;

namespace KillEmAll.momsspaghetti
{
    public class MovementUtility
    {
        public Directions GetDirection(double currentAngle, double targetAngle)
        {
            currentAngle = currentAngle < 0 ? 2 * Math.PI + currentAngle : currentAngle;
            targetAngle = targetAngle < 0 ? 2 * Math.PI + targetAngle : targetAngle;

            if (targetAngle > currentAngle)
            {
                return (targetAngle - currentAngle < Math.PI) ? Directions.Left : Directions.Right;
            }
            return (currentAngle - targetAngle > Math.PI) ? Directions.Left : Directions.Right;

        }

        public double GetTargetAngle(GameObject me, GameObject notMe)
        {
            if (me.Position.X == notMe.Position.X)
            {
                if (me.Position.Y <= notMe.Position.Y)
                {
                    return Math.PI / 2;
                }
                return -Math.PI / 2;
            }
            if (me.Position.X < notMe.Position.X)
            {
                var distY = notMe.Position.Y - me.Position.Y;
                var distX = notMe.Position.X - me.Position.X;
                var tanAlpha = distY / distX;
                var alpha = Math.Atan(tanAlpha);
                return alpha;
            }
            if (me.Position.X > notMe.Position.X)
            {
                var distY = notMe.Position.Y - me.Position.Y;
                var distX = notMe.Position.X - me.Position.X;
                var tanAlpha = distY / distX;
                var alpha = Math.Atan(tanAlpha);
                alpha -= Math.PI;
                return alpha;
            }
            return 0;
        }

        public double GetAngleTolerance(GameObject me, GameObject notMe)
        {
            var dist = DistanceBetween(me.Position, notMe.Position);
            return Math.Atan(notMe.Radius / dist);
        }

        public double DistanceBetween(PointF point1, PointF point2)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(point1.X - point2.X), 2) + Math.Pow(Math.Abs(point1.Y - point2.Y), 2));
        }
    }
}