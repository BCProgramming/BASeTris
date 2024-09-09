using BASeTris.GameStates.GameHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{
    /// <summary>
    /// BlockGroupChooser which, well, I guess isn't even really a chooser. Just takes a single function.
    /// This is needed for certain types of game modes where there isn't a specific, determinate set of possible Nominoes, or where that finite set isn't feasible.
    /// For example: Dr.Mario just has Duominoes and will adjust them with the result affector, so should use this. Additionally, Pentris and other "N-tris" versions will have escalating numbers of Nominoes which
    /// become unfeasible in terms of memory usage beyond around 13 or 14. Not to mention with that many it starts to get weird.
    /// </summary>
    /// 
    [ChooserCompatibility(typeof(DrMarioHandler))]
    public class SingleFunctionChooser : BlockGroupChooser
    {
        
        public SingleFunctionChooser(Func<Nomino> pAvailable,int pSeed) : base(new Func<Nomino>[] { pAvailable },pSeed) //seed is not used here.
        {
        }
        protected override Nomino GetNext()
        {
            return _Available[0]();
        }
    }
}
