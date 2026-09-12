using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BASeCamp.Elementizer;
using BASeTris.Rendering;
using BASeTris.Rendering.RenderElements;

namespace BASeTris.Blocks
{
    
    
    public abstract class NominoBlock : IXmlPersistable
    {
        //Draw Modifiers are a concept to help make it easier to create special additives that draw in different ways.
        //Right now we have a weird hard-coded percentage that changes drawing, ideally we'd have a 'shrink' modifier that gets added to a block instead for that purpose.
        //This was originally going to try to use the whole rendering provider mechanic but it's a bit problematic to use that way.
        //note: decided against this for now- it would require a brand new bitmap every single frame, one for each modifier- which seems annoying. will stick with BlockDrawParameters instead, and they can be handled in the RenderingHandler implementation(s)
        /*public Dictionary<Type,List<TetrisBlockRenderingModifier>> DrawModifiers = new Dictionary<Type,List<TetrisBlockRenderingModifier>>();

        public void AddOrMutateModifier<T>(Func<T> New, Action<T> Modify) where T:TetrisBlockRenderingModifier
        {
            if (!DrawModifiers.ContainsKey(typeof(T)))
            {
                //if not present in the dictionary, create a new list, add the result of the "new" function, add the list to the dictionary, and return.
                var MadeList = new List<TetrisBlockRenderingModifier>() { New() };
                DrawModifiers.Add(typeof(T), MadeList);
            }
            else
            {
                //modify the existing reference.

                var checklist = DrawModifiers[typeof(T)];
                foreach (var modifyitem in checklist)
                    Modify(modifyitem as T); //forced downcast, but since it is in the dictionary indexed by the type it should be that type of list and element...
            }
        }*/



        public Action<TetrisBlockDrawParameters> BeforeDraw = null;
        public Nomino? Owner { get; set; }
        public bool Visible { get; set; } = true;
        public virtual bool IsAnimated
        {
            get { return false; }
        }

        private int _Rotation = 0;

        public bool IgnoreRotation { get; set; } = false;

        public virtual XElement GetXmlData(string pNodeName, object PersistenceData)
        {
            XElement buildresult = new XElement(pNodeName, 
                new XAttribute("Visible",Visible),
                new XAttribute("Rotation",_Rotation),
                new XAttribute("IgnoreRotation",IgnoreRotation)
            );
            return buildresult;
        }
        public NominoBlock(XElement src, Object pContext)
        {
            Visible = src.GetAttributeBool("Visible", true);
            Rotation = src.GetAttributeInt("Rotation", 0);
            IgnoreRotation = src.GetAttributeBool("IgnoreRotation", IgnoreRotation);
            Owner = null;

        }
        public NominoBlock()
        {
        }
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