using BASeTris.GameStates.GameHandlers;
using BASeTris.GameStates.Menu;


namespace BASeTris.Theme.Block
{
    [HandlerTheme("3D Shaded", typeof(Tetris2Handler), typeof(DrMarioHandler), typeof(StandardTetrisHandler), typeof(NTrisGameHandler))]
    [ThemeDescription("Variety of the 3D Shaded themes.")]
    public class ShadedVarietyTheme : VarietyThemeBase
    {
        public override string Name => "Shaded Variety Theme";
        private static NominoTheme[] UseThemes = new NominoTheme[] { new SNESTetris2Theme(), new NESTengenTetrisTheme() };
        public ShadedVarietyTheme() : base(UseThemes)
        {
        }
    }
}
