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
            NorthWest = 16,
            NorthEast = 32,
            SouthWest = 64,
            SouthEast = 128,
            MaxValue = 128 + 64 + 32 + 16 + 8 + 4 + 2 + 1
        }
        
        /*public static Dictionary<ConnectedStyles, String> SuffixLookup = new Dictionary<ConnectedStyles, string>()
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
            



        };*/

        

        public static Dictionary<String,ConnectedStyles> StyleLookup = new Dictionary<String,ConnectedStyles>()
        {
            {"",ConnectedStyles.None},
            {"N",ConnectedStyles.North},
            {"W",ConnectedStyles.West },
            {"E",ConnectedStyles.East},
            {"S",ConnectedStyles.South },
            {"EW",ConnectedStyles.West | ConnectedStyles.East },
            {"NS",ConnectedStyles.North| ConnectedStyles.South},
            {"NE",ConnectedStyles.North | ConnectedStyles.East },
            {"NW",ConnectedStyles.North | ConnectedStyles.West},
            {"SW",ConnectedStyles.South | ConnectedStyles.West },
            {"SE",ConnectedStyles.South | ConnectedStyles.East },
            {"NSW",ConnectedStyles.North | ConnectedStyles.South  | ConnectedStyles.West },
            {"NSE",ConnectedStyles.North | ConnectedStyles.South  | ConnectedStyles.East},
            {"NWE",ConnectedStyles.North | ConnectedStyles.West | ConnectedStyles.East},
            {"SWE",ConnectedStyles.South | ConnectedStyles.West | ConnectedStyles.East},
            {"NSWE",ConnectedStyles.South | ConnectedStyles.North | ConnectedStyles.West | ConnectedStyles.East }
        };
        public static Dictionary<String, ConnectedStyles> CornerStyleLookup = new Dictionary<string, ConnectedStyles>()
        {
            {"1",ConnectedStyles.NorthWest },
            {"2",ConnectedStyles.NorthEast },
            {"3",ConnectedStyles.SouthEast },
            {"4",ConnectedStyles.SouthWest },
            {"12",ConnectedStyles.NorthWest | ConnectedStyles.NorthEast},
            {"13",ConnectedStyles.NorthWest | ConnectedStyles.SouthWest},
            {"14",ConnectedStyles.NorthWest | ConnectedStyles.SouthEast},
            {"23",ConnectedStyles.NorthEast | ConnectedStyles.SouthWest},
            {"24",ConnectedStyles.NorthEast | ConnectedStyles.SouthEast},
            {"34",ConnectedStyles.SouthWest | ConnectedStyles.SouthEast},
            {"123",ConnectedStyles.NorthWest|ConnectedStyles.NorthEast|ConnectedStyles.SouthEast},
            {"234",ConnectedStyles.NorthEast|ConnectedStyles.SouthEast|ConnectedStyles.SouthWest},
            {"341",ConnectedStyles.SouthEast|ConnectedStyles.SouthWest|ConnectedStyles.NorthWest},//
            {"412",ConnectedStyles.SouthWest|ConnectedStyles.NorthWest|ConnectedStyles.NorthEast},//
            {"1234",ConnectedStyles.NorthWest|ConnectedStyles.NorthEast|ConnectedStyles.SouthEast | ConnectedStyles.SouthWest}
        }; //for the diagonals, we use numbers instead of cardinal directions since it is confusing.


        public Dictionary<String, ConnectedStyles> AllStylesLookup = new Dictionary<String,ConnectedStyles>(StyleLookup.Concat(CornerStyleLookup));


        public static Dictionary<ConnectedStyles, String> SuffixLookup = FlipDictionary(StyleLookup);
        public static Dictionary<ConnectedStyles, String> CornerSuffixLookup = FlipDictionary(CornerStyleLookup);
        private static Dictionary<V, K> FlipDictionary<K, V>(Dictionary<K, V> InputDictionary)
        {
            return new Dictionary<V, K>(InputDictionary.Select((k) =>  KeyValuePair.Create<V,K>(k.Value, k.Key)));
        }


        private static Dictionary<ConnectedStyles, ConnectedStyles> RotationStyleLookup = new Dictionary<ConnectedStyles, ConnectedStyles>()
        {
            {ConnectedStyles.None,ConnectedStyles.None },
            {ConnectedStyles.North,ConnectedStyles.East },
            {ConnectedStyles.East,ConnectedStyles.South },
            {ConnectedStyles.South,ConnectedStyles.West },
            {ConnectedStyles.West,ConnectedStyles.North },
            {ConnectedStyles.NorthWest,ConnectedStyles.NorthEast },
            {ConnectedStyles.NorthEast,ConnectedStyles.SouthEast },
            {ConnectedStyles.SouthEast,ConnectedStyles.SouthWest },
            {ConnectedStyles.SouthWest,ConnectedStyles.NorthWest },

        };
        public static ConnectedStyles RotateStyle(ConnectedStyles Src)
        {

            ConnectedStyles Result = ConnectedStyles.None;
            foreach (ConnectedStyles checkenum in (ConnectedStyles[])Enum.GetValues(typeof(ConnectedStyles)))
            {

                if (RotationStyleLookup.ContainsKey(checkenum) && Src.HasFlag(checkenum))
                    Result |= RotationStyleLookup[checkenum];

            }

            return Result;

        }
        /*private static Dictionary<String, String[]> RotationSuffixLookup = new Dictionary<string, string[]>()
        {
            {"", new[]{"","","" } },
            {"N", new[]{"E","S","W" } },
            {"E", new[]{"S","W","N" } },
            {"W", new[]{"N","E","S" } },
            {"S", new[]{"W","N","E" } },
            {"EW", new[]{"NS","EW","NS" } },
            {"NS", new[]{"EW","NS","EW" } },
            {"NE", new[]{"SE","SW","NW" } },
            {"SE", new[]{"SW","NW","NE" } },
            {"SW", new[]{"NW","NE","SE" } },
            {"NW", new[]{"NE","SE","SW" } },
            {"NSWE", new[]{"NSWE","NSWE","NSWE" } },
            {"NSW", new[]{"NWE","NSE","SWE" } },
            {"NWE", new[]{"NSE","SWE","NSW" } },
            {"NSE", new[]{"SWE","NSW","NWE" } },
            {"SWE", new[]{"NSW","NWE","NSE" } }


        };*/
        public static ConnectedStyles[] GetRotations(ConnectedStyles value)
        {
            ConnectedStyles Rotate1 = RotateStyle(value);
            ConnectedStyles Rotate2 = RotateStyle(Rotate1);
            ConnectedStyles Rotate3 = RotateStyle(Rotate2);
            return new ConnectedStyles[] { Rotate1, Rotate2, Rotate3 };

        }
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
        public static CardinalConnectionSet GetImageSet(CardinalImageSet Source, SKColor Colorize)
        {
            return new CardinalConnectionSet<SKImage, SKColor>(Source, Colorize, (c, i) => TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(i, c));
        }
    }

    public class CardinalConnectionDictionary<Key, Data>
    {
        Dictionary<Key, Data[]> AllData = new Dictionary<Key, Data[]>();
        public Data DefaultValue = default(Data);
        
        public Data this[Key key, ConnectedStyles connection]
        {
            get
            {
                if (AllData.ContainsKey(key))
                    return AllData[key][(int)connection];

                return DefaultValue;

            }
            set
            {
                if (!AllData.ContainsKey(key))
                    AllData[key] = new Data[(int)ConnectedStyles.MaxValue];

                AllData[key][(int)connection] = value;

            }
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
        public void SetAll(Element Value)
        {
            
        }

        public CardinalConnectionSet()
        {
        }
        public CardinalConnectionSet(CardinalConnectionSet<Element,Data> Source, Data Input, Func<Data,Element,Element> ProcessFunc, String sOptionalSuffix = "" )
        {
            for (int i = 0; i < (int)ConnectedStyles.MaxValue; i++)
            {

                Element getimage = Source[i];
                if (getimage != null)
                {
                    AllImages[i] = ProcessFunc(Input, getimage); //  TetrisStandardColouredBlockSkiaRenderingHandler.RecolorImage(getimage, Input);
                }
                else
                {
                    
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
