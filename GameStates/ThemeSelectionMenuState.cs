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
        private List<List<MenuStateMenuItem>> Pages = new List<List<MenuStateMenuItem>>();
        private int CurrentPage = 0;
        public int MaxPerPage { get; set; } = 6;
        private static IEnumerable<List<T>> Partition<T>(IList<T> source, Int32 size)
        {
            for (int i = 0; i < Math.Ceiling(source.Count / (Double)size); i++)
                yield return new List<T>(source.Skip(size * i).Take(size));
        }
        private IStateOwner _Owner = null;
        public ThemeSelectionMenuState(IStateOwner pOwner,IBackground pBG,GameState pRevertState,  Type pHandlerType, Type pCurrentThemeType,Action<NominoTheme> ChosenThemeAction)
        {
            _Owner= pOwner;
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
            CancelItem = new MenuStateTextMenuItem() { Text = "Cancel", TipText = "Cancel theme change" };
            PreviousPageItem = new MenuStateTextMenuItem() { Text = "Previous", TipText = "Previous Page" };
            NextPageItem = new MenuStateTextMenuItem() { Text = "Next", TipText = "Next Page" };
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

            foreach (var designeritem in ThemeItems.Concat(new[] { PreviousPageItem,NextPageItem,CancelItem }))
            {
                if (designeritem is MenuStateTextMenuItem mstmi)
                {
                    mstmi.FontFace = FontSrc.FontFamily.Name;
                    mstmi.FontSize = FontSrc.Size;
                    if (designeritem == SelectedNominoThemeItem)
                    {
                        base.SelectedIndex = MenuElements.Count - 1;
                    }
                }
            }

            Pages = Partition((from t in ThemeItems select (MenuStateMenuItem)t).ToList(), MaxPerPage).ToList();
            PreparePage(CurrentPage);
            base.MenuItemActivated += ThemeSelectionMenuState_MenuItemActivated;


           
            
        }
        private MenuStateTextMenuItem PreviousPageItem = null;
        private MenuStateTextMenuItem NextPageItem = null;
        private MenuStateTextMenuItem CancelItem = null;
        private void ThemeSelectionMenuState_MenuItemActivated(object sender, MenuStateMenuItemActivatedEventArgs e)
        {

            if (e.MenuElement.Tag is MenuStateThemeSelection)
            {
                MenuStateThemeSelection cast = e.MenuElement.Tag as MenuStateThemeSelection;
                var resulttheme = cast.GenerateThemeFunc();
                ThemeChosen(resulttheme);
                IsDirty = true;
                base.ActivatedItem = null;
            }
            else if (e.MenuElement == PreviousPageItem)
            {
                CurrentPage--;
                PreparePage(CurrentPage);
            }
            else if (e.MenuElement == NextPageItem)
            {
                CurrentPage++;
                PreparePage(CurrentPage);
            }
            else if (e.MenuElement == CancelItem)
            {
                _Owner.CurrentState = RevertState;
                base.ActivatedItem = null;
            }


        }

        private void PreparePage(int PageNumber)
        {
            var CurrentPageItems = Pages[PageNumber];
            IEnumerable<MenuStateMenuItem> ItemsToAdd = CurrentPageItems.Prepend(CancelItem) ;
            if (PageNumber > 0)
                ItemsToAdd = ItemsToAdd.Prepend(PreviousPageItem);

            if (PageNumber < Pages.Count - 1)
                ItemsToAdd = ItemsToAdd.Append(NextPageItem);

            MenuElements = ItemsToAdd.ToList();
            SelectedIndex = 0;
            ActivatedItem = null;
            IsDirty = true;


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
