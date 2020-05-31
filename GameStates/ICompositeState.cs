using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates
{
    //interface for a "composite" state, one which runs "on top" of a previous state, allowing that original state to be used in some manner.
    public interface ICompositeState<CompositeState> where CompositeState : GameState
    {
        CompositeState GetComposite();

    }
}
