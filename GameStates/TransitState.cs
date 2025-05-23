﻿using BASeTris.BackgroundDrawers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.BASeCamp;
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
    public class TransitState<T> : TransitState, ICompositeState<T> where T : GameState
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


    public abstract class TransitionState : GameState
    {
        [Flags]
        public enum DelegateProcConstants
        {
            Delegate_None = 0,
            Delegate_Previous = 1,
            Delegate_Next = 2
        }
        [Flags]
        public enum SnapshotConstants
        {
            Snapshot_None = 0,
            Snapshot_Previous = 1,
            Snapshot_Next = 2,
            Snapshot_Both = 3
        }
        public SnapshotConstants SnapshotSettings = SnapshotConstants.Snapshot_None;
        public DelegateProcConstants GameProcDelegationMode = DelegateProcConstants.Delegate_None;
        public DelegateProcConstants GameKeyDelegationMode = DelegateProcConstants.Delegate_None;

        public override DisplayMode SupportedDisplayMode {
            get {
                return DisplayMode.Full;
                
            } }
        public DateTime? StartTime { get; set; }
        public TimeSpan TransitWait { get; set; } = TimeSpan.Zero; //time to wait before starting transition.
        public TimeSpan TransitLength { get; set; }
        public GameState PreviousState { get; set; }
        public GameState NextState { get; set; }

        public double TransitionPercentage { get {

                if (StartTime == null) return 0;
                if (DateTime.Now - StartTime < TransitWait) return 0; //hasn't started yet.
                return (double)((DateTime.Now - StartTime+TransitWait).Value.Ticks) / (double)(TransitLength.Ticks);

            } }
        public TransitionState(GameState pPrevState, GameState pNextState, TimeSpan pTransitionLength)
        {
            PreviousState = pPrevState;
            NextState = pNextState;
            TransitLength = pTransitionLength;
        }

        public override void GameProc(IStateOwner pOwner)
        {
            if (GameProcDelegationMode.HasFlag(DelegateProcConstants.Delegate_Previous) || GameProcDelegationMode.HasFlag(DelegateProcConstants.Delegate_Next))
            {
                var PrevStateTask = Task.Run(() =>
                {

                    if (GameProcDelegationMode.HasFlag(DelegateProcConstants.Delegate_Previous))
                    {
                        PreviousState.GameProc(pOwner);
                    }
                });
                var NextStateTask = Task.Run(() =>
                {
                    if (GameProcDelegationMode.HasFlag(DelegateProcConstants.Delegate_Next))
                    {
                        NextState.GameProc(pOwner);
                    }
                });
                Task.WaitAll(PrevStateTask, NextStateTask);
            }
            if (PreviousState is ITransitableState its)
            {
                its.AppearanceTransitionPercentage = 1-this.TransitionPercentage;
            }
            if (DateTime.Now - StartTime > TransitLength)
            {
                Debug.Print("Moving to next state:" + NextState.GetType().Name);
                pOwner.CurrentState = NextState;
            }
        }
        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            if (GameKeyDelegationMode.HasFlag(DelegateProcConstants.Delegate_Previous))
                PreviousState.HandleGameKey(pOwner, g);

            if (GameKeyDelegationMode.HasFlag(DelegateProcConstants.Delegate_Next))
                NextState.HandleGameKey(pOwner, g);
        }
        public static TimeSpan StandardTransitionLength = new TimeSpan(0, 0, 0, 0, 750);
        private static Type[] _AllTransitionTypes = null;
        public static Type[] AllTransitionStates
        {
            get
            {
                if (_AllTransitionTypes == null)
                {
                    _AllTransitionTypes = Program.DITypes[typeof(TransitionState)].ManagedTypes.ToArray();
                }
                return _AllTransitionTypes;
            }
        }
        
        public static TransitionState GetRandomTransitionState(GameState pPrevState, GameState pNextState, TimeSpan pTransitionLength)
        {
            Type useType = TetrisGame.Choose(AllTransitionStates, TetrisGame.StatelessRandomizer);
            TransitionState result = (TransitionState)Activator.CreateInstance(useType, pPrevState, pNextState, pTransitionLength);
            result.GameProcDelegationMode = TransitionState.DelegateProcConstants.Delegate_None;
            result.SnapshotSettings = TransitionState.SnapshotConstants.Snapshot_Both;
            return result;


        }

        public static TransitionState CreateTransitionChain(GameState[] States, TimeSpan[] Lengths, Func<GameState, GameState, TimeSpan, TransitionState> TransitionBuilder = null)
        {
            if (TransitionBuilder == null) TransitionBuilder = (p, n, ts) =>
            {
                Type useType = TetrisGame.Choose(AllTransitionStates, TetrisGame.StatelessRandomizer);
                TransitionState result = (TransitionState)Activator.CreateInstance(useType, p, n, ts);
                return result;
            };
            //both should be non-null...
            if (States == null) throw new ArgumentNullException("States");
            if (Lengths == null) throw new ArgumentNullException("Lengths");
            //if there are two states...
            if (States.Length == 2)
            {
                //then we simply create a transitstate between them and return the result.



                TransitionState buildstate = TransitionBuilder(States[0], States[1], Lengths[0]);
                buildstate.TransitWait = new TimeSpan(0, 0, 1);
                buildstate.GameProcDelegationMode = DelegateProcConstants.Delegate_None;
                return buildstate;
            }
            else
            {
                //otherwise, remove first state and first length...
                GameState[] remainderstates = States.Skip(1).ToArray();
                TimeSpan[] lengths = Lengths.Skip(1).ToArray();
                TransitionState buildstate = TransitionBuilder(States[0], CreateTransitionChain(remainderstates, lengths, TransitionBuilder), lengths[0]);
                buildstate.GameProcDelegationMode = DelegateProcConstants.Delegate_None;
                return buildstate;
            }

        }




    }

    public class TransitionState_BoxWipe : TransitionState
    {
        public int BoxWidth = 16, BoxHeight = 16;
        public TransitionState_BoxWipe(GameState pPrevState, GameState pNextState, TimeSpan pTransitionLength) : base(pPrevState, pNextState, pTransitionLength)
        {
            BoxWidth = BoxHeight = TetrisGame.StatelessRandomizer.Next(10, 100);
        }
    }

    public class TransitionState_AlphaBlend : TransitionState
    {
        public TransitionState_AlphaBlend(GameState pPrevState, GameState pNextState, TimeSpan pTransitionLength) : base(pPrevState, pNextState, pTransitionLength)
        {
        }
    }
    public class TransitionState_BlockRandom : TransitionState_Block
    {
        public TransitionState_BlockRandom(GameState pPrevState, GameState pNextState, TimeSpan pTransitionLength) : base(pPrevState, pNextState, pTransitionLength)
        {
        }
    }
    public class TransitionState_BlockRandomColumnBuild : TransitionState_Block
    {
        public TransitionState_BlockRandomColumnBuild(GameState pPrevState, GameState pNextState, TimeSpan pTransitionLength) : base(pPrevState, pNextState, pTransitionLength)
        {
        }
    }
    
    public abstract class TransitionState_Block : TransitionState
        {
        public int BlockSize = 32;
        public List<SKRect> OrderedBlocks { get; set; } = null;//data initialized on first frame draw, which is when we get information about the canvas and bounds.
        public int RandomSeed { get; private set; }

        public TransitionState_Block(GameState pPrevState, GameState pNextState, TimeSpan pTransitionLength) : base(pPrevState, pNextState, pTransitionLength)
        {
            RandomSeed = (int)TetrisGame.GetTickCount();
            
        }
    }


    public class TransitionState_Pixelate : TransitionState
    {
        public TransitionState_Pixelate(GameState pPrevState, GameState pNextState, TimeSpan pTransitionLength) : base(pPrevState, pNextState, pTransitionLength)
        {
        }
    }
    public class TransitionState_Blur : TransitionState
    {
        public TransitionState_Blur(GameState pPrevState, GameState pNextState, TimeSpan pTransitionLength) : base(pPrevState, pNextState, pTransitionLength)
        {
        }
    }
    public class TransitionState_Melt : TransitionState
    {
        public int Size { get; set; } = 5;
        public List<int> MeltOffset = null;
        public TransitionState_Melt(GameState pPrevState, GameState pNextState, TimeSpan pTransitionLength) : base(pPrevState, pNextState, pTransitionLength)
        {
        }
    }

    
    

}
