using System;
using Hackathon.Public;

namespace KillEmAll.momsspaghetti
{
    public class MovementUtility
    {
        public string GetDirection(double currentAngle, double targetAngle)
        {
            currentAngle = currentAngle < 0 ? 2 * Math.PI + currentAngle : currentAngle;
            targetAngle = targetAngle < 0 ? 2 * Math.PI + targetAngle : targetAngle;

            if (targetAngle > currentAngle)
            {
                return (targetAngle - currentAngle < Math.PI) ? "left" : "right";
            }
            return (currentAngle - targetAngle > Math.PI) ? "left" : "right";

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
            var dist = Math.Sqrt(Math.Pow(Math.Abs(me.Position.X - notMe.Position.X), 2) +
                                 Math.Pow(Math.Abs(me.Position.Y - notMe.Position.Y), 2));
            return Math.Atan(notMe.Radius / dist);
        }
    }
}