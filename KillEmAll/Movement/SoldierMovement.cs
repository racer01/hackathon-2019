using System;
using Hackathon.Public;
using KillEmAll.Enums;
using KillEmAll.Utility;

namespace KillEmAll
{
    public class SoldierMovement : ISoldierMovement
    {
        private readonly MovementUtility _movementUtility;
        private const float MAXIUM_DISTANCE = 2f;
        private const float MINIUM_DISTANCE = 1f;
        private const float SHOOTING_DISTANCE = 5f;
        private const double ANGLE_LIMIT = Math.PI / 2;

        public SoldierMovement(MovementUtility movementUtility)
        {
            _movementUtility = movementUtility;
        }

        // usually used when we dont see the target object and use the pathfinder
        public SoldierCommand MoveToLocation(Soldier currentSoldier, PointF targetPoint, float virtualRadius, ref SoldierCommand command)
        {
            var targetAngle = _movementUtility.GetTargetAngle(currentSoldier, targetPoint);
            var virtualTarget = new Soldier(null, null, targetPoint, virtualRadius, 0, 0, 0, 0, 0);
            var tolerance = _movementUtility.GetAngleTolerance(currentSoldier, virtualTarget);

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

        // usually used when we see the target object
        public SoldierCommand MoveToObject(Soldier currentSoldier, GameObject targetObject, ref SoldierCommand command)
        {
            var targetAngle = _movementUtility.GetTargetAngle(currentSoldier, targetObject.Position);
            var tolerance = _movementUtility.GetAngleTolerance(currentSoldier, targetObject);

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


        public SoldierCommand TargetEnemy(Soldier currentSoldier, GameObject enemy, ref SoldierCommand command)
        {
            var targetAngle = _movementUtility.GetTargetAngle(currentSoldier, enemy.Position);
            var angleTolerance = _movementUtility.GetAngleTolerance(currentSoldier, enemy);
            var distance = _movementUtility.DistanceBetween(currentSoldier.Position, enemy.Position);
            var angleDiff = Math.Abs(currentSoldier.LookAtDirection - targetAngle);

            // ROTATION
            if (angleDiff > angleTolerance)
            {
                var targetDirection = _movementUtility.GetDirection(currentSoldier.LookAtDirection, targetAngle);
                command.RotateRight = targetDirection == Directions.Right;
                command.RotateLeft = targetDirection == Directions.Left;
            }

            // MOVE
            if (InGoodRange(distance) && angleDiff < angleTolerance)
            {
                // if we are in a good range with a good angle
                command.MoveBackward = false;
                command.MoveForward = false;
            }

            if (TooClose(distance))
            {
                if (angleDiff < ANGLE_LIMIT)
                {
                    command.MoveBackward = true;
                    command.MoveForward = false;
                }
                else
                {
                    command.MoveBackward = false;
                    command.MoveForward = true;
                }
            }

            if (TooFar(distance))
            {
                command.MoveForward = true;
            }

            // SHOOT?
            if (distance <= SHOOTING_DISTANCE && 
                angleDiff <= angleTolerance 
                && currentSoldier.TimeTillCanShoot <= 0)
                command.Shoot = true;
            else
                command.Shoot = false;

            return command;
        }

        private bool InGoodRange(double distanceFromEnemy)
        {
            return distanceFromEnemy < MAXIUM_DISTANCE && distanceFromEnemy > MINIUM_DISTANCE;
        }

        private bool TooClose(double distanceFromEnemy)
        {
            return distanceFromEnemy <= MINIUM_DISTANCE;
        }

        private bool TooFar(double distanceFromEnemy)
        {
            return distanceFromEnemy >= MAXIUM_DISTANCE;
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
