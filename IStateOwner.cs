using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris
{
    public interface IStateOwner
    {
        GameState CurrentState { get; set; }
        void EnqueueAction(Action pAction);
        
        
    }
}
