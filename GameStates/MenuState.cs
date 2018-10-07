using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates
{
    public class MenuState : GameState
    {
        public override void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //unused
        }

        public override void GameProc(IStateOwner pOwner)
        {
            
        }

        public override void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            
        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            
        }

        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            
        }

        public override DisplayMode SupportedDisplayMode { get{ return DisplayMode.Full; } }
    }
}
