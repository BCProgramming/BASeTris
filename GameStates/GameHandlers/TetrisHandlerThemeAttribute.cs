using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.GameHandlers
{
    //Attribute used to indicate the desired handler(s) for which a particular Theme is intended to work with.
    public class HandlerThemeAttribute : Attribute 
    {
        public Type[] HandlerType { get; set; }
        public HandlerThemeAttribute(String pName,params Type[] typespecifiers)
        {
            if (typespecifiers.Any((w) => (w.GetInterface("IGameCustomizationHandler")==null)))
                throw new ArgumentException("All types provided to TetrisHandlerThemeAttribute must be a Game Handler");
            HandlerType = typespecifiers;
        }
    }
}
