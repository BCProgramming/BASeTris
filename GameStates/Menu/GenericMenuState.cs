using BASeTris.BackgroundDrawers;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public GameState PreviousState = null; //may not be needed (or useful) depending largely on if there was a previous state or if we ever want to go back to it.
                                               //whatever is calling to set the state could blank it out as it should know if it is relevant or not.

        private GameState.DisplayMode _DisplayMode = GameState.DisplayMode.Full;

        public override DisplayMode SupportedDisplayMode 
            {get { return _DisplayMode;} }
        public void SetDisplayMode(DisplayMode src)
        {
            _DisplayMode = src;
        }
        public GenericMenuState(IBackground pBG, IStateOwner pOwner,IMenuPopulator Populator):base(pBG)
        {
            
            StateHeader = "";
            PreviousState = pOwner.CurrentState;
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
            
            var NewGameItem = new MenuStateTextMenuItem() { Text = "New Game" };
            var OptionsItem = new MenuStateTextMenuItem() { Text = "Options" };
            var scaleitem = new MenuStateScaleMenuItem(pOwner);
            var HighScoresItem = new MenuStateTextMenuItem() { Text = "High Scores" };
            var ExitItem = new ConfirmedTextMenuItem() { Text = "Quit" };
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
            Target.MenuItemActivated += (o, e) =>
            {
                if(e.MenuElement == NewGameItem)
                {
                    //start a new game.
                    if(pOwner is IGamePresenter igp)
                    {
                        igp.StartGame();
                    }
                }
                if(e.MenuElement == OptionsItem)
                {
                    //Show the options menu
                    GenericMenuState OptionsMenu = new GenericMenuState(Target.BG, pOwner, new OptionsMenuPopulator());
                }
                if(e.MenuElement == HighScoresItem)
                {
                    ShowHighScoresState scorestate = new ShowHighScoresState(TetrisGame.ScoreMan["Standard"], Target, null);
                    pOwner.CurrentState = scorestate;
                    Target.ActivatedItem = null;
                }
                if(e.MenuElement == ExitItem)
                {
                    //nothing, this needs confirmation so is handled separate.
                }
            

            };



            var FontSrc = TetrisGame.GetRetroFont(14, 1.0f);
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



            var FontSrc = TetrisGame.GetRetroFont(14, 1.0f);
            foreach (var iterate in new[] { NewGameItem, OptionsItem, HighScoresItem, ExitItem })
            {
                iterate.FontFace = FontSrc.FontFamily.Name;
                iterate.FontSize = FontSrc.Size;
                Target.MenuElements.Add(iterate);
            }

        }
    }




}
