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
        public virtual bool Fixed { get; set; } = false;
        public bool IsSupported(Nomino Owner, TetrisField field, List<CascadingBlock> RecursionBlocks = null)
        {
            if (Fixed) return true;
            if (RecursionBlocks == null)
            {
                RecursionBlocks = new List<CascadingBlock>() { };
            }
            else
            {
                
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
                            //If the block below is part of the same nomino as the one we are checking, then we must disregard it as supporting this one. A Nomino cannot support itself!
                            if (!Owner.HasBlock(belowBlock))
                            {
                                if (belowBlock is CascadingBlock cb && !RecursionBlocks.Contains(cb))
                                {
                                    RecursionBlocks.Add(this);
                                    if (cb.IsSupported(Owner, field, RecursionBlocks))
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
                else
                {
                    ;
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
        public bool Popping { get; set; } = false;
        public int CriticalMass { get; set; } = 4; //'Critical mass' or number that need to be in a row.
        public CombiningTypes CombiningIndex { get; set; } //this is more or less the "color" of the block in question.


        //while part of a Nomino, items that are part of different sets will remain joined as expected. However when the nomino comes to 'rest' the sets are separated and any set that 
        //can still freely fall will be split out to new Active Groups.
        public int NominoSet { get; set; } = 0; 
 
        
    }
    //a "Master" block is a block that spawns as part of the level. eg. Dr. Mario Viruses or the flashing blocks in Tetris 2. The main difference is in behaviour.
    
    public class LineSeriesMasterBlock : LineSeriesBlock
    {
        public override bool Fixed { get { return true; }  set { } }
        public LineSeriesMasterBlock()
        {
           
        }

    }
}
