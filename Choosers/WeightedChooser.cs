using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{
    class WeightedChooser : BlockGroupChooser
    {
        protected float[] pWeights;

        public delegate float WeightLookupRoutine(Func<BlockGroup> ForFunc);

        protected WeightLookupRoutine LookupRoutine = null;

        protected float DefaultWeightLookup(Func<BlockGroup> ForFunc)
        {
            int foundindex = -1;
            for (int i = 0; i < _Available.Length; i++)
            {
                if (_Available[i] == ForFunc)
                {
                    foundindex = i;
                    break;
                }
            }

            if (foundindex == -1) return 1;
            return pWeights[foundindex % pWeights.Length];
        }

        public WeightedChooser(Func<BlockGroup>[] pAvailable, float[] pWeight) : base(pAvailable)
        {
            pWeights = pWeight;
            LookupRoutine = DefaultWeightLookup;
        }

        public WeightedChooser(Func<BlockGroup>[] pAvailable, WeightLookupRoutine lookuproutine) : base(pAvailable)
        {
            LookupRoutine = lookuproutine;
        }


        public override BlockGroup GetNext()
        {
            float[] useWeight = (from p in _Available select LookupRoutine(p)).ToArray();
            return RandomHelper.Select(_Available, useWeight)();
        }
    }
}