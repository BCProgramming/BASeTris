using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates
{
    public class StandardGameOptions
    {
        //options for the standard game.
        public bool AllowHold = true;
        public int NextQueueSize = 6;
        public bool MoveResetsSetTimer = true;
        public bool RotateResetsSetTimer = true;
        public bool DrawGhostDrop = true;
        
    }
}
