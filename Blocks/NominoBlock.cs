using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Rendering;
using BASeTris.Rendering.RenderElements;

namespace BASeTris.Blocks
{
    
    
    public abstract class NominoBlock
    {
        public Action<TetrisBlockDrawParameters> BeforeDraw = null;
        public Nomino Owner { get; set; }
        public bool Visible { get; set; } = true;
        public virtual bool IsAnimated
        {
            get { return false; }
        }

        private int _Rotation = 0;

        public bool IgnoreRotation { get; set; } = false;


        //rotation can be set but if owned by a Nomino we use it's rotation.
        public virtual int Rotation
        {
            get
            {
                if (IgnoreRotation) return 0;
                if (Owner != null)
                {
                    NominoElement getbge = Owner.FindEntry(this);
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
        public virtual char GetCharacterRepresentation()
        {
            return '#';
        }
        public virtual void AnimateFrame()
        {
            //nothing by default. Well, for now anyway....
        }
    }
   
  
}