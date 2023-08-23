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
            _Owner.CurrentState = scorestate;
            _State.ActivatedItem = null;
            EnterExitScoreCount++;
            if (EnterExitScoreCount == 5)
            {
                MenuStateTextMenuItem CreditsMenu = new MenuStateTextMenuItem() { Text = "Credits", TipText = "View Credits!" };
                CreditsMenu.FontFace = FontSrc.FontFamily.Name;
                CreditsMenu.FontSize = FontSrc.Size;
                _State.MenuElements.Add(CreditsMenu);
                scorestate.BeforeRevertState += (o, e) =>
                {
                    TetrisGame.Soundman.PlaySound("level_up", false).Tempo = 2;
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
