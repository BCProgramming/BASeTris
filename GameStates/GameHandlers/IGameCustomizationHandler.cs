using BASeCamp.BASeScores;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Tetrominoes;
using BASeTris.Choosers;

namespace BASeTris.GameStates.GameHandlers
{
    /// <summary>
    /// interface for game customization, for different Nomino-based games. (eg. Tetris being standard, but could be Tetris 2 or Dr Mario and stuff)
    /// </summary>
    public interface IGameCustomizationHandler
    {
        FieldChangeResult ProcessFieldChange(GameplayGameState state, IStateOwner pOwner, Nomino Trigger);
        IHighScoreList<TetrisHighScoreData> GetHighScores();
        Choosers.BlockGroupChooser Chooser { get;  }
        IGameCustomizationHandler NewInstance();
        TetrominoTheme DefaultTheme { get; }
        void PrepareField(GameplayGameState state, IStateOwner pOwner); //prepare field for a new game. (or level or whatever- basically depends on the type of game)
        BaseStatistics Statistics { get; }

    }

    public class FieldChangeResult
    {
        public int ScoreResult { get; set; }
        public IList<Action> AfterClearActions { get; set; }
    }
    public interface IGameCustomizationStatAreaRenderer<TRenderTarget,TRenderSource,TDataElement,TOwnerType> : BASeCamp.Rendering.Interfaces.IRenderingHandler<TRenderTarget,TRenderSource,TDataElement,TOwnerType>
         where TRenderSource : class
    {

    }

}
