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
        private static Func<BlockGroup>[] StandardTetrominoFunctions =
        
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
        
        public static Choosers.BlockGroupChooser BagTetrominoChooser()
        {
            BlockGroupChooser Chooser = new BagChooser(TetrisGame.rgen,StandardTetrominoFunctions);
            return Chooser;
        }
        public static Choosers.BlockGroupChooser RandomTetrominoChooser()
        {
            BlockGroupChooser Chooser = new FullRandomChooser(TetrisGame.rgen, StandardTetrominoFunctions);
            return Chooser;
        }

    }
}
