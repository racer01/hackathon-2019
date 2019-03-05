using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hackathon.Public;
using KillEmAll.Utility.Interfaces;

namespace KillEmAll
{
    public enum TargetType
    {
        ENEMY,
        OBJECT
    }

    public class SoldierMovement : ISoldierMovement
    {
        private const int MAGIC_NUMBER = 1;
        private const double ROTATION_LIMIT = Math.PI / 4;
        private const float ROTATION_ACCURACY = 0.008f;
        private const float MINIMUM_DISTANCE = 2.5f;
        private const float DISTANCE_MARGIN = 0.1f;

        private IPointUtility _pointUtility;

        public SoldierMovement(IPointUtility pointUtility)
        {
            _pointUtility = pointUtility;
        }

        public SoldierCommand MoveToLocation(Soldier currentSoldier, PointF target, TargetType targetType, ref SoldierCommand command)
        {
            return MoveToLocation(currentSoldier, new Soldier(null, null, target, 0, 0, 0, 0, 0, 0), targetType, ref command);
        }

        // the function does too many things (rotate, forward-backward movement, shooting)
        // if I separated these 3 things, then all the math (distance, desired rotation etc.) would have to be re-done.
        public SoldierCommand MoveToLocation(Soldier currentSoldier, Soldier targetSoldier, TargetType targetType, ref SoldierCommand command)
        {
            var targetSpeed = targetSoldier.Speed;
            var targetDirection = targetSoldier.LookAtDirection;

            var futurePosition = PredictNextPosition(targetSoldier);

            var desiredAngle = _pointUtility.GetAngleBetween(currentSoldier.Position, new PointF(futurePosition.Item1, futurePosition.Item2));
            //var desiredAngle = GetDesiredAngle(currentSoldier.Position, targetSoldier.Position);

            var normlookatDirection = NormaliseRadian(currentSoldier.LookAtDirection);
            var normDesiredAngle = NormaliseRadian((float)desiredAngle);

            var rotationDiff = normlookatDirection - normDesiredAngle;

            if (targetType == TargetType.ENEMY && Math.Abs(rotationDiff) < ROTATION_ACCURACY && currentSoldier.TimeTillCanShoot == 0)
            {
                command.Shoot = true;
                return command;
            }

            var distance = Math.Sqrt(currentSoldier.Position.X - targetSoldier.Position.X) * (currentSoldier.Position.X - targetSoldier.Position.X) + (currentSoldier.Position.Y - targetSoldier.Position.Y) * (currentSoldier.Position.Y - targetSoldier.Position.Y);


            command.MoveForward = targetType == TargetType.ENEMY;

            if (rotationDiff > ROTATION_ACCURACY)
                command.RotateRight = true;
            else if (rotationDiff < -ROTATION_ACCURACY)
                command.RotateLeft = true;
            else 
                command.MoveForward = true;
                

            if (targetType == TargetType.OBJECT)
                return command;

            if (distance <= MINIMUM_DISTANCE * (1 + DISTANCE_MARGIN) && distance >= MINIMUM_DISTANCE * (1 - DISTANCE_MARGIN))
            {
                command.MoveForward = false;
                command.MoveBackward = false;
                return command;
            }

            if (distance <= MINIMUM_DISTANCE && Math.Abs(rotationDiff) > ROTATION_LIMIT)
            {
                command.MoveForward = false;
                command.MoveBackward = true;

                command.RotateLeft = !command.RotateLeft;
                command.RotateRight = !command.RotateRight;
            }
            return command;
        }

        public SoldierCommand TurnTowards(Soldier currentSoldier, Soldier targetSoldier, ref SoldierCommand command)
        {
            var desiredAngle = _pointUtility.GetAngleBetween(currentSoldier.Position, targetSoldier.Position);

            var rotationDiff = currentSoldier.LookAtDirection - desiredAngle;

            if (rotationDiff > 0)
                command.RotateRight = true;
            else if (rotationDiff < 0)
                command.RotateLeft = true;

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

        private float NormaliseRadian(float radian)
        {
            if (radian < 0)
                return (float)Math.PI * 2 - Math.Abs(radian);
            else
                return radian;
        }
    }
}
