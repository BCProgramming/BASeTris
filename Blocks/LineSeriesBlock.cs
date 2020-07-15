using BASeTris.Blocks;
using BASeTris.Theme.Block;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Blocks
{
    public class CascadingBlock :StandardColouredBlock
    {
        public bool Fixed { get; set; } = false;
        public bool IsSupported(Nomino Owner, TetrisField field, List<CascadingBlock> RecursionBlocks = null)
        {
            if (Fixed) return true;
            if (RecursionBlocks == null)
            {
                RecursionBlocks = new List<CascadingBlock>() { this };
            }
            else
            {
                RecursionBlocks.Add(this);
            }
          


            //search through our owner's Nominos...
            foreach (var iterate in Owner)
            {
                if (RecursionBlocks.Contains(iterate.Block)) continue;
                //find the field position of this block.
                int PosX = iterate.X + Owner.X;
                int PosY = iterate.Y + Owner.Y;
                //grab the block at this position.
                var fieldblock = field.Contents[PosY][PosX];
                if (fieldblock == iterate.Block)
                {
                    //ensure the block that is in the field is the one we reference.
                    //we need to do this because a destroyed block will be removed from the field,
                    //but it will not be removed from our owning nomino.
                    //check if the block in question has another block beneath it or is touching the bottom.
                    int CheckPosX = PosX;
                    int CheckPosY = PosY + 1;
                    if (CheckPosY > field.Contents.Length - 1)
                    {
                        return true; //this block is supported by the bottom of the stage/field.
                    }
                    else
                    {
                        //retrieve the block beneath this one.
                        NominoBlock belowBlock = field.Contents[CheckPosY][CheckPosX];
                        if (belowBlock != null)
                        {
                            if (belowBlock is CascadingBlock cb && !RecursionBlocks.Contains(cb))
                            {
                                if(cb.IsSupported(Owner,field,RecursionBlocks))
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                //we consider other block types to be solid.
                                return true;

                            }

                        }
                    }


                }
            }

            return false;
        }
    }
    
    /// <summary>
    /// not the best name, granted. A LineSeriesBlock will "break" in certain game modes if the necessary number line up.
    
    /// </summary>
    public class LineSeriesBlock:CascadingBlock
    {
        public enum CombiningTypes
        {
            Blue,
            Red,
            Yellow
        }
        public int CriticalMass { get; set; } = 4; //'Critical mass' or number that need to be in a row.
        public CombiningTypes CombiningIndex { get; set; } //this is more or less the "color" of the block in question.
 
        
    }
    //a "Master" block is a block that spawns as part of the level. eg. Dr. Mario Viruses or the flashing blocks in Tetris 2. The main difference is in behaviour.
    
    public class LineSeriesMasterBlock : LineSeriesBlock
    {
        public LineSeriesMasterBlock()
        {
            this.Fixed = true;
        }

    }
}
