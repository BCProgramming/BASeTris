using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Blocks;
using BASeTris.Rendering.Adapters;

namespace BASeTris.GameStates
{
   
    public class BlockClearData
    {
        public NominoBlock Block { get; set; }
        public BCPoint[] DirectionVectors { get; set; } = new BCPoint[] { };
        public BlockClearData(NominoBlock pBlock, params BCPoint[] pVector)
        {
            Block = pBlock;
            DirectionVectors = pVector;
        }
        public BlockClearData(NominoBlock pBlock):this(pBlock,BCPoint.Empty)
        {

        }
        
        

    }
    //used to clear a Block. 
    public abstract class FieldBlockClearAction
    {
        public TimeSpan ClearTime;

        protected List<BlockClearData> BlocksEffect = new List<BlockClearData>();

        
        public FieldBlockClearAction(TimeSpan pClearTime)
        {
            ClearTime = pClearTime;
        }
        public virtual void InitBlockAction(IEnumerable<BlockClearData> Blocks)
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
        public abstract void ProcessBlock(IStateOwner pOwner, BlockClearData Target, TimeSpan Elapsed);
    }
   
    public class FieldBlockClearShrinkAction : FieldBlockClearAction
    {
        public FieldBlockClearShrinkAction(TimeSpan pClearTime):base(pClearTime)
        {

        }
        
        public override void ProcessBlock(IStateOwner pOwner, BlockClearData Target, TimeSpan Elapsed)
        {
            double useClear = 0;
            if (Elapsed < ClearTime)
            {
                useClear = 1 - ((double)Elapsed.Ticks / (double)ClearTime.Ticks);
            }
            if (Target != null && Target.Block != null)
            {
                Target.Block.BeforeDraw = (tbdp) =>
                {
                    //TODO: we should figure a way to abstract this such that different clear actions can have their own set of information. At the same time though I guess the renderers would need access to it somehow too... Perhaps some sort of RenderingModifier tag property that the renderer
                    //can access with these bits of extra data?
                    tbdp.FillPercent = (float)useClear;
                };
            }
            else
            {
                ;
            }
            //throw new NotImplementedException();
        }
    }
}
