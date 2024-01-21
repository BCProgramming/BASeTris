using BASeTris.GameStates.GameHandlers;


namespace BASeTris.Theme.Block
{
    public abstract class VarietyThemeBase : CompositeBlockTheme
    {
        protected NominoTheme[] SelectableThemes = null;

        protected VarietyThemeBase(NominoTheme[] pSelectableThemes)
        {
            SelectableThemes = pSelectableThemes;
        }
        public override NominoTheme[] GetAllThemes()
        {
            return SelectableThemes;
        }
        PlayFieldBackgroundInfo pfbi = null;
        protected NominoTheme GetRandomTheme()
        {
            return TetrisGame.Choose(SelectableThemes, TetrisGame.StatelessRandomizer);
        }
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IBlockGameCustomizationHandler GameHandler)
        {
            if (pfbi == null)
            {
                pfbi = GetRandomTheme().GetThemePlayFieldBackground(Field, GameHandler);
            }
            return pfbi;
        }
        public override NominoTheme GetGroupTheme(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field)
        {

            return GetRandomTheme();
        }


    }
}
