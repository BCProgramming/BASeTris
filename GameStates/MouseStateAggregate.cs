using BASeTris.Rendering.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates
{
    //aggregate class that encapsulates mouse handling and similar information.
    
    public class MouseStateAggregate
    {
        public bool MouseActive { get { return (DateTime.Now - LastMouseMovement).Seconds > 5; } }
        public BCPoint LastMouseMovementPosition { get; set; }

        public DateTime LastMouseMovement { get; set; }

    }
}
