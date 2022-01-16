using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Blocks;
using BASeTris.GameStates.GameHandlers;
using BASeTris.Theme.Block;
namespace BASeTris.GameStates.Menu
{
    public class MenuStateDisplayThemeMenuItem : MenuStateMultiOption<MenuStateThemeSelection>
    {
        private IStateOwner _Owner = null;


        MenuStateThemeSelection[] ThemeOptions = null; /*new MenuStateThemeSelection[] {

            new MenuStateThemeSelection("Standard",typeof(StandardTetrominoTheme), ()=>new StandardTetrominoTheme(StandardColouredBlock.BlockStyle.Style_Shine)),
            new MenuStateThemeSelection("Nintendo NES",typeof(NESTetrominoTheme),() => new NESTetrominoTheme()),
            new MenuStateThemeSelection("Game Boy",typeof(GameBoyTetrominoTheme), ()=>new GameBoyTetrominoTheme()),
            new MenuStateThemeSelection("SNES TD&DRM",typeof(SNESTetrominoTheme), ()=>new SNESTetrominoTheme()),
            new MenuStateThemeSelection("Outlined",typeof(OutlinedTetrominoTheme), ()=>new OutlinedTetrominoTheme()),
            new MenuStateThemeSelection("Simple",typeof(SimpleBlockTheme), ()=>new SimpleBlockTheme()),

        };*/

        private IEnumerable<MenuStateThemeSelection> GetThemeSelectionsForHandler(IGameCustomizationHandler handler)
        {

            var themetypes = Program.GetHandlerThemes(handler.GetType());
            foreach(var themeiter in themetypes)
            {
                ConstructorInfo ci = themeiter.GetConstructor(new Type[] { });
                if(ci!=null)
                {
                    NominoTheme buildResult = (NominoTheme)ci.Invoke(new object[] { });
                    MenuStateThemeSelection msst = new MenuStateThemeSelection(buildResult.Name, themeiter, () => buildResult);
                    yield return msst;
                }
            }


        }
        private IGameCustomizationHandler _Handler;
        public MenuStateDisplayThemeMenuItem(IStateOwner pOwner, IGameCustomizationHandler handler) : base(null)
        {
            _Owner = pOwner;
            Type currentthemetype = null;

            if (_Owner.CurrentState is GameplayGameState gs)
            {
                currentthemetype = gs.PlayField.Theme.GetType();
            }
            else if (_Owner.CurrentState is ICompositeState<GameplayGameState> comp)
            {
                currentthemetype = comp.GetComposite().PlayField.Theme.GetType();
            }
            ThemeOptions = GetThemeSelectionsForHandler(handler).ToArray();
            int currentIndex = 0;
            for (int i = 0; i < ThemeOptions.Length; i++)
            {
                if (ThemeOptions[i].ThemeType == currentthemetype)
                {
                    currentIndex = i;
                    break;
                }
            }


            base.OptionManager = new MultiOptionManagerList<MenuStateThemeSelection>(ThemeOptions,currentIndex);
            var closest = ThemeOptions[currentIndex];
            this.Text = closest.Description;

           
            OnChangeOption += ThemeActivate;

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
            if (e.NewState is GameplayGameState newstate)
            {
                //if it's a standard state, we set the Theme of the TetrisField, and un-assign this event.
                newstate.PlayField.Theme = ActivatedOption.Option.GenerateThemeFunc();
                newstate.DoRefreshBackground = true;
                _Owner.BeforeGameStateChange -= _Owner_BeforeGameStateChange;
            }
        }
    }


    public class MenuStateThemeSelection
    {
        public String Description;
        public Type ThemeType;
        public Func<NominoTheme> GenerateThemeFunc = null;
        public MenuStateThemeSelection(String pDescription,Type pThemeType, Func<NominoTheme> pThemeFunc)
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
