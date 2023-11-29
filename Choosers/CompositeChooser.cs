using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{
    public class CompositeChooser : BlockGroupChooser
    {
        private BlockGroupChooser[] Choosers = null;
        private Func<BlockGroupChooser, float> WeightFunction;

        public CompositeChooser(BlockGroupChooser[] pChoosers, Func<BlockGroupChooser, float> pWeightFunction,int Seed):base(null,Seed)
        {
            Choosers = pChoosers;
            WeightFunction = pWeightFunction;

        }
        internal override Nomino GetNext()
        {

            float[] BuildWeights = (from i in Choosers select WeightFunction(i)).ToArray();

            var resultvalue = RandomHelpers.Static.Select(Choosers, BuildWeights,this.rgen);
            return resultvalue.GetNext();
            

        }
        



    }
}
