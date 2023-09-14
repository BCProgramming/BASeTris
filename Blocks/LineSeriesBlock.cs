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
        public bool IsSupported(Nomino Owner, int Row,int Column,TetrisField field, List<CascadingBlock> RecursionBlocks = null)
        {
            if (Fixed) return true;
            if (RecursionBlocks == null)
            {
                RecursionBlocks = new List<CascadingBlock>() { };
            }
            else
            {
                
            }
            
            if (Row + 1 >= field.RowCount) return true; //block is at the bottom, so it is supported.
            var ThisBlock = field.Contents[Row][Column];
            var BlockBelow = field.Contents[Row + 1][Column];
            
            CascadingBlock castcb = BlockBelow as CascadingBlock;
            if (castcb == null && BlockBelow != null && BlockBelow.Owner!=Owner)
                return true; //there is a block below, but it is not a cascading block. We are supported by that block.

            if (BlockBelow is CascadingBlock cb)
            {
                RecursionBlocks.Add(this); //add ourselves to the cascading list.
                var UnderSupported =BlockBelow.Owner!=Owner && !(BlockBelow is LineSeriesBlock lsb && lsb.Popping) &&  cb.IsSupported(cb.Owner, Row + 1, Column, field, RecursionBlocks);
                if (UnderSupported) return true; //the block below is a CascadingBlock but that block is supported.
               
            }

            if (Owner != null && Owner.Count > 1)
            {
                //if we have an owner and it has more than one element, we must look through to see if there are any blocks in that nomino that are supported.
                //search through our owner's Nominos...
                foreach (var iterate in Owner)
                {
                    if (iterate.Block == ThisBlock) continue;
                    if (RecursionBlocks.Contains(iterate.Block)) continue;
                    //if (iterate.Block==this) continue;
                    //find the field position of this block.
                    int PosX = iterate.X + Owner.X;
                    int PosY = iterate.Y + Owner.Y;
                    //grab the block at this position.
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
            public double CombineAddWeight { get; set; } //amount to combine. negative values mean that while this block is compatible, it will take more pills to actually "pop".
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
            return src switch
            {
                CombiningTypes.Red => SKColors.Red,
                CombiningTypes.Yellow => SKColors.Yellow,
                CombiningTypes.Blue => SKColors.Blue,
                CombiningTypes.Green => SKColors.Green,
                CombiningTypes.Magenta => SKColors.Magenta,
                CombiningTypes.Orange => SKColors.Orange,
                _ => new SKColor((byte)TetrisGame.rgen.Next(255), (byte)TetrisGame.rgen.Next(255), (byte)TetrisGame.rgen.Next(255))
            }; 
            


        }
        public override char GetCharacterRepresentation()
        {
            return CombiningIndex.ToString()[0];    
        }
        
        public bool Popping { get; set; } = false;
        public int CriticalMass { get; set; } = 4; //'Critical mass' or number that need to be in a row.
        public CombiningTypes CombiningIndex { get; set; } //this is more or less the "color" of the block in question. 

        public int ComboTracker { get; set; }
        public List<AdditionalCombineInfo> AdditionalCombinations { get; private set; } = new List<AdditionalCombineInfo>();

        //while part of a Nomino, items that are part of different sets will remain joined as expected. However when the nomino comes to 'rest' the sets are separated and any set that 
        //can still freely fall will be split out to new Active Groups.
        public int NominoSet { get; set; } = 0; 
 
        
    }
    //a "Master" block is a block that spawns as part of the level. eg. Dr. Mario Viruses or the flashing blocks in Tetris 2. The main difference is in behaviour.
    
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
