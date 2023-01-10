using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.AI;
using BASeTris.Blocks;
using BASeTris.Choosers;
using BASeTris.GameStates.GameHandlers.HandlerOptions;
using BASeTris.Tetrominoes;
using BASeTris.Theme.Block;

namespace BASeTris.GameStates.GameHandlers
{

    //The Tetris 2 Handler is similar to the DrMarioHandler, however, it has some of it's own unique features.
    //1. Some of the tetrominoes have multiple sets. These tetrominoes are initially combined as one, but if one of the groups gets "set" (eg comes to rest on another block)
    //   then the other groups become their own BlockGroup, and can still be controlled.
    //2. In addition to "Viruses" (the fixed blocks that need to be destroyed) each stage has one glowing variant for each color that generated in the stage. If it is destroyed, then all the fixed
    //   blocks of that colour are destroyed as well.
    //3. Presumably we are going to want an appropriate Tetris2Theme.We may be able to re-use some of the internals of the line clear animations for the block clears though.
    [GameScoringHandler(typeof(DrMarioAIScoringHandler), typeof(StoredBoardState.DrMarioScoringRuleData))]
    [HandlerOptionsMenu(typeof(Tetris2OptionsHandler))]
    public class Tetris2Handler : CascadingPopBlockGameHandler<Tetris2Statistics, Tetris2GameOptions>,IGameHandlerChooserInitializer
    {
        private BlockGroupChooser _Chooser = null;
        private String CurrentChooserValue = "Default";
        public override Choosers.BlockGroupChooser GetChooser(IStateOwner pOwner)
        {
            //TODO: proper per-handler configs should include the chooser class to use.
            if (_Chooser == null || CurrentChooserValue != pOwner.Settings.std.Chooser)
            {
                CurrentChooserValue = pOwner.Settings.std.Chooser;
                Type createType = BlockGroupChooser.ChooserTypeFromString(CurrentChooserValue);
                if (createType == null)
                {
                    CurrentChooserValue = pOwner.Settings.std.Chooser;
                    createType = typeof(BagChooser);
                }
                _Chooser = CreateSupportedChooser(createType);
            }
            return _Chooser;
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
        public override NominoTheme DefaultTheme => new Tetris2Theme_Enhanced();
        public override string GetName()
        {
            return "Tetris 2";
        }

        public override Nomino[] GetNominos()
        {

            Tetromino_I TetI = new Tetromino_I((a)=>new LineSeriesBlock());
            Tetromino_J TetJ = new Tetromino_J((a) => new LineSeriesBlock());
            Tetromino_L TetL = new Tetromino_L((a) => new LineSeriesBlock());
            Tetromino_O TetO = new Tetromino_O((a) => new LineSeriesBlock());
            Tetromino_S TetS = new Tetromino_S((a) => new LineSeriesBlock());
            Tetromino_T TetT = new Tetromino_T((a) => new LineSeriesBlock());
            Tetromino_Z TetZ = new Tetromino_Z((a) => new LineSeriesBlock());
            Tetromino_Y TetY = new Tetromino_Y((a) => new LineSeriesBlock());
            Tetromino_G TetG = new Tetromino_G((a) => new LineSeriesBlock());
            Tetromino_F TetF = new Tetromino_F((a) => new LineSeriesBlock());
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

        public BlockGroupChooser CreateSupportedChooser(Type DesiredChooserType)
        {
            if (DesiredChooserType == typeof(BagChooser))
            {
                BagChooser bc = new BagChooser(Tetromino.Tetris2TetrominoFunctions);
                bc.ResultAffector = Tetris2NominoTweaker;
                return bc;
            }
            else if (DesiredChooserType == typeof(NESChooser))
            {
                NESChooser nc = new NESChooser(Tetromino.Tetris2TetrominoFunctions);
                nc.ResultAffector = Tetris2NominoTweaker;
                return nc;
            }
            else if (DesiredChooserType == typeof(GameBoyChooser))
            {
                GameBoyChooser gbc = new GameBoyChooser(Tetromino.Tetris2TetrominoFunctions);
                gbc.ResultAffector = Tetris2NominoTweaker;
                return gbc;
            }
            else if (DesiredChooserType == typeof(FullRandomChooser))
            {
                FullRandomChooser frc = new FullRandomChooser(Tetromino.Tetris2TetrominoFunctions);
                frc.ResultAffector = Tetris2NominoTweaker;
                return frc;
            }
            return null;
        }
    }
}
