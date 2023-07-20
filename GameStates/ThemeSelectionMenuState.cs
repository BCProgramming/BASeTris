using BASeTris.BackgroundDrawers;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates
{
    /// <summary>
    /// given a handler type and a current selection (or null for no option) will show a menu of all available themes for that handler type.
    /// 
    /// </summary>
    public class ThemeSelectionMenuState : MenuState
    {
        public Type HandlerThemesType { get; set; }
        public Type InitialTheme { get; set; }

        public GameState RevertState { get; set; }
       
        private Action<NominoTheme> ThemeChosen { get; set; }

        public bool IsDirty { get; set; } = false;

        public NominoTheme DisplayNominoTheme { get; set; } = null;
        public Type InitialThemeType { get; set; } = null;
        public override DisplayMode SupportedDisplayMode => base.SupportedDisplayMode;
        public ThemeSelectionMenuState(IStateOwner pOwner,IBackground pBG,GameState pRevertState,  Type pHandlerType, Type pCurrentThemeType,Action<NominoTheme> ChosenThemeAction)
        {
            RevertState = pRevertState;
            base.BG = pBG;
            InitialThemeType = pCurrentThemeType;
            var AvailableThemes = MenuStateDisplayThemeMenuItem.GetThemeSelectionsForHandler(pHandlerType);
            var FontSrc = TetrisGame.GetRetroFont(14, pOwner.ScaleFactor);
            NominoTheme CurrentTheme = pCurrentThemeType==null?null:(NominoTheme)Activator.CreateInstance(pCurrentThemeType, new Object[] { });
            DisplayNominoTheme = CurrentTheme;
            StateHeader = "Theme";
            HeaderTypeface = TetrisGame.GetRetroFont(14, 1.0f).FontFamily.Name;
            int DesiredFontPixelHeight = (int)(pOwner.GameArea.Height * (23d / 644d));
            HeaderTypeSize = DesiredFontPixelHeight * .75f;




            ThemeChosen = ChosenThemeAction;
            //first, Cancel option
            MenuStateTextMenuItem CancelItem = new MenuStateTextMenuItem() { Text = "Cancel", TipText = "Cancel theme change" };
            MenuStateTextMenuItem SelectedNominoThemeItem = null;
            List<MenuStateTextMenuItem> ThemeItems = new List<MenuStateTextMenuItem>();
            foreach (var iterate in AvailableThemes)
            {
                MenuStateTextMenuItem mstmi = new MenuStateTextMenuItem() { Text = iterate.Description, TipText = iterate.TipText, Tag = iterate };
                if (iterate.ThemeType == InitialThemeType)
                {
                    SelectedNominoThemeItem = mstmi;
                }
                ThemeItems.Add(mstmi);

            }

            foreach (var designeritem in ThemeItems.Concat(new[] { CancelItem }))
            {
                if (designeritem is MenuStateTextMenuItem mstmi)
                {
                    mstmi.FontFace = FontSrc.FontFamily.Name;
                    mstmi.FontSize = FontSrc.Size;
                    MenuElements.Add(mstmi);
                    if (designeritem == SelectedNominoThemeItem)
                    {
                        base.SelectedIndex = MenuElements.Count - 1;
                    }
                }
            }
            
            base.MenuItemActivated += (obj, act) =>
            {

                if (ThemeItems.Contains(act.MenuElement))
                {
                    MenuStateThemeSelection cast = act.MenuElement.Tag as MenuStateThemeSelection;
                    var resulttheme = cast.GenerateThemeFunc();
                    ThemeChosen(resulttheme);
                    IsDirty = true;
                    base.ActivatedItem = null;
                }

                else if (act.MenuElement == CancelItem)
                {
                    pOwner.CurrentState = RevertState;
                    base.ActivatedItem = null;
                }


            };
            
        }
        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {

            if (new GameKeys[] { GameKeys.GameKey_Down, GameKeys.GameKey_Drop }.Contains(g))
            {
                IsDirty = true;
            }

            base.HandleGameKey(pOwner, g);
        }

    }
}
