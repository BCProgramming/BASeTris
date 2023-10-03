using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates
{

    public interface ITransitableState
    {
        double AppearanceTransitionPercentage { get; set; }
        double AppearanceTransitionLength { get; }
    }

    public abstract class TransitState : GameState
    {
        public TransitState()
        {
        }
        public abstract GameState GetCompositeState();
        
    }
    /// <summary>
    /// TransitState effectively allows a state to be forced through a series of changes until a completion expression evaluates to true, at which point a completionexpression is run.
    /// I added this to provide for transitions outward for menus, but it could be used for a lot of stuff.
    /// One thing that is still needed is a helper in the MenuState class that provides a transitioning TransitState<MenuState> more easily- then state transitions within the menus can use that as an "out" transition and set to the new state.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TransitState<T> : TransitState,ICompositeState<T> where T:GameState
    {
        public bool DelegateGameProc { get; set; } = true;
        public bool DelegateGameKeys { get; set; } = false;
        T CompositeState;
        public Action InitExpression { get; set; }
        public Action IterateExpression { get; set; }

        public Func<bool> TerminateExpression { get; set; }

        public Action CompleteExpression { get; set; }

        private bool _flInitialized;
        public TransitState(T ParentState, Action pInitExpression, Action pIterateExpression, Func<bool> pTerminateExpression, Action pCompleteExpression)
        {
            _BG = ParentState.BG;
            CompositeState = ParentState;
            InitExpression = pInitExpression;
            IterateExpression = pIterateExpression;
            TerminateExpression = pTerminateExpression;
            CompleteExpression = pCompleteExpression;
        }

        public override void GameProc(IStateOwner pOwner)
        {
            if (!_flInitialized)
            {
                _flInitialized = true;
                InitExpression();
            }
            if (DelegateGameProc) CompositeState.GameProc(pOwner);
            IterateExpression();
            if (TerminateExpression())
            {
                CompleteExpression();
            }
        }
        public override GameState GetCompositeState()
        {
            return this.GetComposite();
        }
        
        public T GetComposite()
        {
            return CompositeState;
        }
        public override DisplayMode SupportedDisplayMode => GetCompositeState().SupportedDisplayMode;
        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            if (DelegateGameKeys) CompositeState.HandleGameKey(pOwner, g);
        }
    }
}
