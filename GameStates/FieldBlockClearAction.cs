using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Blocks;

namespace BASeTris.GameStates
{
   

    //used to clear a Block. 
    public abstract class FieldBlockClearAction
    {
        public TimeSpan ClearTime;
        
        protected List<NominoBlock> BlocksEffect = new List<NominoBlock>();
        public FieldBlockClearAction(TimeSpan pClearTime)
        {
            ClearTime = pClearTime;
        }
        public virtual void InitBlockAction(IEnumerable<NominoBlock> Blocks)
        {
            BlocksEffect = Blocks.ToList();
 }

        public virtual bool Proc(IStateOwner pOwner,TimeSpan Elapsed)
        {
            foreach(var iterate in BlocksEffect)
            {
                ProcessBlock(pOwner,iterate, Elapsed);
            }
            return Elapsed > ClearTime;
        }
        public abstract void ProcessBlock(IStateOwner pOwner, NominoBlock Target, TimeSpan Elapsed);
    }
   
    public class FieldBlockClearShrinkAction : FieldBlockClearAction
    {
        public FieldBlockClearShrinkAction(TimeSpan pClearTime):base(pClearTime)
        {

        }
        
        public override void ProcessBlock(IStateOwner pOwner, NominoBlock Target, TimeSpan Elapsed)
        {
            double useClear = 0;
            if (Elapsed < ClearTime)
            {
                useClear = 1 - ((double)Elapsed.Ticks / (double)ClearTime.Ticks);
            }
            Target.BeforeDraw = (tbdp) =>
             {
                 tbdp.FillPercent = (float)useClear;
             };

            //throw new NotImplementedException();
        }
    }
}
