using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BASeTris;
using BASeTris.BackgroundDrawers;
using BASeTris.Rendering.Adapters;
using SkiaSharp;

namespace BASeTris.GameStates.Menu
{
    public class MenuState : GameState,IMouseInputState
    {
        public class MenuStateFadedParentStateInformation
        {
            public MenuStateFadedParentStateInformation(GameState pFadedParent, bool pRunGameProc)
            {
                FadedParentState = pFadedParent;
                RunGameProc = pRunGameProc;
            }
            public GameState FadedParentState { get; set; } = null;
            public bool RunGameProc { get; set; } = false;
        }
        //The "Menu" state presents a Menu. Well, I mean, obviously.

        //Draws a stack of "menu" Items
        //Above the stack is drawn some sort of "Header" Image. (or text).
        //One item can be selected at a time. Moving up and down moves the selection.
        //Pressing Enter will "Activate" the item. This will trigger it's action. For most items this will perform some action which will change to another state, for example.

        public MenuStateFadedParentStateInformation FadedBGFadeState = null;

        public event EventHandler<MenuStateMenuItemActivatedEventArgs> MenuItemActivated;
        public event EventHandler<MenuStateMenuItemSelectedEventArgs> MenuItemSelected;
        public event EventHandler<MenuStateMenuItemSelectedEventArgs> MenuItemDeselected;

        public String BackgroundMusicKey = null;

        public static MenuState CreateMenu(IStateOwner pOwner,String pHeaderText, GameState ReversionState, IBackground usebg,String sCancelText, int PerPageItems = int.MaxValue,params MenuStateMenuItem[] Items)
        {
            MenuState ResultState = new MenuState(usebg ?? ReversionState.BG);
            
            var FontSrc = TetrisGame.GetRetroFont(14, pOwner.ScaleFactor);
            ResultState.StateHeader = pHeaderText;
            ResultState.HeaderTypeface = FontSrc.FontFamily.Name;
            ResultState.HeaderTypeSize = (float)(28f * pOwner.ScaleFactor);





            //ConfirmedTextMenuItem ReturnItem = new ConfirmedTextMenuItem() { Text = sCancelText??"", TipText = "Return" };
            MenuStateTextMenuItem ReturnItem = new MenuStateTextMenuItem() { Text = sCancelText ?? "Cancel", TipText = "Return" };
            ResultState.MenuItemActivated += (o1, e1) =>
            {
                if (e1.MenuElement == ReturnItem)
                {
                    if (ReversionState != null)
                    {
                        TetrisGame.Soundman.StopMusic();
                        pOwner.CurrentState = ReversionState;
                        ResultState.ActivatedItem = null;
                    }
                }
            };
            /*ReturnItem.OnOptionConfirmed += (o, e) =>
            {
                if (ReversionState != null)
                {
                    TetrisGame.Soundman.StopMusic();
                    pOwner.CurrentState = ReversionState;
                    ResultState.ActivatedItem = null;
                }
            };*/
            IEnumerable<MenuStateMenuItem> ThisPageItems;
            if (PerPageItems < Items.Length)
            {
                //we need to paginate, create a new Menuitem...
                
                MenuStateTextMenuItem NextPageItem = new MenuStateTextMenuItem() { Text = "Next Page>>", TipText = "Switch to the next page" };
                ResultState.MenuItemActivated += (o, e) =>
                {
                   

                    if (e.MenuElement == NextPageItem)
                    {
                        MenuState NextPageList = CreateMenu(pOwner, pHeaderText, ResultState, ResultState.BG, "Previous Page", PerPageItems, Items.Skip(PerPageItems).ToArray());
                        NextPageList.MenuItemActivated = ResultState.MenuItemActivated;
                        NextPageList.MenuItemDeselected = ResultState.MenuItemDeselected;
                        NextPageList.MenuItemSelected = ResultState.MenuItemSelected;
                        pOwner.CurrentState = NextPageList;
                        ResultState.ActivatedItem = null;
                        e.CancelActivation = true;

                    }
                };




            }
            else
            {
                ThisPageItems = Items.Prepend(ReturnItem);
            }


            foreach (var designeritem in sCancelText==null?Items:Items.Prepend(ReturnItem))
            {
                if (designeritem is MenuStateTextMenuItem mstmi)
                {
                    mstmi.FontFace = FontSrc.FontFamily.Name;
                    mstmi.FontSize = FontSrc.Size;
                    ResultState.MenuElements.Add(mstmi);
                }
                else
                {
                    ResultState.MenuElements.Add(designeritem);
                }
            }

            return ResultState;
        }


        public String StateHeader { get; set; }

        public String FooterText { get; set; } = "";
        public BCPoint LastMouseMovement { get; set; }

        public String HeaderTypeface { get; set; } = "Arial";
        public float HeaderTypeSize { get; set; } = 18;

        public float FooterTypeSize { get; set; } = 14;

        //Our menu elements.
        public List<MenuStateMenuItem> MenuElements = new List<MenuStateMenuItem>();
        public int MainXOffset;
        bool OffsetUsed = false;
        public int StartItemOffset = 0; //start drawing menu items at this index. This is
        //used to scroll the menu up and down.
        public int SelectedIndex = 0;

        

        public override GameState.DisplayMode SupportedDisplayMode
        {
            get { return GameState.DisplayMode.Full; }
        }

        

        public MenuState(IBackground pBG)
        {
            _BG = pBG;
        }
        public MenuState():this(null)
        {
            
        }
        public double AppearanceTransitionPercentage = 0;
        public Stopwatch AppearanceTransitionStopWatch = null;
        public static readonly long AppearanceTransitionLength = 400;
        public bool Rendered { get; set; } = false;
        
        public override void GameProc(IStateOwner pOwner)
        {

            if (!String.IsNullOrEmpty(BackgroundMusicKey))
            {
                var currentmusic = TetrisGame.Soundman.GetPlayingMusic();
                if (BackgroundMusicKey != TetrisGame.Soundman.scurrentPlayingMusic)
                {
                    TetrisGame.Soundman.PlayMusic(BackgroundMusicKey,0.5f,true );
                }

                pOwner.EnqueueAction(() =>
                {
                    if (pOwner.CurrentState != this && TetrisGame.Soundman.GetPlayingMusic() != null && TetrisGame.Soundman.scurrentPlayingMusic == BackgroundMusicKey)
                    {
                        TetrisGame.Soundman.StopMusic();
                    }

                });
            }


            if (FadedBGFadeState != null && FadedBGFadeState.FadedParentState != null)
            {
                FadedBGFadeState.FadedParentState.GameProc(pOwner);
            }
            if (AppearanceTransitionStopWatch == null && Rendered )
            {
                AppearanceTransitionStopWatch = new Stopwatch();
                AppearanceTransitionStopWatch.Start();
            }
            if (AppearanceTransitionStopWatch != null)
            {
                if (AppearanceTransitionStopWatch.IsRunning && AppearanceTransitionStopWatch.ElapsedMilliseconds > MenuState.AppearanceTransitionLength)
                {
                    AppearanceTransitionStopWatch.Stop();

                }
                AppearanceTransitionPercentage =  (double)MenuState.AppearanceTransitionLength / (double)AppearanceTransitionStopWatch.ElapsedMilliseconds;
                lock (MenuElements)
                {
                    foreach (var iterate in MenuElements)
                    {
                        iterate.TransitionPercentage = AppearanceTransitionPercentage;
                    }
                }
            }
            /*if (!OffsetUsed)
            {
                MainXOffset = -pOwner.GameArea.Width / 2;
                OffsetUsed = true;
            }
            if (MainXOffset < 0) MainXOffset += OffsetAnimationSpeed;
            OffsetAnimationSpeed += 3;*/
            //throw new NotImplementedException();

        }
        
       
        protected int GetPreviousIndex(int StartPos)
        {
            StartPos--;
            if (StartPos < 0)StartPos= MenuElements.Count - 1;

            return StartPos;
        }
        protected int GetNextIndex(int StartPos)
        {
            StartPos++;
            if (MenuElements.Count - 1 < StartPos)
            {
                StartPos = 0;
            }
            return StartPos;
        }
        public MenuStateMenuItem ActivatedItem = null;
        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            //handle up and down to change the currently selected menu item.
            //Other game keys we pass on to the currently selected item itself for additional handling.
            bool triggered = false;
            var OriginalIndex = SelectedIndex;
            if(g==GameKeys.GameKey_Down)
            {
                if (ActivatedItem != null)
                {
                    ActivatedItem.ProcessGameKey(pOwner, g);
                }
                else
                {
                    //move selected index upwards.

                    SelectedIndex = GetNextIndex(SelectedIndex);
                    while (!MenuElements[SelectedIndex].GetSelectable())
                        SelectedIndex = GetNextIndex(SelectedIndex);
                }
                triggered = true;
                //should also skip if disabled...
            }
            else if(g==GameKeys.GameKey_Drop)
            {
                if (ActivatedItem != null)
                {
                    ActivatedItem.ProcessGameKey(pOwner, g);
                }
                else
                { //move selected index downwards.
                    SelectedIndex = GetPreviousIndex(SelectedIndex);

                    while (!MenuElements[SelectedIndex].GetSelectable())
                        SelectedIndex = GetPreviousIndex(SelectedIndex);
                }
                triggered = true;
            }

            if(g==GameKeys.GameKey_RotateCW || g==GameKeys.GameKey_MenuActivate || g==GameKeys.GameKey_Pause)
            {
                if (ActivatedItem != null)
                {
                    TetrisGame.Soundman.PlaySound(pOwner.AudioThemeMan.MenuItemActivated.Key, pOwner.Settings.std.EffectVolume);
                    ActivatedItem.OnDeactivated(pOwner);
                    ActivatedItem = null;
                }
                else
                {

                    //Activate the currently selected item.
                    var currentitem = MenuElements[SelectedIndex];
                    TetrisGame.Soundman.PlaySound(pOwner.AudioThemeMan.MenuItemActivated.Key, pOwner.Settings.std.EffectVolume);
                    ActivatedItem = currentitem;
                    var args = new MenuStateMenuItemActivatedEventArgs(currentitem, pOwner);
                    MenuItemActivated?.Invoke(this, args);
                    if (!args.CancelActivation) 
                        currentitem.OnActivated(pOwner);
                }
                triggered = true;
            }

            else if (OriginalIndex != SelectedIndex)
            {
                TetrisGame.Soundman.PlaySound(pOwner.AudioThemeMan.MenuItemSelected.Key, pOwner.Settings.std.EffectVolume);
                
                var previousitem = MenuElements[OriginalIndex];
                var currentitem = MenuElements[SelectedIndex];
                MenuItemDeselected?.Invoke(this, new MenuStateMenuItemSelectedEventArgs(previousitem,pOwner));
                MenuItemSelected?.Invoke(this,new MenuStateMenuItemSelectedEventArgs(currentitem,pOwner));
                previousitem.OnDeselected(pOwner);
                currentitem.OnSelected(pOwner);
                FooterText = currentitem.TipText ?? "";
            }

            if(!triggered)
            {
                var currentitem = MenuElements[SelectedIndex];
                currentitem.ProcessGameKey(pOwner,g);
                
            }
            //throw new NotImplementedException();
        }
        private void TriggerSelection()
        {
            
       
        }

        MenuStateMenuItem MouseDownedItem = null;
        HashSet<StateMouseButtons> PressedButtons = new HashSet<StateMouseButtons>();
        public void MouseDown(IStateOwner pOwner,StateMouseButtons ButtonDown, BCPoint Position)
        {
            PressedButtons.Add(ButtonDown);
            //throw new NotImplementedException();
            var selecteditem = MenuElements[SelectedIndex];
            if (selecteditem.LastBounds.Contains(Position))
            {
                if (ButtonDown == StateMouseButtons.LButton)
                {
                    MouseDownedItem = selecteditem;
                }
            }
        }

        public void MouseUp(IStateOwner pOwner,StateMouseButtons ButtonUp, BCPoint Position)
        {
            try
            {
                if (MouseDownedItem != null)
                {
                    if (ButtonUp == StateMouseButtons.LButton)
                    {
                        if (MouseDownedItem.LastBounds.Contains(Position))
                        {
                            HandleGameKey(pOwner, GameKeys.GameKey_RotateCW);
                        }
                    }
                    else if (ButtonUp == StateMouseButtons.xButton1)
                    {
                        if (MouseDownedItem.LastBounds.Contains(Position))
                        {
                            HandleGameKey(pOwner, GameKeys.GameKey_Left);
                        }
                    }
                    else if (ButtonUp == StateMouseButtons.xButton2)
                    {
                        if (MouseDownedItem.LastBounds.Contains(Position))
                        {
                            HandleGameKey(pOwner, GameKeys.GameKey_Right);
                        }
                    }

                }
            }
            finally
            {
                PressedButtons.Remove(ButtonUp);
            }
            //throw new NotImplementedException();
        }

        public void MouseMove(IStateOwner pOwner,BCPoint Position)
        {
            if (this.ActivatedItem == null)
            {
                for (int i = 0; i < MenuElements.Count; i++)
                {
                    var checkitem = MenuElements[i];
                    if (checkitem.LastBounds.Contains(Position))
                    {
                        if (SelectedIndex != i)
                        {
                            var previouslyselecteditem = MenuElements[SelectedIndex];
                            MenuItemDeselected?.Invoke(this, new MenuStateMenuItemSelectedEventArgs(previouslyselecteditem, pOwner));
                            MenuItemSelected?.Invoke(this, new MenuStateMenuItemSelectedEventArgs(checkitem, pOwner));
                            previouslyselecteditem.OnDeselected(pOwner);
                            SelectedIndex = i;
                            checkitem.OnSelected(pOwner);
                            TetrisGame.Soundman.PlaySound(pOwner.AudioThemeMan.MenuItemSelected.Key, pOwner.Settings.std.EffectVolume);

                        }


                        SelectedIndex = i;
                    }
                }
            }
            LastMouseMovement = Position;
        }
    }
    public class MenuStateEventArgs : EventArgs
    {
        public IStateOwner Owner;
        public MenuStateMenuItem MenuElement;
        public MenuStateEventArgs(MenuStateMenuItem _Element,IStateOwner pOwner)
        {
            MenuElement = _Element;
            Owner = pOwner;
        }
    }
    
    public class MenuStateMenuItemActivatedEventArgs : MenuStateEventArgs
    {
        public bool CancelActivation = false;
        public MenuStateMenuItemActivatedEventArgs(MenuStateMenuItem _Element,IStateOwner pOwner):base(_Element,pOwner)
        {

        }
    }
    public class MenuStateMenuItemSelectedEventArgs : MenuStateEventArgs
    {
        public MenuStateMenuItemSelectedEventArgs(MenuStateMenuItem _Element,IStateOwner pOwner) : base(_Element,pOwner)
        {

        }
    }
}
