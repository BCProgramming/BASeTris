using BASeTris.GameStates.GameHandlers;
using BASeTris.GameStates.Menu;


namespace BASeTris.Theme.Block
{
    [HandlerTheme("SNES Variety", typeof(Tetris2Handler), typeof(DrMarioHandler), typeof(StandardTetrisHandler), typeof(NTrisGameHandler))]
    [ThemeDescription("All SNES Styles at the same time")]
    public class SNESVarietyTheme : VarietyThemeBase
    {
        public override string Name => "SNES Variety Theme";
        private static NominoTheme[] SNESThemes = new NominoTheme[] { new SNESTetris2Theme(), new SNESTetris3Theme(), new SNESTetrominoTheme() };
        public SNESVarietyTheme() : base(SNESThemes)
        {
        }
    }
}
