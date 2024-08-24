using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
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
                () => new Tetromino_Z((a)=>new LineSeriesBlock(){CriticalMass = 3 }),
                () => new Tetromino_I((a)=>new LineSeriesBlock(){CriticalMass = 3 }),
                () => new Tetromino_J((a)=>new LineSeriesBlock(){CriticalMass = 3 }),
                () => new Tetromino_L((a)=>new LineSeriesBlock(){CriticalMass = 3 }),
                () => new Tetromino_O((a)=>new LineSeriesBlock(){CriticalMass = 3 }),
                () => new Tetromino_S((a)=>new LineSeriesBlock(){CriticalMass = 3 }),
                () => new Tetromino_T((a)=>new LineSeriesBlock(){CriticalMass = 3 }),
                () => new Tetromino_Y((a)=>new LineSeriesBlock(){CriticalMass = 3 }),
                () => new Tetromino_G((a)=>new LineSeriesBlock(){CriticalMass = 3 }),
                () => new Tetromino_F((a)=>new LineSeriesBlock(){CriticalMass = 3 })

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

        public static Choosers.BlockGroupChooser NESTetrominoChooser(int pSeed)
        {
            BlockGroupChooser Chooser = new NESChooser(StandardTetrominoFunctions,pSeed);
            return Chooser;
        }

        public static Choosers.BlockGroupChooser BagTetrominoChooser(int pSeed)
        {
            BlockGroupChooser Chooser = new BagChooser(StandardTetrominoFunctions,pSeed);
            return Chooser;
        }

        public static Choosers.BlockGroupChooser RandomTetrominoChooser(int pSeed)
        {
            BlockGroupChooser Chooser = new FullRandomChooser(StandardTetrominoFunctions,pSeed);
            return Chooser;
        }

        public static Choosers.BlockGroupChooser GameBoyTetrominoChooser(int pSeed)
        {
            BlockGroupChooser Chooser = new GameBoyChooser(StandardTetrominoFunctions,pSeed);
            return Chooser;
        }

        public static Choosers.BlockGroupChooser WeightedTetrominoChooser(int pSeed)
        {
            BlockGroupChooser Chooser = new WeightedChooser(StandardTetrominoFunctions, StandardTetrominoWeights,pSeed);
            return Chooser;
        }
        public Tetromino()
        {
        }
        public Tetromino(XElement src, object pContext) : base(src, pContext)
        {
        }
    }
}