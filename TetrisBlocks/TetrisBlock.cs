using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Rendering.RenderElements;

namespace BASeTris.TetrisBlocks
{
    //TODO: All "drawable" class instances should have their drawing implementation moved to a separate helper class or series of helper classes
    //which function as an "adapter" that will draw to certain outputs.
    //For example, as it stands now, we'd create a class to draw things via System.Drawing/GDI+. Once we have the interface-based approach to select the
    //"Drawing" implementation we can create additional implementations for drawing to other output types (openTK for example)).

    public abstract class TetrisBlock
    {
        public Action<TetrisBlockDrawParameters> BeforeDraw = null;
        public BlockGroup Owner { get; set; }

        public virtual bool IsAnimated
        {
            get { return false; }
        }

        private int _GroupID = 0;

        public int GroupID
        {
            get { return _GroupID; }
            set { _GroupID = value; }
        }

        private int _Rotation = 0;

       


        //rotation can be set but if owned by a BlockGroup we use it's rotation.
        public virtual int Rotation
        {
            get
            {
                if (Owner != null)
                {
                    BlockGroupEntry getbge = Owner.FindEntry(this);
                    if (getbge != null) return getbge.RotationModulo;
                }

                return _Rotation;
            }

            set { _Rotation = value; }
        }

        internal void InvokeBeforeDraw(TetrisBlockDrawParameters parameters)
        {
            BeforeDraw?.Invoke(parameters);
        }
        //[Obsolete("Use Rendering Providers.")]
        /*public virtual void DrawBlock(TetrisBlockDrawParameters parameters)
        {
            
            InvokeBeforeDraw(parameters);            
        }*/

        public virtual void AnimateFrame()
        {
            //nothing by default. Well, for now anyway....
        }
    }
   
  
}