using BASeTris.Blocks;
using BASeTris.Choosers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BASeTris.Duomino
{
    public class Duomino : Nomino
    {
        internal static Func<Nomino>[] StandardDuominoFunctions =
            new Func<Nomino>[]
            {
                () => new Duomino(),

            };



        public static Choosers.BlockGroupChooser BagTetrominoChooser()
        {
            BlockGroupChooser Chooser = new BagChooser(StandardDuominoFunctions);
            return Chooser;
        }
        private static Point[] Duomino_Point_1 = new Point[]
    {
            new Point(0, 0), new Point(0, 0),new Point(1, 0),new Point(0, 1)
    };
        private static Point[] Duomino_Point_2 = new Point[]
    {
            new Point(1, 0), new Point(0, 1),new Point(0, 0),new Point(0, 0)
    };

        public NominoElement FirstBlock
        {
            get
            {
                return BlockData.First();
            }
        }
        public NominoElement SecondBlock
        {
            get
            {
                return BlockData.Last();
            }
        }

        public Duomino()
        {
            //generate a new Duomino. Duomino's don't actually use StandardColouredBlocks.


            BlockData = Nomino.GetNominoEntries(new[] { Duomino_Point_1, Duomino_Point_2 }, 
                (i) => new LineSeriesBlock()
                {
                    CombiningIndex = TetrisGame.Choose(new LineSeriesBlock.CombiningTypes[] { LineSeriesBlock.CombiningTypes.Yellow, LineSeriesBlock.CombiningTypes.Red, LineSeriesBlock.CombiningTypes.Blue })
                }  ).ToList();
            
            base.SpecialName = "Pill";
            base.SetBlockOwner();
            base.RecalcExtents();
        }

    }
}
