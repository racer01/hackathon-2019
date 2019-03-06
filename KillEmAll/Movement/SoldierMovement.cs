using System;
using Hackathon.Public;
using KillEmAll.momsspaghetti;

namespace KillEmAll
{
    public class SoldierMovement : ISoldierMovement
    {
        private const int MAGIC_NUMBER = 1;
        private const double ROTATION_LIMIT = Math.PI;
        private const float ROTATION_ACCURACY = 0.008f;
        private const float MINIMUM_DISTANCE = 2.5f;
        private const float DISTANCE_TO_STOP = 0.1f;
        private const float  DEFAULT_RADIUS = 0.05f;

        private MovementUtility _movementUtility;

        public SoldierMovement(MovementUtility movementUtility)
        {
            _movementUtility = movementUtility;
        }

        public SoldierCommand MoveToLocation(Soldier currentSoldier, PointF target, TargetType targetType, ref SoldierCommand command)
        {
            return MoveToLocation(currentSoldier, new Soldier(null, null, target, DEFAULT_RADIUS, 0, 0, 0, 0, 0), targetType, ref command);
        }

        public SoldierCommand MoveToLocation(Soldier currentSoldier, GameObject target, TargetType targetType, ref SoldierCommand command)
        {
            return MoveToLocation(currentSoldier, new Soldier(null, null, target.Position, target.Radius, 0, 0, 0, 0, 0), targetType, ref command);
        }

        // the function does too many things (rotate, forward-backward movement, shooting)
        // if I separated these 3 things, then all the math (distance, desired rotation etc.) would have to be re-done.
        public SoldierCommand MoveToLocation(Soldier currentSoldier, Soldier targetSoldier, TargetType targetType, ref SoldierCommand command)
        {
            var targetSpeed = targetSoldier.Speed;
            var targetDirection = targetSoldier.LookAtDirection;

            var targetAngle = _movementUtility.GetTargetAngle(currentSoldier, targetSoldier);
            //var targetAngle = Math.Atan2(currentSoldier.Position.Y - targetSoldier.Position.Y, currentSoldier.Position.X - targetSoldier.Position.X);


            var targetAngleNorm = NormaliseRadian(targetAngle);
            var normLookatDirection = NormaliseRadian(currentSoldier.LookAtDirection);
            //var rotationDiff = normalisedLookatDirection - normTargetAngle;
            //var amountToRotate = Math.Abs(rotationDiff) + Math.Abs(normTargetAngle - normalisedLookatDirection);
            /*
                IF TARGETANGLE > LOOKATDIRECTION => desiredAngle = TARGETANGLE - PI

                IF TARGETANGLE < LOOKATDIRECTION => desiredAngle = TARGETANGLE + PI
             */

            //var desiredAngle = Math.PI;
            //if (targetAngle > Math.PI)
            //    desiredAngle = targetAngle - Math.PI;
            //else if (targetAngle < Math.PI)
            //    desiredAngle = targetAngle + Math.PI;
            //else if (targetAngle == Math.PI)
            //    desiredAngle = targetAngle + Math.PI;

            var tolerance = _movementUtility.GetAngleTolerance(currentSoldier, targetSoldier);

            // how much we need to turn
            var rotationDiff = normLookatDirection - targetAngleNorm;

            if (rotationDiff >= tolerance)
                command.RotateRight = true;
            else if (rotationDiff <= -tolerance)
                command.RotateLeft = true;
            else
                Console.WriteLine();

            // if we need to turn more than 180, its faster to turn the other way
            //if (rotationDiff > Math.PI && (command.RotateRight || command.RotateLeft))
            //{
            //    command.RotateLeft = !command.RotateLeft;
            //    command.RotateRight = !command.RotateRight;
            //}

            //else if (rotationDiff >= tolerance || rotationDiff < -tolerance)
            //{
            //    command.RotateLeft = rotationDiff < 0 + ROTATION_ACCURACY;
            //    command.RotateRight = rotationDiff > 0 - ROTATION_ACCURACY;
            //}

            if (!command.RotateLeft && !command.RotateRight)
                command.MoveForward = true;

            //if (desiredAngle <= ROTATION_LIMIT + tolerance && desiredAngle >= ROTATION_LIMIT - tolerance)
            //{
            //    //TARGET IS BEHIND US
            //    command.MoveBackward = true;
            //    command.MoveForward = false;

            //    command.RotateRight = !command.RotateRight;
            //    command.RotateLeft = !command.RotateLeft;
            //    return command;

            //}

            //var distance = _movementUtility.DistanceBetween(currentSoldier.Position, targetSoldier.Position);

            //command.MoveForward = DISTANCE_TO_STOP < distance;
            return command;
        }

        private Tuple<float, float> PredictNextPosition(Soldier soldier)
        {
            var direction = soldier.LookAtDirection;
            var futureX = soldier.Position.X;
            var futureY = soldier.Position.Y;

            //Normalise radians to 0 - 2PI
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
                //PI->  2PI
              traveledX = (float)((3 / 2 * Math.PI - direction) / (Math.PI / 2)) * soldier.Speed;

                traveledY = soldier.Speed - Math.Abs(traveledX);

                if (traveledX < 0)
                    traveledY = -traveledY;
            }

            futureX += traveledX * 0.15f; //haaaaaaaaaaaaaaack
            futureY += traveledY * 0.15f;

            //DISABLE 
            //return Tuple.Create(soldier.Position.X, soldier.Position.Y);
            return Tuple.Create(futureX, futureY);
        }

        private double NormaliseRadian(double radian)
        {
            if (radian < 0)
                return Math.PI * 2 - Math.Abs(radian);
            else
                return radian;
        }
    }
}
