using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Blocks;
using BASeTris.Choosers;

namespace BASeTris.Tetrominoes
{
    [ConsistentNomino]
    public class Tetromino : Nomino
    {
        internal static Func<Nomino>[] StandardTetrominoFunctions =
            new Func<Nomino>[]
            {
                () => new Tetromino_Z(),
                () => new Tetromino_I(),
                () => new Tetromino_J(),
                () => new Tetromino_L(),
                () => new Tetromino_O(),
                () => new Tetromino_S(),
                () => new Tetromino_T(),
                
            };

        private static float[] StandardTetrominoWeights = new float[]
        {
            1f,
            3f,
            1f,
            1f,
            1f,
            1f,
            1f
        };

        internal static Func<Nomino>[] Tetris2TetrominoFunctions =
           new Func<Nomino>[]
           {
                () => new Tetromino_Z((a)=>new LineSeriesBlock()),
                () => new Tetromino_I((a)=>new LineSeriesBlock()),
                () => new Tetromino_J((a)=>new LineSeriesBlock()),
                () => new Tetromino_L((a)=>new LineSeriesBlock()),
                () => new Tetromino_O((a)=>new LineSeriesBlock()),
                () => new Tetromino_S((a)=>new LineSeriesBlock()),
                () => new Tetromino_T((a)=>new LineSeriesBlock()),
                () => new Tetromino_Y((a)=>new LineSeriesBlock()),
                () => new Tetromino_G((a)=>new LineSeriesBlock()),
                () => new Tetromino_F((a)=>new LineSeriesBlock())

           };
        
        private static float[] Tetris2TetrominoWeights = new float[]
        {
            1f,
            3f,
            1f,
            1f,
            1f,
            1f,
            1f,
            1f,
            1f,
            1f
        };
        //TODO: make these able to work with the tetris two sets. Probably (ideally) a standard one for each that allows for an arbitrary Func<Nomino>[].

        public static Choosers.BlockGroupChooser NESTetrominoChooser()
        {
            BlockGroupChooser Chooser = new NESChooser(StandardTetrominoFunctions);
            return Chooser;
        }

        public static Choosers.BlockGroupChooser BagTetrominoChooser()
        {
            BlockGroupChooser Chooser = new BagChooser(StandardTetrominoFunctions);
            return Chooser;
        }

        public static Choosers.BlockGroupChooser RandomTetrominoChooser()
        {
            BlockGroupChooser Chooser = new FullRandomChooser(StandardTetrominoFunctions);
            return Chooser;
        }

        public static Choosers.BlockGroupChooser GameBoyTetrominoChooser()
        {
            BlockGroupChooser Chooser = new GameBoyChooser(StandardTetrominoFunctions);
            return Chooser;
        }

        public static Choosers.BlockGroupChooser WeightedTetrominoChooser()
        {
            BlockGroupChooser Chooser = new WeightedChooser(StandardTetrominoFunctions, StandardTetrominoWeights);
            return Chooser;
        }
    }
}