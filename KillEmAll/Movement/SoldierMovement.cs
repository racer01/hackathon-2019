using System;
using Hackathon.Public;
using KillEmAll.Enums;
using KillEmAll.Utility;

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
            var targetAngle = _movementUtility.GetTargetAngle(currentSoldier, targetSoldier);
            var tolerance = _movementUtility.GetAngleTolerance(currentSoldier, targetSoldier);

            if (Math.Abs(currentSoldier.LookAtDirection - targetAngle) > tolerance)
            {
                var targetDirection = _movementUtility.GetDirection(currentSoldier.LookAtDirection, targetAngle);
                command.RotateRight = targetDirection == Directions.Right;
                command.RotateLeft = targetDirection == Directions.Left;
            }

            if (!command.RotateLeft && !command.RotateRight)
                command.MoveForward = true;

            return command;
        }


        //////////////////////// big words under this line ///////////////////////////////////
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
            return radian;
        }
    }
}
