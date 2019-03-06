using Hackathon.Public;
using KillEmAll.Enums;

namespace KillEmAll.Utility.Interfaces
{
    public interface IPointUtility
    {
        /// <summary>
        /// Returns if the target point is in on a line between point 'start' and point 'end'.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        bool IsInBetween(PointF start, PointF end, PointF target);


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
