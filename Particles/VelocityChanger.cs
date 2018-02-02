using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris
{
    public abstract class VelocityChanger
    {
        public delegate PointF VelocityChangerFunction(PointF Input);
        public abstract PointF PerformFrame(IStateOwner gstate, PointF CurrentLocation);

        public abstract PointF getVelocity();

    }

    public class VelocityChangerLinear : VelocityChanger
    {
        protected PointF _Delta = new PointF(0, 0);
        public PointF Delta
        {
            get { return _Delta; }
            set { _Delta = value; }
        }
        public VelocityChangerLinear(PointF pDelta)
        {

            _Delta = pDelta;

        }
        public VelocityChangerLinear()
            : this(new PointF(0, 2))
        {

        }
        public override PointF getVelocity()
        {
            return _Delta;
        }
        public override PointF PerformFrame(IStateOwner gstate, PointF CurrentLocation)
        {
            //return new PointF(CurrentLocation.X + _Delta.X, CurrentLocation.Y + _Delta.Y);
            TrigFunctions.IncrementLocation(gstate, ref CurrentLocation, _Delta);
            return CurrentLocation;
        }


    }
    /// <summary>
    /// Class used to present "Exponential" changes to Velocity. This in most cases just means subject to gravity, really.
    /// </summary>
    public class VelocityChangerExponential : VelocityChangerLinear
    {
        private PointF _Acceleration = new PointF(1, 1.01f);

        public PointF Acceleration { get { return _Acceleration; } set { _Acceleration = value; } }

        public VelocityChangerExponential(PointF pDelta, PointF pAcceleration)
            : base(pDelta)
        {
            _Acceleration = pAcceleration;


        }
        public override PointF PerformFrame(IStateOwner gstate, PointF CurrentLocation)
        {
            _Delta = new PointF(_Delta.X * _Acceleration.X, _Delta.Y * _Acceleration.Y);
            return base.PerformFrame(gstate, CurrentLocation);
        }


    }
    public class VelocityChangerParametric : VelocityChangerLinear
    {


        public delegate float ParametricFunction(PointF Currposition);


        private ParametricFunction _ParametricX = null;
        private ParametricFunction _ParametricY = null;


        public ParametricFunction ParametricX
        {
            get { return _ParametricX; }
            set { _ParametricX = value; }
        }
        public ParametricFunction ParametricY
        {
            get { return _ParametricY; }
            set { _ParametricY = value; }

        }

        public VelocityChangerParametric(ParametricFunction xFunction, ParametricFunction yFunction)
        {

            _ParametricX = xFunction;
            _ParametricY = yFunction;


        }

        public override PointF PerformFrame(IStateOwner gstate, PointF CurrentLocation)
        {
            //throw new NotImplementedException();
            float XValue = 0, YValue = 0;

            if (_ParametricX != null) XValue = _ParametricX(CurrentLocation);
            if (_ParametricY != null) YValue = _ParametricY(CurrentLocation);
            Debug.Print("parametric: X=" + XValue + " Y=" + YValue);
            _Delta = new PointF(XValue, YValue);
            return base.PerformFrame(gstate, CurrentLocation);
            //return new PointF(CurrentLocation.X + XValue,CurrentLocation.Y+YValue);

        }




    }
}
