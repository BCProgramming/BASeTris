using BASeTris.GameStates.GameHandlers;
using BASeTris.GameStates.Menu;


namespace BASeTris.Theme.Block
{
    [HandlerTheme("NES Variety", typeof(Tetris2Handler), typeof(DrMarioHandler), typeof(StandardTetrisHandler), typeof(NTrisGameHandler))]
    [ThemeDescription("All NES Styles at the same time")]
    public class NESVarietyTheme : VarietyThemeBase
    {
        public override string Name => "NES Variety Theme";
        private static NominoTheme[] NESThemes = new NominoTheme[] { new NESTetris2Theme(), new NESTetrominoTheme(), new NESTengenTetrisTheme() };
        public NESVarietyTheme() : base(NESThemes)
        {
        }
    }
}
