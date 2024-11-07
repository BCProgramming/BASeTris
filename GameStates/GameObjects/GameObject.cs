using BASeTris.BackgroundDrawers;
using BASeTris.Rendering.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.GameObjects
{
    //GameObjects are generic items that are designed to be rendered and GameProc'd and can exist on any state.
    public abstract class GameObject
    {

        public abstract void GameProc(IStateOwner pOwner);

        //Draw routine, of course, is in a rendering provider, and separate from this.


    }
    public interface ILocatable //getting flashbacks to BASeBlock at this point...
    {
        public BCPoint Location { get; set; }
    }

    
    public class ImageObject : GameObject, ILocatable
    {
        //Image Source
        public BCImage Image { get; set; }
        //Clip rect to use within the source image.
        public BCRect Clip { get; set; }

        public double Rotation { get; set; }

        public double RotationFalloffFactor { get; set; } = 0.9d;


        public BCPoint Location { get; set; }

        public BCPoint Velocity { get; set; }

        public IVectorMutator<BCPoint> VelocityMutator { get; set; }
        

        public override void GameProc(IStateOwner pOwner)
        {
            
        }
    }
    //image Object. This is pretty much a particle that has a GameProc
}
