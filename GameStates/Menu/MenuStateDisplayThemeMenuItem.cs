using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.TetrisBlocks;

namespace BASeTris.GameStates.Menu
{
    public class MenuStateDisplayThemeMenuItem : MenuStateMultiOption<MenuStateThemeSelection>
    {
        private IStateOwner _Owner = null;

        
        MenuStateThemeSelection[] ThemeOptions = new MenuStateThemeSelection[] {

            new MenuStateThemeSelection("Standard",typeof(StandardTetrominoTheme), ()=>new StandardTetrominoTheme(StandardColouredBlock.BlockStyle.Style_Shine)),
            new MenuStateThemeSelection("Nintendo NES",typeof(NESTetrominoTheme),() => new NESTetrominoTheme()),
            new MenuStateThemeSelection("Game Boy",typeof(GameBoyTetrominoTheme), ()=>new GameBoyTetrominoTheme())
        };
        public MenuStateDisplayThemeMenuItem(IStateOwner pOwner) : base(null)
        {
            _Owner = pOwner;
            
            base.OptionManager = new MultiOptionManagerList<MenuStateThemeSelection>(ThemeOptions, 1);
            var closest = ThemeOptions.First();
            
            this.Text = closest.Description;
            OptionManager.SetCurrentIndex(Array.IndexOf(ThemeOptions, closest));
            OnActivateOption += ThemeActivate;

        }
        private OptionActivated<MenuStateThemeSelection> ActivatedOption = null;
        public void ThemeActivate(Object sender, OptionActivated<MenuStateThemeSelection> e)
        {
            //Theme Activation can be handled by adding an event handler to the Game Owner
            //on the state change. If the state is StandardTetrisGameState, we set the theme and remove the handler.
            //otherwise, we ignore the event trigger and wait for it to cchange to a StandardTetrisGameState.
            ActivatedOption = e;
            _Owner.BeforeGameStateChange -= _Owner_BeforeGameStateChange;
            _Owner.BeforeGameStateChange += _Owner_BeforeGameStateChange;

        }

        private void _Owner_BeforeGameStateChange(object sender, BeforeGameStateChangeEventArgs e)
        {
            if (e.NewState is StandardTetrisGameState newstate)
            {
                //if it's a standard state, we set the Theme of the TetrisField, and un-assign this event.
                newstate.PlayField.Theme = ActivatedOption.Option.GenerateThemeFunc();
                _Owner.BeforeGameStateChange -= _Owner_BeforeGameStateChange;
            }
        }
    }


    public class MenuStateThemeSelection
    {
        public String Description;
        public Type ThemeType;
        public Func<TetrominoTheme> GenerateThemeFunc = null;
        public MenuStateThemeSelection(String pDescription,Type pThemeType, Func<TetrominoTheme> pThemeFunc)
        {
            Description = pDescription;
            ThemeType = pThemeType;
            GenerateThemeFunc = pThemeFunc;
        }
        public MenuStateThemeSelection()
        {

        }
     
        public override String ToString()
        {
            return Description;
        }

    }
}
