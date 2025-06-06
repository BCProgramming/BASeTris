﻿using BASeTris.AI;
using BASeTris.BackgroundDrawers;
using BASeTris.GameStates.GameHandlers;
using BASeTris.Settings;
using BASeTris.Tetrominoes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

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
            //var HighScoresItem = new MenuStateTextMenuItem() { Text = "High Scores" ,TipText="View High scores"};
            var testslider = new MenuStateSliderOption(0, 100, 50) { Label = "Slider of Excitement" };
            var Controls = new MenuStateTextMenuItem() { Text = "Controls", TipText = "Display control settings." };
            var BGDesign = new MenuStateTextMenuItem() { Text = "Design", TipText = "Background Designer. Why does this exist?" };
            var CrappyThemeTestthing = new MenuStateTextMenuItem() { Text = "Theme shit", TipText = "Crappy item for testing new theme menu. ignore me" };
            var ExitItem = new ConfirmedTextMenuItem() { Text = "Quit",TipText="Quit to DOS. Haha, just kidding." };
            var Replays = new MenuStateTextMenuItem() { Text = "Replays", TipText = "Show Replays" };
            var TestTextEdit = new MenuStateTextInputMenuItem() { Text = "EDITABLE!", TipText = "Test Editing text.", Label="Editable Item"};


            ExitItem.OnOptionConfirmed += (a, b) =>
            {
                
                if(pOwner is BASeTrisTK)
                {
                    ((BASeTrisTK)pOwner).Close();
                }
                else if(pOwner is BASeTris)
                {
                    ((BASeTris)pOwner).Close();
                }
            };

            
            MenuStateTextMenuItem CreditsMenu = null;
            var FontSrc = TetrisGame.GetRetroFont(14, pOwner.ScaleFactor);

            var RealHighScoresItem = new MenuStateHighScoreItem(pOwner, Target, FontSrc) {Text="High Scores",TipText="Show High Scores" };


            Target.MenuItemActivated += (o, e) =>
            {
                if (e.MenuElement == Replays)
                {
                    GenericMenuState gms = new GenericMenuState(Target.BG, pOwner, new ReplayMenuPopulator(Target));
                    pOwner.CurrentState = MenuState.CreateOutroState(pOwner, pOwner.CurrentState, gms);
                    Target.ActivatedItem = null;
                }
                else if (e.MenuElement == NewGameItem)
                {
                    GenericMenuState gms = new GenericMenuState(Target.BG, pOwner, new NewGameMenuPopulator(Target));
                    pOwner.CurrentState = MenuState.CreateOutroState(pOwner, pOwner.CurrentState, gms);
                    Target.ActivatedItem = null;
                }

                else if (e.MenuElement == OptionsItem)
                {
                    //Show the options menu
                    //var OptionsMenu = new OptionsMenuState(Target.BG, pOwner, pOwner.CurrentState); // GenericMenuState(Target.BG, pOwner, new OptionsMenuPopulator());
                    var OptionsMenu = new OptionsMenuSettingsSelectorState(Target.BG, pOwner, pOwner.CurrentState);
                    pOwner.CurrentState = MenuState.CreateOutroState(pOwner, OptionsMenu);
                    Target.ActivatedItem = null;
                }
                else if (e.MenuElement == Controls)
                {
                    var ControlsState = new ControlSettingsViewState(pOwner.CurrentState, pOwner.Settings, ControlSettingsViewState.ControllerSettingType.Gamepad);
                    ControlsState.BG = pOwner.CurrentState.BG;

                    //TransitionState tstatePixelate = new TransitionState_Pixelate(pOwner.CurrentState, ControlsState, new TimeSpan(0, 0, 0, 0, 1750)) { GameProcDelegationMode = TransitionState.DelegateProcConstants.Delegate_Previous, SnapshotSettings = TransitionState.SnapshotConstants.Snapshot_Both };
                    var tstate = TransitionState.GetRandomTransitionState(pOwner.CurrentState,ControlsState,TransitionState.StandardTransitionLength);


                    pOwner.CurrentState = tstate; //MenuState.CreateOutroState(pOwner, ControlsState);
                    Target.ActivatedItem = null;
                }
                else if (e.MenuElement == BGDesign)
                {
                    var DesignState = new DesignBackgroundState(pOwner, pOwner.CurrentState, null);
                    pOwner.CurrentState = MenuState.CreateOutroState(pOwner, DesignState);
                    Target.ActivatedItem = null;
                }
                else if (e.MenuElement == ExitItem)
                {
                    //nothing, this needs confirmation so is handled separate.
                }
                else if (e.MenuElement == CrappyThemeTestthing)
                {
                    ThemeSelectionMenuState themestate = new ThemeSelectionMenuState(pOwner, pOwner.CurrentState.BG, pOwner.CurrentState, typeof(StandardTetrisHandler), typeof(SNESTetrominoTheme), (nt) => { });
                    pOwner.CurrentState = MenuState.CreateOutroState(pOwner, themestate);
                    Target.ActivatedItem = null;
                }
                /*else if (e.MenuElement == TestReplay)
                {
                    if (pOwner is IGamePresenter igp)
                    {
                        var statefunc = igp.GetPresenter().ReplayStateCreator(new GameReplayOptions() { Settings = pOwner.Settings, GameplayRecord = GameplayRecord.GetDrunkRecording(new TimeSpan(0, 10, 0)) });
                        pOwner.CurrentState = statefunc();
                        Target.ActivatedItem = null;
                    }


                }*/



            };



            
            Target.HeaderTypeface = FontSrc.FontFamily.Name;
            Target.HeaderTypeSize = (float)(28f*pOwner.ScaleFactor);
            foreach(var iterate in new [] { NewGameItem,OptionsItem,scaleitem,Controls,RealHighScoresItem, BGDesign,Replays, ExitItem})
            {
                iterate.FontFace = FontSrc.FontFamily.Name;
                iterate.FontSize = FontSrc.Size;
                Target.MenuElements.Add(iterate);
            }
            //Target.MenuElements.Add(testslider);
        }
    }
    public class HandlerSelectionPopulator : IMenuPopulator
    {
        private GameState RevertState = null;
        String DesiredCategory = null;
        public delegate bool HandlerPredicate(Type HandlerType);
        public delegate void HandlerAction(GenericMenuState Target, IStateOwner pOwner, IBlockGameCustomizationHandler useHandler);
        protected HandlerAction HandlerChosenAction = null;
        protected HandlerPredicate AllowHandler = null;
        public HandlerSelectionPopulator(GameState ReversionState,String Category = null,HandlerAction pHandlerChosenAction = null)
        {
            HandlerChosenAction = pHandlerChosenAction;
            RevertState = ReversionState;
            DesiredCategory = Category;
        }

        public virtual void PopulateMenu(GenericMenuState Target, IStateOwner pOwner)
        {
            if (DesiredCategory == null)
                Target.StateHeader = "Select Game Type";
            else
                Target.StateHeader = "Game Type - " + DesiredCategory;
            
            Dictionary<String, List<Type>> FoundHandlerCategories = new Dictionary<string, List<Type>>();
            //var NewGameItem = new MenuStateTextMenuItem() { Text = "New Game" };
            List<MenuStateMenuItem> AllItems = new List<MenuStateMenuItem>();
            List<MenuStateMenuItem> CategoryItems = new List<MenuStateMenuItem>();
            Dictionary<MenuStateMenuItem, IBlockGameCustomizationHandler> HandlerLookup = new Dictionary<MenuStateMenuItem, IBlockGameCustomizationHandler>();
            var BackItem = new MenuStateTextMenuItem() { Text = "Back" };
            foreach(var iterate in Program.GetGameHandlers())
            {
                
                var FindAttribute = iterate.GetCustomAttribute(typeof(HandlerMenuCategoryAttribute), true) as HandlerMenuCategoryAttribute;
                var FindTipAttribute = iterate.GetCustomAttribute(typeof(HandlerTipTextAttribute), false) as HandlerTipTextAttribute;
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
                    if (AllowHandler != null && !AllowHandler(iterate))
                    {
                        ;
                    }
                    else
                    {
                        IBlockGameCustomizationHandler handler = (IBlockGameCustomizationHandler)ci.Invoke(new object[] { });
                        MenuStateTextMenuItem builditem = new MenuStateTextMenuItem() { Text = handler.Name, TipText = (FindTipAttribute?.TipText) ?? "" };
                        HandlerLookup.Add(builditem, handler);
                        AllItems.Add(builditem);
                    }
                }
            }
            if (DesiredCategory == null)
            {
                foreach(var iterate in FoundHandlerCategories)
                {
                    MenuStateTextMenuItem buildcategoryitem = new MenuStateTextMenuItem { Text = iterate.Key + ">>",Tag = iterate.Key};
                    buildcategoryitem.MenuFlags = MenuStateTextMenuItem.AdditionalMenuFlags.MenuFlags_ShowSubmenuArrow;
                    buildcategoryitem.BackColor = Color.FromArgb(128,Color.Yellow);
                    AllItems.Add(buildcategoryitem);
                    CategoryItems.Add(buildcategoryitem);
                 }
            }
            AllItems.Add(BackItem);

           

           

            Target.MenuItemActivated += (o, e) =>
            {
                if (HandlerLookup.ContainsKey(e.MenuElement))
                {


                    IBlockGameCustomizationHandler usehandler = HandlerLookup[e.MenuElement];
                    HandlerChosenAction(Target,pOwner, usehandler);
                    /*
                    var StartGameFunc = () =>
                    {
                        if (pOwner is IGamePresenter igp)
                        {
                            

                            var NewGameState = new GameplayGameState(pOwner, usehandler, null, TetrisGame.Soundman, Target.PrimaryMenu);
                            //TransitionState ts = new TransitionState_Pixelate(pOwner.CurrentState, NewGameState, new TimeSpan(0, 0, 0, 0, 10000)) { GameProcDelegationMode = TransitionState.DelegateProcConstants.Delegate_None, SnapshotSettings = TransitionState.SnapshotConstants.Snapshot_Both };
                            var ts = TransitionState.GetRandomTransitionState(pOwner.CurrentState, NewGameState, TransitionState.StandardTransitionLength);
                            Target.BackgroundMusicKey = null;
                            pOwner.CurrentState = ts;
                            igp.StartGame();
                        }
                    };
                    GamePreparerAttribute gpa = GamePreparerAttribute.HasPreparerAttribute(usehandler.GetType());
                    if (usehandler is IPreparableGame ipg && gpa != null)
                    {
                        //we want to create a submenu for the options; the starting function should call the initialization function on usehandler and then call StartGameFunc.
                        GamePreparerOptions InitializeOption = (GamePreparerOptions)Activator.CreateInstance(gpa.PreparerOptionsType, new[] { usehandler.GetType() });

                        var createstate = GamePreparerOptions.ConstructPreparationState(pOwner, usehandler.Name, Target, Target.BG, "Return", InitializeOption, (gpo) =>
                        {
                            ipg.SetPrepData(InitializeOption);
                            StartGameFunc();
                        });
                        Target.ActivatedItem = null;

                        pOwner.CurrentState = MenuState.CreateOutroState(pOwner,createstate);


                    }
                    else
                    {
                        StartGameFunc();
                    }
                    */
                }
                else if (CategoryItems.Contains(e.MenuElement))
                {
                    GenericMenuState gms = new GenericMenuState(Target.BG, pOwner, new HandlerSelectionPopulator(Target,(String)(((MenuStateTextMenuItem)e.MenuElement).Tag),HandlerChosenAction));
                    pOwner.CurrentState = MenuState.CreateOutroState(pOwner,gms);
                    Target.ActivatedItem = null;


                }
                else if (e.MenuElement == BackItem)
                {
                    pOwner.CurrentState = MenuState.CreateOutroState(pOwner, RevertState);
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
    public class ReplayMenuPopulator : HandlerSelectionPopulator
    {
        
        private class ReplayMenuItem
        {
            public String Text;
            public String FullPath;
            public ReplayMenuItem(String pText, String pFullPath)
            {
                Text = pText;
                FullPath = pFullPath;
            }
            public override string ToString()
            {
                return Text;
            }
        }
        protected Dictionary<Type, List<String>> TypeReplayFiles = new Dictionary<Type, List<string>>();
       
        public ReplayMenuPopulator(GameState ReversionState, String Category = null) : base(ReversionState, Category)
        {
            var allHandlers = Program.GetGameHandlers();
            foreach (var iterateHandler in allHandlers)
            {
                List<String> Replays = GameplayRecord.GetHandlerReplayFiles(iterateHandler).ToList();
                if(Replays.Count>0)
                    TypeReplayFiles.Add(iterateHandler, Replays);
            }

            base.AllowHandler = (h) =>
            {
                return TypeReplayFiles.ContainsKey(h);
            };

            base.HandlerChosenAction = (Target, pOwner, useHandler) =>
            {
                //when a handler is chosen, we want to show a list of the replays to choose from.

                //we use the SimpleMenuPopulator here, using a Tuple for the objects.


                var BuildList = TypeReplayFiles[useHandler.GetType()].Select((r) => new ReplayMenuItem(Path.GetFileNameWithoutExtension(r), r));

                List<Object> ObjectList = new List<object>(BuildList);


                GenericMenuState SubReplayListing = new GenericMenuState(Target.BG, pOwner, new SimpleMenuPopulator(Target, ObjectList, (o) =>
                {
                    //load the replay data, and then play the replay...
                    ReplayMenuItem rmi = o as ReplayMenuItem;
                    GameplayRecord gpr = GameplayRecord.ReadFromFile(pOwner,rmi.FullPath,out var _GeneratedMinos);
                    if (pOwner is IGamePresenter igp)
                    {

                        var statefunc = igp.GetPresenter().ReplayStateCreator(new GameReplayOptions() { Settings = pOwner.Settings, GameplayRecord = gpr,GeneratedMinos = _GeneratedMinos });
                        pOwner.CurrentState = statefunc();
                        Target.ActivatedItem = null;
                    }


                }));
                SubReplayListing.StateHeader = "Replays - " + useHandler.Name;

                pOwner.CurrentState = SubReplayListing;
                Target.ActivatedItem = null;

                

            };
        }
        
    }

    

    public class NewGameMenuPopulator : HandlerSelectionPopulator
    {
        public NewGameMenuPopulator(GameState ReversionState, String Category = null) : base(ReversionState, Category)
        {
            //for a new game when an item is selected we want to start a new game. Well, I suppose that ought to be obvious.
            base.HandlerChosenAction = (Target, pOwner, usehandler) =>
            {
                var StartGameFunc = () =>
                    {
                        if (pOwner is IGamePresenter igp)
                        {


                            var NewGameState = new GameplayGameState(pOwner, usehandler, null, TetrisGame.Soundman, Target.PrimaryMenu);
                            //TransitionState ts = new TransitionState_Pixelate(pOwner.CurrentState, NewGameState, new TimeSpan(0, 0, 0, 0, 10000)) { GameProcDelegationMode = TransitionState.DelegateProcConstants.Delegate_None, SnapshotSettings = TransitionState.SnapshotConstants.Snapshot_Both };
                            var ts = TransitionState.GetRandomTransitionState(pOwner.CurrentState, NewGameState, TransitionState.StandardTransitionLength);
                            Target.BackgroundMusicKey = null;
                            pOwner.CurrentState = ts;
                            igp.StartGame();
                        }
                    };
                GamePreparerAttribute gpa = GamePreparerAttribute.HasPreparerAttribute(usehandler.GetType());
                if (usehandler is IPreparableGame ipg && gpa != null)
                {
                    //we want to create a submenu for the options; the starting function should call the initialization function on usehandler and then call StartGameFunc.
                    GamePreparerOptions InitializeOption = (GamePreparerOptions)Activator.CreateInstance(gpa.PreparerOptionsType, new[] { usehandler.GetType() });
                    var createstate = GamePreparerOptions.ConstructPreparationState(pOwner, usehandler.Name, Target, Target.BG, "Return", InitializeOption, (gpo) =>
                    {
                        ipg.SetPrepData(InitializeOption);
                        StartGameFunc();
                    });
                    Target.ActivatedItem = null;

                    pOwner.CurrentState = MenuState.CreateOutroState(pOwner, createstate);


                }
                else
                {
                    StartGameFunc();
                }
            };
        }
    }



    public class SimpleMenuPopulator : IMenuPopulator
    {
        private List<Object> MenuItems = new List<object>();
        private Dictionary<MenuStateMenuItem, Object> ItemObjects = new Dictionary<MenuStateMenuItem, object>();
        Action<Object> ItemChosen = null;
        private GameState ReversionState = null;
        public SimpleMenuPopulator(GameState pReversionState, List<Object> Items,Action<Object> OnSelectedItem)
        {
            ReversionState = pReversionState;
            ItemChosen = OnSelectedItem;
            MenuItems.AddRange(Items);
            
        }
        public void PopulateMenu(GenericMenuState Target, IStateOwner pOwner)
        {

            var kvpset = MenuItems.Select((i) => (new MenuStateTextMenuItem() { Text = i.ToString() }, i));
            var ReturnItem = new MenuStateTextMenuItem() { Text = "Return" };
            var FontSrc = TetrisGame.GetRetroFont(20, 1.0f);

            ReturnItem.FontFace = FontSrc.FontFamily.Name;
            ReturnItem.FontSize = FontSrc.Size;

            foreach (var k in kvpset)
            {
                k.Item1.FontFace = FontSrc.FontFamily.Name;
                k.Item1.FontSize = FontSrc.Size;


                ItemObjects.Add(k.Item1, k.Item2);
                Target.MenuElements.Add(k.Item1);

            }
            Target.MenuElements.Add(ReturnItem);

            Target.MenuItemActivated += (o, e) =>
                {
                    if (e.MenuElement == ReturnItem)
                    {
                        e.Owner.CurrentState = ReversionState;
                    }
                    else
                    {
                        if (ItemObjects.ContainsKey(e.MenuElement))
                        {
                            ItemChosen(ItemObjects[e.MenuElement]);
                        }
                    }
                };

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
                    ((BASeTrisTK)pOwner).Close();
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
                    pOwner.CurrentState = MenuState.CreateOutroState(pOwner,scorestate);
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
