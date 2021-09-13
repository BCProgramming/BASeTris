using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Blocks;
using BASeTris.Choosers;
using BASeTris.Tetrominoes;

namespace BASeTris.GameStates.GameHandlers
{

    //The Tetris 2 Handler is similar to the DrMarioHandler, however, it has some of it's own unique features.
    //1. Some of the tetrominoes have multiple sets. These tetrominoes are initially combined as one, but if one of the groups gets "set" (eg comes to rest on another block)
    //   then the other groups become their own BlockGroup, and can still be controlled.
    //2. In addition to "Viruses" (the fixed blocks that need to be destroyed) each stage has one glowing variant for each color that generated in the stage. If it is destroyed, then all the fixed
    //   blocks of that colour are destroyed as well.
    //3. Presumably we are going to want an appropriate Tetris2Theme.We may be able to re-use some of the internals of the line clear animations for the block clears though.
    
    public class Tetris2Handler : CascadingPopBlockGameHandler<Tetris2Statistics, Tetris2GameOptions>
    {
        public override BlockGroupChooser GetChooser()
        {
            BagChooser bc = new BagChooser(Tetromino.Tetris2TetrominoFunctions);
            bc.ResultAffector = Tetris2NominoTweaker;
            return bc;
        }
        private void Tetris2NominoTweaker(Nomino Source)
        {
            //tweak the nomino and set a random combining index.
            foreach (var iterate in Source)
            {
                if (iterate.Block is LineSeriesBlock lsb)
                {
                    lsb.CombiningIndex = TetrisGame.Choose(GetValidBlockCombiningTypes());
                }
            }


        }
        public override string GetName()
        {
            return "Tetris 2";
        }

        public override Nomino[] GetNominos()
        {

            Tetromino_I TetI = new Tetromino_I();
            Tetromino_J TetJ = new Tetromino_J();
            Tetromino_L TetL = new Tetromino_L();
            Tetromino_O TetO = new Tetromino_O();
            Tetromino_S TetS = new Tetromino_S();
            Tetromino_T TetT = new Tetromino_T();
            Tetromino_Z TetZ = new Tetromino_Z();
            Tetromino_Y TetY = new Tetromino_Y();
            Tetromino_G TetG = new Tetromino_G();
            Tetromino_F TetF = new Tetromino_F();
            return new Nomino[] { TetI, TetJ, TetL, TetO, TetS, TetT, TetZ,TetY,TetG,TetF };
        }

        public override void HandleLevelComplete(IStateOwner pOwner, GameplayGameState state)
        {
            throw new NotImplementedException();
        }

        public override IGameCustomizationHandler NewInstance()
        {
            return new Tetris2Handler();
        }
    }
}
