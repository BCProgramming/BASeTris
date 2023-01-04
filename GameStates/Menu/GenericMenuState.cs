using BASeTris.BackgroundDrawers;
using BASeTris.GameStates.GameHandlers;
using BASeTris.Tetrominoes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.Menu
{
    /// <summary>
    /// base class that provides a little bit of pipework so that menu class implementations can be created more easily. Menus can be defined as IMenuPopulator implementations and passed to any GenericMenuState
    /// implementation. Current thought is to allow for several such implementations for different contexts (Menus being shown over top of gameplay, menus nested within other menus while showing the other menus underneath, that sort of thing)
    /// Some of those ideas of course need their own special drawing logic so will need double implementation for the GDI+ and Skia drawing code.
    /// </summary>
    public class GenericMenuState : MenuState
    {
        public GameState ParentState = null; //may not be needed (or useful) depending largely on if there was a previous state or if we ever want to go back to it.
                                               //whatever is calling to set the state could blank it out as it should know if it is relevant or not.

        private GameState.DisplayMode _DisplayMode = GameState.DisplayMode.Full;

        public GenericMenuState PrimaryMenu { get
            {
                var copied = ParentState;

                return (ParentState as GenericMenuState)?.PrimaryMenu ?? this;
                


            } }
        public override DisplayMode SupportedDisplayMode 
            {get { return _DisplayMode;} }
        public void SetDisplayMode(DisplayMode src)
        {
            _DisplayMode = src;
        }
        public GenericMenuState(IBackground pBG, IStateOwner pOwner,IMenuPopulator Populator):base(pBG)
        {
            
            StateHeader = "";
            ParentState = pOwner.CurrentState;
            Populator.PopulateMenu(this, pOwner);
            
        }
        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {

            base.HandleGameKey(pOwner, g);
        }

    }
    public interface IMenuPopulator
    {
        void PopulateMenu(GenericMenuState Target, IStateOwner pOwner);
    }

    /// <summary>
    /// Title Menu- New Game, Options, High Scores, Quit
    /// </summary>
    public class TitleMenuPopulator : IMenuPopulator
    {
    
        public void PopulateMenu(GenericMenuState Target,IStateOwner pOwner)
        {
            Target.StateHeader = "BASeTris";

            //var NewGameItem = new MenuStateTextMenuItem() { Text = "New Game" };
            var NewGameItem = new MenuStateTextMenuItem() { Text = "New Game",TipText="I feel the menu text is already sufficiently descriptive." };
            var OptionsItem = new MenuStateTextMenuItem() { Text = "Options",TipText="Adjust various options."};
            var scaleitem = new MenuStateScaleMenuItem(pOwner) { TipText = "Change Scaling" };
            var HighScoresItem = new MenuStateTextMenuItem() { Text = "High Scores" ,TipText="View High scores"};
            var ExitItem = new ConfirmedTextMenuItem() { Text = "Quit",TipText="Quit to DOS. Haha, just kidding." };
            ExitItem.OnOptionConfirmed += (a, b) =>
            {
                
                if(pOwner is BASeTrisTK)
                {
                    ((BASeTrisTK)pOwner).Exit();
                }
                else if(pOwner is BASeTris)
                {
                    ((BASeTris)pOwner).Close();
                }
            };
            
            /*NewGameItem.OnDeactivateOption += (o, eventarg) =>
             {
                 //start a new game.


                 if (pOwner is IGamePresenter igp)
                 {
                     IGameCustomizationHandler Handler = (eventarg.Option.Handler);
                     if (Handler != null)
                     {
                         //IGameCustomizationHandler Handler = DrMarioGame ? (IGameCustomizationHandler)new DrMarioHandler() : (IGameCustomizationHandler)new StandardTetrisHandler();
                         pOwner.CurrentState = new GameplayGameState(Handler, null, TetrisGame.Soundman);

                         igp.StartGame();
                     }
                     else
                     {
                         NewGameItem.Reset();
                     }
                 }
             };
            NewGameItem.OnActivateOption += (o, eventarg) =>
            {
                
            };
            NewGameItem.OnChangeOption += (o2, eventarg2) =>
            {
                //nothing for when we change the option.
            };*/
            
            Target.MenuItemActivated += (o, e) =>
            {
                if(e.MenuElement==NewGameItem)
                {
                    GenericMenuState gms = new GenericMenuState(Target.BG, pOwner, new NewGameMenuPopulator(Target));
                    pOwner.CurrentState = gms;
                    Target.ActivatedItem = null;
                }
               
                else if(e.MenuElement == OptionsItem)
                {
                    //Show the options menu
                    //var OptionsMenu = new OptionsMenuState(Target.BG, pOwner, pOwner.CurrentState); // GenericMenuState(Target.BG, pOwner, new OptionsMenuPopulator());
                    var OptionsMenu = new OptionsMenuSettingsSelectorState(Target.BG, pOwner, pOwner.CurrentState);
                    pOwner.CurrentState = OptionsMenu;
                    Target.ActivatedItem = null;
                }
                else if(e.MenuElement == HighScoresItem)
                {
                    ShowHighScoresState scorestate = new ShowHighScoresState(TetrisGame.ScoreMan["Standard"], Target, null);
                    pOwner.CurrentState = scorestate;
                    Target.ActivatedItem = null;
                }
                else if(e.MenuElement == ExitItem)
                {
                    //nothing, this needs confirmation so is handled separate.
                }
            

            };



            var FontSrc = TetrisGame.GetRetroFont(14, pOwner.ScaleFactor);
            Target.HeaderTypeface = FontSrc.FontFamily.Name;
            Target.HeaderTypeSize = (float)(28f*pOwner.ScaleFactor);
            foreach(var iterate in new [] { NewGameItem,OptionsItem,scaleitem,HighScoresItem,ExitItem})
            {
                iterate.FontFace = FontSrc.FontFamily.Name;
                iterate.FontSize = FontSrc.Size;
                Target.MenuElements.Add(iterate);
            }
     
        }
    }

    public class NewGameMenuPopulator : IMenuPopulator
    {
        private GameState RevertState = null;
        String DesiredCategory = null;
        public NewGameMenuPopulator(GameState ReversionState,String Category = null)
        {
            RevertState = ReversionState;
            DesiredCategory = Category;
        }

        public void PopulateMenu(GenericMenuState Target, IStateOwner pOwner)
        {
            if (DesiredCategory == null)
                Target.StateHeader = "Select Game Type";
            else
                Target.StateHeader = "Game Type - " + DesiredCategory;
            {
            }
            Dictionary<String, List<Type>> FoundHandlerCategories = new Dictionary<string, List<Type>>();
            //var NewGameItem = new MenuStateTextMenuItem() { Text = "New Game" };
            List<MenuStateMenuItem> AllItems = new List<MenuStateMenuItem>();
            List<MenuStateMenuItem> CategoryItems = new List<MenuStateMenuItem>();
            Dictionary<MenuStateMenuItem, IGameCustomizationHandler> HandlerLookup = new Dictionary<MenuStateMenuItem, IGameCustomizationHandler>();
            var BackItem = new MenuStateTextMenuItem() { Text = "Back to Main" };
            foreach(var iterate in Program.GetGameHandlers())
            {
                var FindAttribute = iterate.GetCustomAttribute(typeof(HandlerMenuCategoryAttribute), true) as HandlerMenuCategoryAttribute;

                //if Category is null, we only want to show the items that have no category.
                //if it is not null, then the category needs to match the category we were told to show.
                if (FindAttribute != null)
                {
                    if (!FoundHandlerCategories.ContainsKey(FindAttribute.Category))
                        FoundHandlerCategories[FindAttribute.Category] = new List<Type>();

                    FoundHandlerCategories[FindAttribute.Category].Add(iterate);
                }

                if (FindAttribute == null && DesiredCategory == null)
                {
                    //good to go...
                }
                else if (FindAttribute != null && DesiredCategory == null)
                {
                    continue;
                }
                else if (FindAttribute != null && DesiredCategory != null)
                {
                    if (String.Equals(FindAttribute.Category, DesiredCategory, StringComparison.OrdinalIgnoreCase))
                    {
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (FindAttribute == null && DesiredCategory != null)
                    continue;

                ConstructorInfo ci = iterate.GetConstructor(new Type[] { });
                if(ci!=null)
                {
                    IGameCustomizationHandler handler = (IGameCustomizationHandler)ci.Invoke(new object[] { });
                    MenuStateTextMenuItem builditem = new MenuStateTextMenuItem() { Text = handler.Name };
                    HandlerLookup.Add(builditem, handler);
                    AllItems.Add(builditem);
                }
            }
            if (DesiredCategory == null)
            {
                foreach(var iterate in FoundHandlerCategories)
                {
                    MenuStateTextMenuItem buildcategoryitem = new MenuStateTextMenuItem { Text = iterate.Key + ">>",Tag = iterate.Key};
                    buildcategoryitem.BackColor = Color.Yellow;
                    AllItems.Add(buildcategoryitem);
                    CategoryItems.Add(buildcategoryitem);
                 }
            }
            AllItems.Add(BackItem);

           

           

            Target.MenuItemActivated += (o, e) =>
            {
                if (HandlerLookup.ContainsKey(e.MenuElement))
                {
                    IGameCustomizationHandler usehandler = HandlerLookup[e.MenuElement];
                    if (pOwner is IGamePresenter igp)
                    {


                        pOwner.CurrentState = new GameplayGameState(usehandler, null, TetrisGame.Soundman, Target.PrimaryMenu);

                        igp.StartGame();
                    }
                }
                else if (CategoryItems.Contains(e.MenuElement))
                {
                    GenericMenuState gms = new GenericMenuState(Target.BG, pOwner, new NewGameMenuPopulator(Target,(String)(((MenuStateTextMenuItem)e.MenuElement).Tag)));
                    pOwner.CurrentState = gms;
                    Target.ActivatedItem = null;


                }
                else if (e.MenuElement == BackItem)
                {
                    pOwner.CurrentState = RevertState;
                }




            };



            var FontSrc = TetrisGame.GetRetroFont(14, pOwner.ScaleFactor);
            Target.HeaderTypeface = FontSrc.FontFamily.Name;
            Target.HeaderTypeSize = (float)(28f * pOwner.ScaleFactor);
            foreach (var iterate in AllItems)
            {
                if (iterate is MenuStateTextMenuItem titem)
                {
                    titem.FontFace = FontSrc.FontFamily.Name;
                    titem.FontSize = FontSrc.Size;
                }
                Target.MenuElements.Add(iterate);
            }

        }
    }


    public class OptionsMenuPopulator : IMenuPopulator
    {
        //right now a copy of the title menu...

        public void PopulateMenu(GenericMenuState Target, IStateOwner pOwner)
        {

            var NewGameItem = new MenuStateTextMenuItem() { Text = "New Game" };
            var OptionsItem = new MenuStateTextMenuItem() { Text = "Options" };
            var HighScoresItem = new MenuStateTextMenuItem() { Text = "High Scores" };
            var ExitItem = new ConfirmedTextMenuItem() { Text = "Quit" };
            ExitItem.OnOptionConfirmed += (a, b) =>
            {

                if (pOwner is BASeTrisTK)
                {
                    ((BASeTrisTK)pOwner).Exit();
                }
                else if (pOwner is BASeTris)
                {
                    ((BASeTris)pOwner).Close();
                }
            };
            Target.MenuItemActivated += (o, e) =>
            {
                if (e.MenuElement == NewGameItem)
                {
                    //start a new game.
                    if (pOwner is IGamePresenter igp)
                    {
                        igp.StartGame();
                    }
                }
                if (e.MenuElement == OptionsItem)
                {
                    //Show the options menu
                }
                if (e.MenuElement == HighScoresItem)
                {
                    ShowHighScoresState scorestate = new ShowHighScoresState(TetrisGame.ScoreMan["Standard"], Target, null);
                    pOwner.CurrentState = scorestate;
                    Target.ActivatedItem = null;
                }
                if (e.MenuElement == ExitItem)
                {
                    //nothing, this needs confirmation so is handled separate.
                }


            };



            var FontSrc = TetrisGame.GetRetroFont(20, 1.0f);
            foreach (var iterate in new[] { NewGameItem, OptionsItem, HighScoresItem, ExitItem })
            {
                iterate.FontFace = FontSrc.FontFamily.Name;
                iterate.FontSize = FontSrc.Size;
                Target.MenuElements.Add(iterate);
            }

        }
    }




}
