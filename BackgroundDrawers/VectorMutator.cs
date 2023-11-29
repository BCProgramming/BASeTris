using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var Angle = TetrisGame.StatelessRandomizer.NextDouble() * 2 * Math.PI;
            return new SKPoint((float)(Math.Sin(Angle) * len), ((float)(Math.Cos(Angle) * len)));
        }
    }
    public class SpeedChangeVectorMutator : VectorMutatorBase
    {
        public float AccelerationChangePercent { get; set; } = .1f;
        public SpeedChangeVectorMutator(float pChangePercent)
        {
            AccelerationChangePercent = pChangePercent;
        }
        public override SKPoint DoMutate(SKPoint src)
        {
            return new SKPoint(src.X * (1 + AccelerationChangePercent), src.Y * (1 + AccelerationChangePercent));
        }
    }
    public class AcceleratingVectorMutator : VectorMutatorBase
    {
        SKPoint? InitialVector;
        uint TotalAccelerationTime;
        uint StartAccelerationTime;
        float TotalAccelerationPercentage=.10f; //10 percent total change by default.

        public AcceleratingVectorMutator(uint AccelerationTime, float pAccelerationChangePercent)
        {
            TotalAccelerationTime = AccelerationTime;
            TotalAccelerationPercentage = pAccelerationChangePercent;
        }

        public override SKPoint DoMutate(SKPoint src)
        {
            var CurrentTick = TetrisGame.GetTickCount();
            if (InitialVector == null)
            {
                InitialVector = src;
                StartAccelerationTime = TetrisGame.GetTickCount();
            }

            //get percentage through the change.
            float CompletionPercent = (float)(CurrentTick - StartAccelerationTime) / TotalAccelerationTime;
            var Mult = 1+(CompletionPercent * (TotalAccelerationPercentage));
            return new SKPoint(InitialVector.Value.X * Mult,InitialVector.Value.Y*Mult);



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
        double StartAngle, EndAngle;
        
        CompositeState CurrentState;
        public MutatorAdvancementType AdvanceType { get; set; } = MutatorAdvancementType.Sequential;
        public enum MutatorAdvancementType
        {
            Sequential,
            Random
        }
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
        private int GetNextIndex()
        {
            if (AdvanceType == MutatorAdvancementType.Sequential)
                return (CurrentMutator + 1) % Mutators.Length;
            else
                return TetrisGame.StatelessRandomizer.Next(Mutators.Length);
            
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
                        CurrentMutator = GetNextIndex();
                        TransitionEnd = Mutators[CurrentMutator].DoMutate(TransitionStart);

                        StartAngle = Math.Atan2(TransitionStart.Y, TransitionStart.X);
                        EndAngle = Math.Atan2(TransitionEnd.Y, TransitionStart.X);

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
                        //alternative: instead of tweening the start and end vectors, get the vector angle, and instead transition through the angles.
                        var TimePercent = (float)(CurrentTickTime - LastTransitionStartTime) / (float)TransitionTickTime;


                        var UseAngle = StartAngle + (EndAngle - StartAngle) * TimePercent;

                        //SKPoint TransitionalVector = new SKPoint((float)Math.Sin(UseAngle) * TransitionStart.Length, (float)Math.Cos(UseAngle) * TransitionStart.Length);
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
}
