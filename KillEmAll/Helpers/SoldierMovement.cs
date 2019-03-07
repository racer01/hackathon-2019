using System;
using Hackathon.Public;
using KillEmAll.Enums;
using KillEmAll.Helpers.Interfaces;

namespace KillEmAll
{
    public class SoldierMovement : ISoldierMovement
    {
        private readonly IMovementUtility _movementUtility;
        // EXPERIENCED CONSTANTS
        private const float MAXIUM_DISTANCE = 2.5f;
        private const float MINIUM_DISTANCE = 1f;
        private const float SHOOTING_DISTANCE = 20f;
        private const double ANGLE_LIMIT = Math.PI * 0.4;

        public SoldierMovement(IMovementUtility movementUtility)
        {
            _movementUtility = movementUtility;
        }

        // usually used when we dont see the target object and use the pathfinder
        public SoldierCommand MoveToLocation(Soldier currentSoldier, PointF targetPoint, float virtualRadius, ref SoldierCommand command)
        {
            var targetAngle = _movementUtility.GetTargetAngle(currentSoldier, targetPoint);
            var virtualTarget = new Soldier(null, null, targetPoint, virtualRadius, 0, 0, 0, 0, 0);
            var tolerance = _movementUtility.GetAngleTolerance(currentSoldier, virtualTarget);

            var angleDiff = Math.Abs(currentSoldier.LookAtDirection - targetAngle);
            if (angleDiff > tolerance)
            {
                var targetDirection = _movementUtility.GetDirection(currentSoldier.LookAtDirection, targetAngle);
                command.RotateRight = targetDirection == Directions.Right;
                command.RotateLeft = targetDirection == Directions.Left;
            }

            // SLOWER BUT MORE PRECISE MOVEMENT
            command.MoveForward = true;

            if ((command.RotateRight || command.RotateLeft) && currentSoldier.Speed == 2)
                command.MoveForward = false;

            return command;
        }

        // usually used when we see the target object
        public SoldierCommand MoveToObject(Soldier currentSoldier, GameObject targetObject, ref SoldierCommand command)
        {
            var targetAngle = _movementUtility.GetTargetAngle(currentSoldier, targetObject.Position);
            var tolerance = _movementUtility.GetAngleTolerance(currentSoldier, targetObject);

            var angleDiff = Math.Abs(currentSoldier.LookAtDirection - targetAngle);
            if (angleDiff > tolerance)
            {
                var targetDirection = _movementUtility.GetDirection(currentSoldier.LookAtDirection, targetAngle);
                command.RotateRight = targetDirection == Directions.Right;
                command.RotateLeft = targetDirection == Directions.Left;
            }
            var distance = _movementUtility.DistanceBetween(currentSoldier.Position, targetObject.Position);

            command.MoveForward = true;
            if (InGoodRange(distance) && angleDiff > ANGLE_LIMIT)
                command.MoveForward = false;

            return command;
        }

        // usually used when the target is an enemy
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

            // SHOOT
            if (distance <= SHOOTING_DISTANCE && 
                angleDiff <= angleTolerance 
                && currentSoldier.TimeTillCanShoot <= 0)
                command.Shoot = true;
            else
                command.Shoot = false;

            return command;
        }

        private static bool InGoodRange(double distanceFromEnemy)
        {
            return distanceFromEnemy < MAXIUM_DISTANCE && distanceFromEnemy > MINIUM_DISTANCE;
        }

        private static bool TooClose(double distanceFromEnemy)
        {
            return distanceFromEnemy <= MINIUM_DISTANCE;
        }

        private static bool TooFar(double distanceFromEnemy)
        {
            return distanceFromEnemy >= MAXIUM_DISTANCE;
        }
    }
}
