using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.GameHandlers
{
    //can be used to indicate the Handler can have additional options
    public class GameHandlerOptionsAttribute:Attribute
    {
        private Type _HandlerOptionsType = null;
        private Type _HandlerOptionsStateType = null;
        public Type HandlerOptionsStateType { get { return _HandlerOptionsStateType; } }
        public Type HandlerOptionsType { get { return _HandlerOptionsType; } }
        public GameHandlerOptionsAttribute(Type pOptionsType, Type pStateType)
        {
            _HandlerOptionsType = pOptionsType;
            _HandlerOptionsStateType = pStateType;
        }
    }

}
