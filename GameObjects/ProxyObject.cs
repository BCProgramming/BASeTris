using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris
{
    /// <summary>
    /// A Proxy GameObject can be used for those facilities that are... lacking.
    /// For example, there is no way within a PaddleBehaviour to remove itself, since all calls to the behaviour are done within enumerations, and other concerns.
    /// The proxy object can be used to create a "proxy" game object and redirect the overridden methods to given routines.
    /// </summary>

    public class ProxyObject : GameObject
    {
        public delegate bool ProxyPerformFrame(ProxyObject sourceobject, IStateOwner gamestate);
        public delegate void ProxyDraw(Graphics g);

        private object _Tag = null;
        public object Tag { get { return _Tag; } set { _Tag = value; } }
        protected ProxyPerformFrame funcperformframe;
        protected ProxyDraw funcdraw;

        public ProxyObject(ProxyPerformFrame performframefunc, ProxyDraw drawfunc)
        {
            funcperformframe = performframefunc;
            funcdraw = drawfunc;


        }


        public override bool PerformFrame(IStateOwner gamestate)
        {
            if (funcperformframe != null)
                return funcperformframe(this, gamestate);

            return false;
        }

        public override void Draw(Graphics g)
        {
            if (funcdraw != null)
                funcdraw(g);
        }
    }

}
