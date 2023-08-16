using BASeTris.Rendering.Skia;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BASeTris.Theme.Block.CardinalConnectionSet;

namespace BASeTris.Theme.Block
{
    public class CardinalConnectionSet
    {
        [Flags]
        public enum ConnectedStyles
        {
            None = 0,
            North = 1,
            South = 2,
            East = 4,
            West = 8,
            NorthEast = 16,
            NorthWest = 32,
            SouthEast = 64,
            SouthWest = 128,
            MaxValue = 128 + 64 + 32 + 16 + 8 + 4 + 2 + 1
        }
        public static Dictionary<ConnectedStyles, String> SuffixLookup = new Dictionary<ConnectedStyles, string>()
        {
            {ConnectedStyles.None,"" },
            {ConnectedStyles.North,"N" },
            {ConnectedStyles.West,"W" },
            {ConnectedStyles.East,"E" },
            {ConnectedStyles.South,"S" },
            {ConnectedStyles.West | ConnectedStyles.East,"EW" },
            {ConnectedStyles.North| ConnectedStyles.South,"NS" },
            {ConnectedStyles.North | ConnectedStyles.East,"NE" },
            {ConnectedStyles.North | ConnectedStyles.West,"NW" },
            {ConnectedStyles.South | ConnectedStyles.West,"SW" },
            {ConnectedStyles.South | ConnectedStyles.East,"SE" },
            {ConnectedStyles.North | ConnectedStyles.South  | ConnectedStyles.West,"NSW" },
            {ConnectedStyles.North | ConnectedStyles.South  | ConnectedStyles.East,"NSE" },
            {ConnectedStyles.North | ConnectedStyles.West | ConnectedStyles.East,"NWE" },
            {ConnectedStyles.South | ConnectedStyles.West | ConnectedStyles.East,"SWE" },
            {ConnectedStyles.South | ConnectedStyles.North | ConnectedStyles.West | ConnectedStyles.East,"NSWE" }
        };
        public static String GetSuffix(ConnectedStyles value)
        {
            if (SuffixLookup.ContainsKey(value)) return SuffixLookup[value]; return null;
        }
    }
    public class CardinalImageSet : CardinalConnectionSet<SKImage, SKColor>
    {
        public CardinalImageSet()
        {
        }
        public CardinalImageSet(CardinalImageSet Source, SKColor Colorize):base(Source,Colorize,(c,i)=> TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(i,c))
        {
        }
    }
    public class CardinalConnectionSet<Element,Data> : CardinalConnectionSet
    {
        
       
       
        Element[] AllImages = new Element[(int)ConnectedStyles.MaxValue];
        /*
        public SKImage None { get; set; }
        public SKImage North { get; set}
        public SKImage West { get; set; }
        public SKImage South { get; set; }
        public SKImage East { get; set; }

        public SKImage NorthSouth { get; set; }
        public SKImage WestEast { get; set; }
        public SKImage NorthWest { get; set; }
        public SKImage NorthEast { get; set; }
        public SKImage SouthEast { get; set; }
        public SKImage SouthWest { get; set; }
        public SKImage NorthSouthWest { get; set; }
        public SKImage NorthSouthEast { get; set; }
        public SKImage NorthWestEast { get; set; }
        public SKImage SouthWestEast { get; set; }
        */
        public CardinalConnectionSet()
        {
        }
        public CardinalConnectionSet(CardinalConnectionSet<Element,Data> Source, Data Input, Func<Data,Element,Element> ProcessFunc )
        {
            for (int i = 0; i < (int)ConnectedStyles.MaxValue; i++)
            {

                Element getimage = Source[i];
                if (getimage != null)
                {
                    AllImages[i] = ProcessFunc(Input,getimage); //  TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(getimage, Input);
                }


            }

        }
        public Element this[ConnectedStyles index]
        {
            get
            {
                return this[(int)index];
            }
            set
            {
                this[(int)index] = value;
            }
        }
        public Element this[int index]
        {
            get
            {
                return AllImages[index];
            }
            set
            {
                AllImages[index] = value;
            }
        }
    }
}
