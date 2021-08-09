using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.GameHandlers.HandlerOptions
{
    public class HandlerOptionsAttribute :Attribute
    {
        private Type OptionType = null;
        public HandlerOptionsAttribute(Type pOptionType)
        {
            if (!typeof(GameOptions).IsAssignableFrom(pOptionType))
                throw new ArgumentException("Invalid Option type:" + pOptionType.Name + "; Must derive from GameOptions");
            OptionType = pOptionType;
        }
    }
    public abstract class HandlerOptions<T> where T:IGameCustomizationHandler
    {
        //handler options for CustomizationHandler type T.
        
    }
    public abstract class HandlerOptionHandler<T,X>
        where T : HandlerOptions<X>
        where X:IGameCustomizationHandler
        
    {
        //handler options allow a class to be defined that 
        //1. Can populate an Options menu with handler options from a HandlerOptions<X> implementation
        //2. Applies settings changed within it's specialized Options menu to the HandlerOptions<X> instance
        
        



    }
}
