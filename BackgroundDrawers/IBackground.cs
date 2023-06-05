using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.BackgroundDrawers;
using SkiaSharp;

namespace BASeTris.BackgroundDrawers
{
    public interface IVectorMutator
    {
        SKPoint Mutate(SKPoint src);
    }

    public abstract class VectorMutatorBase : IVectorMutator
    {
        public int MinTickDelay { get; set; } = 0; //0 is default, called every time.
        protected uint LastTick { get; set; } = 0;
        public SKPoint Mutate(SKPoint src)
        {
            if (LastTick + MinTickDelay < TetrisGame.GetTickCount())
            {
                LastTick = TetrisGame.GetTickCount();
                var result = DoMutate(src);
                return result;
            }
            return src;
        }
        public abstract SKPoint DoMutate(SKPoint src);

        public VectorMutatorBase(int pTickDelay)
        {
            MinTickDelay = pTickDelay;
        }
        public VectorMutatorBase() : this(0)
        {
        }

    }
    public class RandomVectorMutator : VectorMutatorBase
    {
        public RandomVectorMutator(int TicksBetween) : base(TicksBetween)
        {
        }
        public override SKPoint DoMutate(SKPoint src)
        {
            var len = src.Length;
            var Angle = TetrisGame.rgen.NextDouble() * 2 * Math.PI;
            return new SKPoint((float)(Math.Sin(Angle) * len), ((float)(Math.Cos(Angle) * len)));
        }
    }
    public class CompositeVectorMutator : VectorMutatorBase
    {
        private VectorMutatorBase[] Mutators = null;
        int CurrentMutator = 0;
        uint TransitionTickTime = 1000;
        uint MutatorStableTime = 5000;
        uint LastTransitionStartTime = 0;
        uint LastStableStartTime = 0;
        bool CallMutatorWhenStable = false;
        SKPoint LastMutatorPoint;
        SKPoint TransitionStart, TransitionEnd;

        CompositeState CurrentState;
        enum CompositeState
        {
            Stable,
            Transitioning
        }
        //not implying transitioning individuals are not stable!
        public CompositeVectorMutator(params VectorMutatorBase[] pMutators)
        {
            base.MinTickDelay = 0;
            Mutators = pMutators;
        }
        public override SKPoint DoMutate(SKPoint src)
        {
            var CurrentTickTime = TetrisGame.GetTickCount();
            if (LastStableStartTime == 0) { LastStableStartTime = TetrisGame.GetTickCount(); CurrentState = CompositeState.Stable; LastMutatorPoint = Mutators[CurrentMutator].DoMutate(src); }
            //note: we only call the base mutators listing when we want a new location.
            switch (CurrentState)
            {
                case CompositeState.Stable:

                    if (CurrentTickTime - MutatorStableTime > LastStableStartTime)
                    {
                        TransitionStart = LastMutatorPoint;
                        CurrentMutator = (CurrentMutator + 1) % Mutators.Length;
                        TransitionEnd = Mutators[CurrentMutator].DoMutate(TransitionStart);
                        CurrentState = CompositeState.Transitioning;
                        LastTransitionStartTime = TetrisGame.GetTickCount();
                        return LastMutatorPoint;
                    }
                    else
                    {
                        return LastMutatorPoint;
                    }

                    
                case CompositeState.Transitioning:
                    if (CurrentTickTime - TransitionTickTime > LastTransitionStartTime)
                    {

                        //switch back to stable.
                        LastMutatorPoint = TransitionEnd;
                        CurrentState = CompositeState.Stable;
                        LastStableStartTime = CurrentTickTime;
                    }
                    else
                    {
                        var TimePercent = (float)(CurrentTickTime - LastTransitionStartTime)/(float)TransitionTickTime;
                        var Diff = TransitionEnd - TransitionStart;
                        SKPoint TransitionalVector = TransitionStart + new SKPoint(Diff.X * TimePercent, Diff.Y * TimePercent);
                        return TransitionalVector;
                    }


                    break;
                default:
                    return LastMutatorPoint;
            }
            return LastMutatorPoint;



        }
    }

    //base class for all Background Draw Data.
    //note that this is part of the Background interface definition. This 
    //is intended to be used for storing any Graphics API specific information. if needed, with subclasses for the appropriate
    //types being implemented as needed for each possible renderer.
    public abstract class BackgroundDrawData
    {

    }
    public class SkiaBackgroundDrawData :BackgroundDrawData
    {
        public SKRect Bounds;
        public SkiaBackgroundDrawData(SKRect pBounds)
        {
            Bounds = pBounds;
        }
    }
    public class GDIBackgroundDrawData : BackgroundDrawData
    {
        public RectangleF Bounds;
        public GDIBackgroundDrawData(RectangleF pBounds)
        {
            Bounds = pBounds;
        }
    }
    public class NullBackgroundDrawData : BackgroundDrawData
    {

    }

    public abstract class Background<T> : Background, IBackground<T> where T:BackgroundDrawData,new()
    {
        //public abstract void DrawProc(Graphics g, RectangleF Bounds);
        public T Data { get; set; }
    }
    public abstract class Background : IBackground
    {
        public abstract void FrameProc(IStateOwner pState);
        
    }
    public interface IBackground<T> : IBackground where T:BackgroundDrawData
    {
        /// <summary>
        /// Routine called to perform the drawing task. 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        //void DrawProc(Graphics g, RectangleF Bounds);
        //drawing is handled by rendering provider now...

        /// <summary>
        /// Called each Game "tick" to allow the background implementation to perform any necessary state changes.
        /// </summary>
        
    }
    public interface IBackground
    {
        void FrameProc(IStateOwner pState);
    }



    public class BackgroundInformationAttribute : Attribute
    {
        public Type CanvasType { get; set; }
        public String StyleName { get; set; }
        public BackgroundInformationAttribute(Type pCanvasType,String pStyleName)
        {
            CanvasType = pCanvasType;
            StyleName = pStyleName;
        }
        public static String Style_Standard = "STANDARD";
    }
}