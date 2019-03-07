using Hackathon.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillEmAll.Helpers
{
    public class Path
    {
        public List<PointF> Route { get; set; }
        public Type TargetType { get; set; }
    }

    public class SoldierPathMapping
    {
        private Dictionary<string, Path> _pathMapping = new Dictionary<string, Path>();

        public void UpdatePath(Soldier soldier, List<PointF> path)
        {
            if (_pathMapping.ContainsKey(soldier.Id))
                _pathMapping[soldier.Id].Route = path;
        }

        public void StoreNewPath(Soldier soldier, Path path)
        {
            if (!_pathMapping.ContainsKey(soldier.Id))
                _pathMapping.Add(soldier.Id, path);
            else
                _pathMapping[soldier.Id] = path;
        }

        public Path GetPath(Soldier soldier)
        {
            if (!_pathMapping.ContainsKey(soldier.Id))
                return new Path();
            return _pathMapping[soldier.Id];
        }

        public bool PathExists(Soldier soldier)
        {
            return _pathMapping.ContainsKey(soldier.Id) && _pathMapping[soldier.Id].Route?.Count != 0;
        }

        public void RemovePath(Soldier soldier)
        {
            if (_pathMapping.ContainsKey(soldier.Id))
                _pathMapping.Remove(soldier.Id);
        }

        public void ClearAll()
        {
            _pathMapping.Clear();
        }
    }
}
