using Hackathon.Public;
using KillEmAll.Enums;

namespace KillEmAll.Utility.Interfaces
{
    public interface IPointUtility
    {
        /// <summary>
        /// Returns the distance between 'point1' and 'point2'.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        float DistanceBetween(PointF point1, PointF point2, bool useCache = false);

        /// <summary>
        /// Returns if the target point is in on a line between point 'start' and point 'end'.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        bool IsInBetween(PointF start, PointF end, PointF target, bool useCache = false);


        /// <summary>
        /// Returns the angle between 'currentPoint' and 'targetPoint'
        /// </summary>
        /// <param name="currentPoint"></param>
        /// <param name="targetPoint"></param>
        /// <returns></returns>
        double GetAngleBetween(PointF currentPoint, PointF targetPoint, bool useCache = false);

        /// <summary>
        /// Returns point2's relative position compared to point1.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="size1"></param>
        /// <param name="point2"></param>
        /// <param name="size2"></param>
        /// <returns></returns>
        RelativePosition GetRelativePosition(PointF point1, int size1, PointF point2, int size2);
    }
}
