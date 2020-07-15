using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeCamp.BASeScores;
using BASeTris.Choosers;
using BASeTris.Theme.Block;

namespace BASeTris.GameStates.GameHandlers
{
    public class DrMarioHandler : IGameCustomizationHandler
    {
        public BlockGroupChooser Chooser => Duomino.Duomino.BagTetrominoChooser();

        public IHighScoreList<TetrisHighScoreData> GetHighScores()
        {
            return null;
        }

        public IGameCustomizationHandler NewInstance()
        {
            return new DrMarioHandler();
        }

        public FieldChangeResult ProcessFieldChange(GameplayGameState state, IStateOwner pOwner, Nomino Trigger)
        {

            //here we would go through the field and handle where the blocks line up to more than the required critical mass. We would need to not only remove those blocks,
            //but we would need to check all blocks and allow them to fall. I think that should be a new composite game state though.

            return new FieldChangeResult() { ScoreResult = 5 };

        }
        public TetrominoTheme DefaultTheme { get { return new DrMarioTheme(); } }
    }
}
