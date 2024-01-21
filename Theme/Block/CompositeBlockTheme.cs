using BASeTris.GameStates.GameHandlers;
using System;
using System.Collections.Generic;
using System.Linq;


namespace BASeTris.Theme.Block
{
    //helper abstraction. This class allows a derived class to, effectively, be able to composite themes together; eg it could use different themes for different block types.
    //This was built because of an expectation to create ConnectedBlockThemes based on the NES style, but, the NES Style actually has a few different block types. Rather than try to add that capability to the connected block theme, 
    //it will be possible to create a separate connectedblockthemes for each of the NES blocks, and then composite them so that the I/O/T pieces use one and the others use the other, to mimic how the base NES Theme applies the blocks themes to particular Nominoes.
    //by also integrating NNominoGenerator.GetNominoData, AND providing an abstract class to provide the themes that are composited, we can also choose and persist a theme option for arbitrary nominoes too.
    public abstract class CompositeBlockTheme : NominoTheme
    {
        Dictionary<String, NominoTheme> Composites = new Dictionary<string, NominoTheme>();

        public override void ApplyRandom(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field)
        {
            var AllThemes = GetAllThemes(); //get all the composite themes from the derived class
            if (AllThemes != null && AllThemes.Any()) //if any were specified...
                TetrisGame.Choose(AllThemes, TetrisGame.StatelessRandomizer).ApplyRandom(Group, GameHandler, Field); //apply a random one.
        }

        public override void ApplyTheme(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field, ThemeApplicationReason Reason)
        {
            var useTheme = NNominoGenerator.GetNominoData<NominoTheme>(Composites, Group,()=>GetGroupTheme(Group,GameHandler,Field));
            useTheme.ApplyTheme(Group, GameHandler, Field, Reason);
        }
        public abstract NominoTheme[] GetAllThemes();
        public abstract NominoTheme GetGroupTheme(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field);
        
        
    }
}
