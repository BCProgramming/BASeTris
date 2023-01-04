using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{
    /// <summary>
    /// Attribute specified on class definitions of BlockGroupChooser subclasses which should be available "Chooser" options for
    /// that particular game style.
    /// The game handler specified should implement the IGameHandlerChooserInitializer interface.
    /// </summary>
    public class ChooserCompatibilityAttribute:Attribute
    {
        public Type HandlerType {get;set;}
        public ChooserCompatibilityAttribute(Type GameHandlerType)
        {
            if (!GameHandlerType.GetInterfaces().Any((i) => i == typeof(IGameHandlerChooserInitializer)))
                throw new ArgumentException("Specified Handler Type " + GameHandlerType.Name + " does not implement IGameHandlerChooserInitializer");

          
            HandlerType = GameHandlerType;
        }
    }
   
}
