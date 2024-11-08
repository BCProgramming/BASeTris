﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{
    
    class WeightedChooser : BlockGroupChooser
    {
        protected float[] pWeights;

        public delegate float WeightLookupRoutine(Func<Nomino> ForFunc);

        protected WeightLookupRoutine LookupRoutine = null;

        protected float DefaultWeightLookup(Func<Nomino> ForFunc)
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

        public WeightedChooser(Func<Nomino>[] pAvailable, float[] pWeight,int pSeed) : base(pAvailable,pSeed)
        {
            pWeights = pWeight;
            LookupRoutine = DefaultWeightLookup;
        }

        public WeightedChooser(Func<Nomino>[] pAvailable, WeightLookupRoutine lookuproutine,int pSeed) : base(pAvailable,pSeed)
        {
            LookupRoutine = lookuproutine;
        }


        protected override Nomino GetNext()
        {
            float[] useWeight = (from p in _Available select LookupRoutine(p)).ToArray();
            return RandomHelpers.Static.Select(_Available, useWeight,base.rgen)();
        }
    }
}