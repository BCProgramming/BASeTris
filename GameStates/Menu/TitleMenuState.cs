using BASeTris.BackgroundDrawers;
using BASeTris.GameStates.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.Menu
{
    public class TitleMenuState : GenericMenuState
    {
        public TitleMenuState(IBackground pBG, IStateOwner pOwner) : base(pBG, pOwner, new TitleMenuPopulator())
        {
            this.StateHeader = "BASeTris";
        }
    }
}
