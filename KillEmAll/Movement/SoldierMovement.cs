using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hackathon.Public;
using KillEmAll.Extensions;

namespace KillEmAll
{
    public class SoldierMovement : ISoldierMovement
    {
        private const int MAGIC_NUMBER = 1;
        private const double ROTATION_LIMIT = Math.PI - MAGIC_NUMBER;
        private const float ROTATION_ACCURACY = 0.008f;

        //SPAGETTI WARNING
        public SoldierCommandExtension MoveToLocation(Soldier currentSoldier, Soldier targetSoldier, ref SoldierCommandExtension command)
        {
            var targetSpeed = targetSoldier.Speed;
            var targetDirection = targetSoldier.LookAtDirection;

            // check if wall is blocking soldier from movement
            var futurePosition = PredictNextPosition(targetSoldier);


            var desiredAngle = GetDesiredAngle(currentSoldier.Position, new PointF(futurePosition.Item1, futurePosition.Item2));
            //var desiredAngle = GetDesiredAngle(currentSoldier.Position, targetSoldier.Position);

            var normlookatDirection = NormaliseRadian(currentSoldier.LookAtDirection);
            var normDesiredAngle = NormaliseRadian((float)desiredAngle);

            var rotationDiff = normlookatDirection - normDesiredAngle;

            //testing shooting accuracy
            if (Math.Abs(rotationDiff) < ROTATION_ACCURACY)
                command.Shoot = true;
            
            if (rotationDiff > ROTATION_ACCURACY)
                command.RotateRight = true;
            else if (rotationDiff < -ROTATION_ACCURACY)
                command.RotateLeft = true;

            // should be faster than math.pow()
            var distance = (currentSoldier.Position.X - targetSoldier.Position.X) * (currentSoldier.Position.X - targetSoldier.Position.X) + (currentSoldier.Position.Y - targetSoldier.Position.Y) * (currentSoldier.Position.Y - targetSoldier.Position.Y);

            command.MoveForward = true;
            if (Math.Abs(rotationDiff) > ROTATION_LIMIT || distance < 3)
            {
                command.MoveForward = false;
                command.MoveBackward = true;

                command.RotateLeft = !command.RotateLeft;
                command.RotateRight = !command.RotateRight;
            }

            

            
            //if (distance < 0.1 && distance > 0)
            //{
            //    command.MoveForward = false;
            //}
            return command;
        }

        public SoldierCommandExtension TurnTowards(Soldier currentSoldier, Soldier targetSoldier, ref SoldierCommandExtension command)
        {
            var desiredAngle = GetDesiredAngle(currentSoldier.Position, targetSoldier.Position);

            var rotationDiff = currentSoldier.LookAtDirection - desiredAngle;

            if (rotationDiff > 0)
                command.RotateRight = true;
            else if (rotationDiff < 0)
                command.RotateLeft = true;

            return command;
        }

        private double GetDesiredAngle(PointF currentPoint, PointF targetPoint)
        {
            return Math.Atan2(targetPoint.Y - currentPoint.Y, targetPoint.X - currentPoint.X);
        }

        private Tuple<float,float> PredictNextPosition(Soldier soldier)
        {
            var direction = soldier.LookAtDirection;
            var futureX = soldier.Position.X;
            var futureY = soldier.Position.Y;

            // Normalise radians to 0 - 2PI
            if (direction < 0)
                direction = (float)Math.PI * 2 - Math.Abs(direction);

            float traveledX;
            float traveledY;
            if (direction <= Math.PI)
            {
                //SHOULD WORK UP TO PI
                traveledX = (float)((Math.PI / 2 - direction) / (Math.PI / 2)) * soldier.Speed;
                traveledY = soldier.Speed - Math.Abs(traveledX);

                if (traveledX < 0)
                    traveledY = -traveledY;
            }
            else
            {
                // PI ->  2PI
                traveledX = (float)((3 / 2 * Math.PI - direction) / (Math.PI / 2)) * soldier.Speed;

                traveledY = soldier.Speed - Math.Abs(traveledX);

                if (traveledX < 0)
                    traveledY = -traveledY;
            }
            
            futureX += traveledX * 0.2f; //haaaaaaaaaaaaaaack
            futureY += traveledY * 0.2f;

            return Tuple.Create(futureX, futureY);
        }

        private float NormaliseRadian(float radian)
        {
            if (radian < 0)
                return (float)Math.PI * 2 - Math.Abs(radian);
            else
                return radian;
        }
    }
}
