using BASeTris.Rendering.Adapters;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.GameObjects
{

    public class ImageDebrisObject : GameObject,ILocatable
    {
        public BCPoint Location { get; set; }

        public BCPoint Velocity { get; set; }
        public BCImage DebrisImage { get; set; }
        public BCRect DebrisClip { get; set; }

        public override void GameProc(IStateOwner pOwner)
        {
            //throw new NotImplementedException();
        }
    }
}
