using Hackathon.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillEmAll.Extensions
{
    public class SoldierCommandExtension : SoldierCommand
    {
        public int RotateRightIncentive { get; set; }
        public int RotateLeftIncentive { get; set; }

        public int MoveForwardIncentive { get; set; }
        public int MoveBackwardIncentive { get; set; }

        public int ShootIncentive { get; set; }
    }
}
