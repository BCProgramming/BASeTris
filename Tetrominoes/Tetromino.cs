using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Choosers;

namespace BASeTris.Tetrominoes
{
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
                () => new Tetromino_Z(),
                () => new Tetromino_I(),
                () => new Tetromino_J(),
                () => new Tetromino_L(),
                () => new Tetromino_O(),
                () => new Tetromino_S(),
                () => new Tetromino_T(),
                () => new Tetromino_Y(),
                () => new Tetromino_G(),
                () => new Tetromino_F()

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