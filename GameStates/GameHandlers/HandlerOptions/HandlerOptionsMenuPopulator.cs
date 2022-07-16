using BASeTris.GameStates.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.GameHandlers.HandlerOptions
{
    public abstract class HandlerOptionsMenuPopulator : IMenuPopulator
    {
        protected GameOptions _Options;
        public abstract void SetOptions(GameState PreviousState,GameOptions options);
        public abstract void PopulateMenu(GenericMenuState Target, IStateOwner pOwner);
    }
    public abstract class HandlerOptionsMenuPopulator<T> : HandlerOptionsMenuPopulator where T:GameOptions
    {
        
        public override void SetOptions(GameState PreviousState, GameOptions options)
        {
            _Options = options;
            SetOptions(PreviousState, (T)options);
        }
        public abstract void SetOptions(GameState PreviousState,T options);
        public override void PopulateMenu(GenericMenuState Target, IStateOwner pOwner)
        {
            throw new NotImplementedException();
        }
    }

    public class HandlerOptionsMenuAttribute : Attribute
    {
        private Type _HandlerPopulator;
        public Type HandlerPopulator {  get { return _HandlerPopulator; } set { _HandlerPopulator = value; } }
        public HandlerOptionsMenuAttribute(Type handlerType)
        {
            if (!typeof(HandlerOptionsMenuPopulator).IsAssignableFrom(handlerType))
                throw new ArgumentException("handlerType must be a HandlerOptionsMenuPopulator");
            _HandlerPopulator = handlerType;
        }
    }


    public class StandardTetrisOptionsHandler : HandlerOptionsMenuPopulator<TetrisGameOptions>
    {
        
        public override void SetOptions(GameState PreviousState, TetrisGameOptions options)
        {
            _Options = options;
            
        }
    }


    public class DrMarioOptionsHandler : HandlerOptionsMenuPopulator<DrMarioGameOptions>
    {
        public override void SetOptions(GameState PreviousState, DrMarioGameOptions options)
        {
            _Options = options;
            
        }
    }

    public class Tetris2OptionsHandler : HandlerOptionsMenuPopulator<Tetris2GameOptions>
    {
        public override void SetOptions(GameState PreviousState, Tetris2GameOptions options)
        {
            _Options = options;

        }
    }
}
