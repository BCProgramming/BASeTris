using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Choosers;

namespace BASeTris.Tetrominoes
{
    public class Tetromino : BlockGroup
    {
        internal static Func<BlockGroup>[] StandardTetrominoFunctions =
        
            new Func<BlockGroup>[]
            {
                () => new Tetromino_Z(),
                () => new Tetromino_I(),
                () => new Tetromino_J(),
                () => new Tetromino_L(),
                () => new Tetromino_O(),
                () => new Tetromino_S(),
                () => new Tetromino_T()

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
