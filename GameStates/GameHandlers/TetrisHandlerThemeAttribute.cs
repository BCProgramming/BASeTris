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
        [Flags]
        public enum HandlerThemeFlags
        {
            ThemeFlags_None = 0,
            ThemeFlags_NonBrowsable = 1,
        }
        public HandlerThemeFlags Flags { get; set; } = HandlerThemeFlags.ThemeFlags_None;
        public Type[] HandlerType { get; set; }
        public HandlerThemeAttribute(String pName,params Type[] typespecifiers):this(pName, HandlerThemeFlags.ThemeFlags_None, typespecifiers)
        {
        }
        public HandlerThemeAttribute(String pName, HandlerThemeFlags pFlags,params Type[] typespecifiers)
        {
            Flags = pFlags;
            if (typespecifiers.Any((w) => (w.GetInterface("IBlockGameCustomizationHandler") == null)))
                throw new ArgumentException("All types provided to TetrisHandlerThemeAttribute must be a Game Handler");
            HandlerType = typespecifiers;
        }
    }
}
