using Hackathon.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillEmAll.Helpers
{
    public class SoldierPathMapping
    {
        private Dictionary<string, List<PointF>> _pathMapping = new Dictionary<string, List<PointF>>();

        public void UpdatePath(Soldier soldier, List<PointF> path)
        {
            if (_pathMapping.ContainsKey(soldier.Id))
                _pathMapping[soldier.Id] = path;
        }

        public void StoreNewPath(Soldier soldier, List<PointF> path)
        {
            if (!_pathMapping.ContainsKey(soldier.Id))
                _pathMapping.Add(soldier.Id, path);
        }

        public List<PointF> GetPath(Soldier soldier)
        {
            if (!_pathMapping.ContainsKey(soldier.Id))
                return new List<PointF>();

            return _pathMapping[soldier.Id];
        }

        public bool PathExists(Soldier soldier)
        {
            return _pathMapping.ContainsKey(soldier.Id) && _pathMapping[soldier.Id]?.Count != 0;
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
