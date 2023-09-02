using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Blocks;
using BASeTris.GameStates.GameHandlers;
using BASeTris.Theme;
using BASeTris.Theme.Block;
namespace BASeTris.GameStates.Menu
{
    public class MenuStateDisplayThemeMenuItem : MenuStateMultiOption<MenuStateThemeSelection>
    {
        public Action<MenuStateThemeSelection> SimpleSelectionFunction = null; //if set, overrides the behaviour to try changing themes on the handler and gamestates, instead just notifying the function when a theme is chosen.
        private IStateOwner _Owner = null;


        MenuStateThemeSelection[] ThemeOptions = null; /*new MenuStateThemeSelection[] {

            new MenuStateThemeSelection("Standard",typeof(StandardTetrominoTheme), ()=>new StandardTetrominoTheme(StandardColouredBlock.BlockStyle.Style_Shine)),
            new MenuStateThemeSelection("Nintendo NES",typeof(NESTetrominoTheme),() => new NESTetrominoTheme()),
            new MenuStateThemeSelection("Game Boy",typeof(GameBoyTetrominoTheme), ()=>new GameBoyTetrominoTheme()),
            new MenuStateThemeSelection("SNES TD&DRM",typeof(SNESTetrominoTheme), ()=>new SNESTetrominoTheme()),
            new MenuStateThemeSelection("Outlined",typeof(OutlinedTetrominoTheme), ()=>new OutlinedTetrominoTheme()),
            new MenuStateThemeSelection("Simple",typeof(SimpleBlockTheme), ()=>new SimpleBlockTheme()),

        };*/

        public static IEnumerable<MenuStateThemeSelection> GetThemeSelectionsForHandler(Type HandlerType)
        {

            var themetypes = Program.GetHandlerThemes(HandlerType);
            foreach(var themeiter in themetypes)
            {
                ConstructorInfo ci = themeiter.GetConstructor(new Type[] { });
                if(ci!=null)
                {
                    NominoTheme buildResult = (NominoTheme)ci.Invoke(new object[] { });
                    ThemeDescriptionAttribute descAttrib = themeiter.GetCustomAttribute(typeof(ThemeDescriptionAttribute)) as ThemeDescriptionAttribute;
                    String useTip = descAttrib == null ? "" : descAttrib.Description;
                    MenuStateThemeSelection msst = new MenuStateThemeSelection(buildResult.Name, themeiter, () => buildResult,useTip);
                    yield return msst;
                }
            }
           



        }
        private IBlockGameCustomizationHandler _Handler;
        public MenuStateDisplayThemeMenuItem(IStateOwner pOwner, Type CustomizationType,Type InitialThemeType = null) : base(null)
        {
            SubMenuSelection = false;
            _Owner = pOwner;
            Type currentthemetype = InitialThemeType;


            if (currentthemetype == null)
            {
                if (_Owner.CurrentState is GameplayGameState gs)
                {
                    currentthemetype = gs.PlayField.Theme.GetType();
                }
                else if (_Owner.CurrentState is ICompositeState<GameplayGameState> comp)
                {
                    currentthemetype = comp.GetComposite().PlayField.Theme.GetType();
                }
            }
            ThemeOptions = GetThemeSelectionsForHandler(CustomizationType).ToArray();
            int currentIndex = -1;
            for (int i = 0; i < ThemeOptions.Length; i++)
            {
                if (ThemeOptions[i].ThemeType == currentthemetype)
                {
                    currentIndex = i;
                    break;
                }
            }


            base.OptionManager = new MultiOptionManagerList<MenuStateThemeSelection>(ThemeOptions,currentIndex);
            if (currentIndex == -1) this.Text = "Unknown";
            else
            {
                var closest = ThemeOptions[currentIndex];
                this.Text = closest.Description;
            }
            
            OnChangeOption += ThemeActivate;
            OnActivateOption += MenuStateDisplayThemeMenuItem_OnActivateOption;
        }

        private void MenuStateDisplayThemeMenuItem_OnActivateOption(object sender, OptionActivated<MenuStateThemeSelection> e)
        {
            if (e.Option == null) return;
            if (!String.IsNullOrEmpty(e.Option.TipText)) TipText = e.Option.TipText;
        }

        private OptionActivated<MenuStateThemeSelection> ActivatedOption = null;
        public void ThemeActivate(Object sender, OptionActivated<MenuStateThemeSelection> e)
        {
            //Theme Activation can be handled by adding an event handler to the Game Owner
            //on the state change. If the state is StandardTetrisGameState, we set the theme and remove the handler.
            //otherwise, we ignore the event trigger and wait for it to cchange to a StandardTetrisGameState.
            //TipText = "Change Display Theme";
            if (!String.IsNullOrEmpty(e.Option.TipText)) 
            (e.Owner.CurrentState as MenuState).FooterText = e.Option.TipText;
            ActivatedOption = e;
            if (SimpleSelectionFunction != null)
            {
                SimpleSelectionFunction(e.Option);

            }
            else
            {
                _Owner.BeforeGameStateChange -= _Owner_BeforeGameStateChange;
                _Owner.BeforeGameStateChange += _Owner_BeforeGameStateChange;
            }

        }

        private void _Owner_BeforeGameStateChange(object sender, BeforeGameStateChangeEventArgs e)
        {
            if (e.NewState is GameplayGameState newstate)
            {
                //if it's a standard state, we set the Theme of the TetrisField, and un-assign this event.
                var generated = ActivatedOption.Option.GenerateThemeFunc();
                _Owner.Settings.GetSettings(_Owner.GetHandler().Name).Theme = generated.Name;
                _Owner.Settings.Save();
                newstate.PlayField.Theme = generated;
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
        
        public String TipText { get; set; }
        public MenuStateThemeSelection(String pDescription,Type pThemeType, Func<NominoTheme> pThemeFunc,String pTipText)
        {
            Description = pDescription;
            ThemeType = pThemeType;
            TipText = pTipText;
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
    public class ThemeDescriptionAttribute : Attribute
    {
        public String Description {get;set;}
        public ThemeDescriptionAttribute(String pDescription)
        {
            Description = pDescription;
        }
    }
}
