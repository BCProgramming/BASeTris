using BASeTris.Blocks;
using BASeTris.Theme.Block;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Blocks
{
    public class CascadingBlock :StandardColouredBlock
    {
        private int _ConnectionIndex = 0; 
        public int ConnectionIndex {  get { return _ConnectionIndex; } set { _ConnectionIndex = value; } }
        //normally, all blocks connect to each other in a nomino, if one is supported, they all are.
        //ConnectionIndex can be used so that only blocks that have the same connectionIndex actually support each other.
        //Should also be used to "split" nominoes that would separate into different pieces.
        
        public override char GetCharacterRepresentation()
        {
            return 'C';
        }
        public virtual bool Fixed { get; set; } = false;


        //This function seems to be busted up. :(
        //Apr 20 2024, yeah this one still has issues. runaway recursion, it looks like.
        public bool IsSupported(Nomino Owner, int Row,int Column,TetrisField field, HashSet<CascadingBlock> RecursionBlocks = null)
        {
            if (Fixed) return true; //Fixed blocks are supported. By themselves, I guess.

            if (RecursionBlocks == null)
            {
                RecursionBlocks = new HashSet<CascadingBlock>() { };
            }
            else
            {
                if (RecursionBlocks.Contains(this)) return false;
            }
            
            if (Row + 1 >= field.RowCount) return true; //block is at the bottom, so it is supported.
            var ThisBlock = field.Contents[Row][Column];
            var BlockBelow = field.Contents[Row + 1][Column];
            
            CascadingBlock castcb = BlockBelow as CascadingBlock;
            if (castcb == null && BlockBelow != null && BlockBelow.Owner!=Owner)
                return true; //there is a block below, but it is not a cascading block. We are supported by that block.
            RecursionBlocks.Add(this); //add ourselves to the cascading list.
            if (BlockBelow is CascadingBlock cb)
            {
                
                if (BlockBelow.Owner != Owner) //the block below has to be part of a separate nomino or it doesn't count.
                {
                    var UnderSupported = !(BlockBelow is LineSeriesBlock lsb && lsb.Popping) && cb.IsSupported(cb.Owner, Row + 1, Column, field, RecursionBlocks);
                    if (UnderSupported) return true; //the block below is a CascadingBlock but that block is supported.
                }
            }
            //We've now eliminated two possibilities: This block is sitting on the bottom of the field, or this block is on top of a block belonging to another piece that is supported.
            //Now we need to go through the other pieces in our nomino and see if any of them are supported.
            if (Owner != null && Owner.Count > 1)
            {
                //Of course, if we have no owner or that owner only has one element (which, presumably, must be us) than we can't do so.
                foreach (var iterate in Owner)
                {
                    //skip if the block is either us, or one of the specified recursion blocks.
                    if (iterate.Block == ThisBlock) continue;
                    if (RecursionBlocks.Contains(iterate.Block)) continue;
                    //if (iterate.Block==this) continue;
                    //find the field position of this block.
                    int PosX = iterate.X + Owner.X;
                    int PosY = iterate.Y + Owner.Y;
                    //retrieve block from the field itself.
                    var fieldblock = field.Contents[PosY][PosX];
                    if (fieldblock == iterate.Block)
                    {
                        if (fieldblock is CascadingBlock cb2)
                        {
                            if (cb2.ConnectionIndex != this.ConnectionIndex) continue; //since the connection index is different, this block in our nomino cannot support us.
                                                                                       //otherwise, ask that block if it is supported.

                            var supportresult = cb2.IsSupported(cb2.Owner, PosY, PosX, field, RecursionBlocks);
                            if (supportresult) return true;
                        }
                        else
                        {
                            //other block types support. Though, you wouldn't think they'd appear in the same game, we should account for the possibility.
                            return true;
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
        //encapsulates data regarding additional combining types which can interact with this block.
        //(technically I guess it could have the actual combining index of the block too, which would make for some weird stuff!)
        
        public class AdditionalCombineInfo
        {
            public CombiningTypes CombineType { get; set; }
            public double CombineAddWeight { get; set; } //amount to combine. negative values mean that while this block is compatible, it will take extra blocks in a series to actually "pop".
            public AdditionalCombineInfo(CombiningTypes pCombineType,double pCombineWeight)
            {
                CombineType = pCombineType;
                CombineAddWeight = pCombineWeight;
            }
        }
        public enum CombiningTypes
        {
            Blue,
            Red,
            Yellow,
            Green,
            Orange,
            Magenta
        }
        public static SKColor GetCombiningTypeColor(CombiningTypes src)
        {
                //currently values outside the enum give a random color. We could be deterministic and have the same values give the same (random) color, but there is no need for that currently.
            return src switch
            {
                CombiningTypes.Red => SKColors.Red,
                CombiningTypes.Yellow => SKColors.Yellow,
                CombiningTypes.Blue => SKColors.Blue,
                CombiningTypes.Green => SKColors.Green,
                CombiningTypes.Magenta => SKColors.Magenta,
                CombiningTypes.Orange => SKColors.Orange,
                _ => new SKColor((byte)TetrisGame.StatelessRandomizer.Next(255), (byte)TetrisGame.StatelessRandomizer.Next(255), (byte)TetrisGame.StatelessRandomizer.Next(255))
            }; 
            


        }
        public override char GetCharacterRepresentation()
        {
            return CombiningIndex.ToString()[0];    
        }
        
        public bool Popping { get; set; } = false;
        public int CriticalMass { get; set; } = 4; //'Critical mass' or number that need to be in a row. The maximum critical mass is used when a block is in a series.
        public CombiningTypes CombiningIndex { get; set; } //this is more or less the "color" of the block in question. 

        public int ComboTracker { get; set; }
        public List<AdditionalCombineInfo> AdditionalCombinations { get; private set; } = new List<AdditionalCombineInfo>();

        //while part of a Nomino, items that are part of different sets will remain joined as expected. However when the nomino comes to 'rest' the sets are separated and any set that 
        //can still freely fall will be split out to new Active Groups.
        public int NominoSet { get; set; } = 0; 
 
        
    }
    //a "Master" block is a block that spawns as part of the level. eg. Dr. Mario Viruses or the fixed blocks in Tetris 2. The main difference is in behaviour.
    
    public class LineSeriesPrimaryBlock : LineSeriesBlock
    {
        public bool _Fixed = true;
        public override bool Fixed { get { return _Fixed; }  set { _Fixed = value; } }
        public LineSeriesPrimaryBlock()
        {
           
        }
    }
    /// <summary>
    /// exactly the same as LineSeriesPrimaryBlock, however this has special logic in the cascading game type where
    /// destroying one will destroy all other lineseriesprimaryblocks of the same.
    /// </summary>
    public class LineSeriesPrimaryShinyBlock : LineSeriesPrimaryBlock
    {

    }
}
