using BASeCamp.BASeScores;
using BASeTris.Choosers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.GameHandlers
{
    //seen online. interesting idea.
    //"Reverse Tetris" Field generates full of blocks, with a gap on the bottom. 
    //from a set of generated tetrominoes, one must be indicated from the "filled" blocks. If the block is able to 'fall' then it will do so and leave the field.
    //-Next queue can be used for display of the generated tetrominoes. 
    //"active" Nomino does not rotate or fall. Instead, player is able to move it freely throughout the field, and use CW or CCW to indicate the position to try to place/clear.
    //scoring is more or less static, just gives a point for each cleared block.
    /*
    internal class ReverseTetrisHandler : IBlockGameCustomizationHandler
    {
        public string Name => throw new NotImplementedException();

        public NominoTheme DefaultTheme => throw new NotImplementedException();

        public BaseStatistics Statistics => throw new NotImplementedException();

        public GameOptions GameOptions => throw new NotImplementedException();

        public BlockGroupChooser GetChooser(IStateOwner pOwner)
        {
            throw new NotImplementedException();
        }

        public FieldCustomizationInfo GetFieldInfo()
        {
            throw new NotImplementedException();
        }

        public GameOverStatistics GetGameOverStatistics(GameplayGameState state, IStateOwner pOwner)
        {
            throw new NotImplementedException();
        }

        public IHighScoreList GetHighScores()
        {
            throw new NotImplementedException();
        }

        public Nomino[] GetNominos()
        {
            throw new NotImplementedException();
        }

        public IGameCustomizationStatAreaRenderer<TRenderTarget, GameplayGameState, TDataElement, IStateOwner> GetStatAreaRenderer<TRenderTarget, TDataElement>()
        {
            throw new NotImplementedException();
        }

        public IBlockGameCustomizationHandler NewInstance()
        {
            throw new NotImplementedException();
        }

        public void PrepareField(GameplayGameState state, IStateOwner pOwner)
        {
            throw new NotImplementedException();
        }

        public FieldChangeResult ProcessFieldChange(GameplayGameState state, IStateOwner pOwner, Nomino Trigger)
        {
            throw new NotImplementedException();
        }
    }*/
}
