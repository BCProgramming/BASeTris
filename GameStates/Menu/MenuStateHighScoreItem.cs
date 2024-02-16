using BASeTris.BackgroundDrawers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.Menu
{
    public class MenuStateHighScoreItem : MenuStateTextMenuItem
    {
        private IStateOwner _Owner;
        private MenuState _State;
        private int EnterExitScoreCount = 0;
        private Font FontSrc;
        public MenuStateHighScoreItem(IStateOwner pOwner,MenuState ParentMenu,Font pFontSrc)
        {
            _Owner = pOwner;
            _State = ParentMenu;
            FontSrc = pFontSrc;
        }
        public override MenuEventResultConstants OnActivated(IStateOwner pOwner)
        {
            ShowHighScoresState scorestate = new ShowHighScoresState(TetrisGame.ScoreMan["Standard"], _State, null);


            



            //currentstate->tstateC->tstateblack->tstateA->scorestate

            //TransitionState tstateBlocks = new TransitionState_BlockRandom(pOwner.CurrentState, scorestate, new TimeSpan(0, 0, 0,0,500)) { GameProcDelegationMode = TransitionState.DelegateProcConstants.Delegate_Previous ,BlockSize=64};
            //TransitionState tstateBlocks = new TransitionState_Melt(pOwner.CurrentState, scorestate, new TimeSpan(0, 0, 0, 0, 750)) { GameProcDelegationMode = TransitionState.DelegateProcConstants.Delegate_Previous, Size=1,SnapshotSettings=TransitionState.SnapshotConstants.Snapshot_Both };
            
                TransitionState tstateBlocks = new TransitionState_Pixelate(pOwner.CurrentState, scorestate, new TimeSpan(0, 0, 0, 0, 1750)) { GameProcDelegationMode = TransitionState.DelegateProcConstants.Delegate_Previous,SnapshotSettings=TransitionState.SnapshotConstants.Snapshot_Both };

            //TransitionState tstate = TransitionState.GetTransitionState(pOwner.CurrentState, scorestate, new TimeSpan(0, 0, 0, 0, 500));
            //tstateblack.GameProcDelegationMode = tstateA.GameProcDelegationMode = TransitionState.DelegateProcConstants.Delegate_None;

            _Owner.CurrentState = tstateBlocks;
            _State.ActivatedItem = null;
            EnterExitScoreCount++;
            if (EnterExitScoreCount == 5)
            {
                MenuStateTextMenuItem CreditsMenu = new MenuStateTextMenuItem() { Text = "Credits", TipText = "View Credits!" };
                CreditsMenu.FontFace = FontSrc.FontFamily.Name;
                CreditsMenu.FontSize = FontSrc.Size;
                
                scorestate.BeforeRevertState += (o, e) =>
                {
                    _State.MenuElements.Add(CreditsMenu);
                    TetrisGame.Soundman.PlaySound("level_up", false).Tempo = 4;
                };
                _State.MenuItemActivated += ((a, b) =>
                {
                    if (b.MenuElement == CreditsMenu)
                    {
                        TextScrollState tss = new TextScrollState(_Owner.CurrentState);
                        _Owner.CurrentState = tss;
                        _State.ActivatedItem = null;
                    }
                });
            }

            return MenuEventResultConstants.Handled;
        }



    }
}
