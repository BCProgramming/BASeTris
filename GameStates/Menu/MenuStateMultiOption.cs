using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.Menu
{
    /// <summary>
    /// A text Menu item that can be activated to select/change between a set of options.
    /// </summary>
    public class MenuStateMultiOption<T> :MenuStateTextMenuItem
    {
        protected IMultiOptionManager<T> OptionManager = null;
        public event EventHandler<OptionActivated<T>> OnActivateOption;
        public T CurrentOption = default(T);
        private bool _Activated = false;
        private bool _Selected = false;
        public MenuStateMultiOption(IMultiOptionManager<T> pOptionManager)
        {
            OptionManager = pOptionManager;
        }

        public override MenuEventResultConstants OnSelected()
        {
            _Selected = true;
            return base.OnSelected();
        }

        public override MenuEventResultConstants OnDeselected()
        {
            _Selected = false;
            return base.OnDeselected();
        }

        public override MenuEventResultConstants OnActivated()
        {
            _Activated = true;
            return MenuEventResultConstants.Handled;
        }
        public override MenuEventResultConstants OnDeactivated()
        {
            _Activated = false;
            return MenuEventResultConstants.Handled;
        }
        public override void Draw(IStateOwner pOwner,Graphics Target, RectangleF Bounds, StateMenuItemState DrawState)
        {
            //draw < and > just outside the bounds using our font.
            Font useFont = GetScaledFont(pOwner);
            String sLeftCover = "< ";
            String sRightCover = ">";
            var PrevItem = OptionManager.GetText(OptionManager.PeekPrevious());
            var NextItem = OptionManager.GetText(OptionManager.PeekNext());
            sLeftCover = PrevItem + sLeftCover;
            sRightCover = sRightCover + NextItem;
            var MeasureLeft = Target.MeasureString(sLeftCover, useFont);
            var MeasureRight = Target.MeasureString(sRightCover, useFont);

            PointF LeftPos = new PointF(Bounds.Left - MeasureLeft.Width, Bounds.Top + (Bounds.Height / 2) - MeasureLeft.Height / 2);
            PointF RightPos = new PointF(Bounds.Right,Bounds.Top + (Bounds.Height/2) - MeasureRight.Height/2);
            
            if (_Activated)
            {
                TetrisGame.DrawText(Target, useFont, sLeftCover, this.ForeBrush, ShadowBrush, LeftPos.X, LeftPos.Y);
                TetrisGame.DrawText(Target, useFont, sRightCover, this.ForeBrush, ShadowBrush, RightPos.X, RightPos.Y);
            }

            base.Draw(pOwner,Target, Bounds, DrawState);
        }

        public override void ProcessGameKey(IStateOwner pStateOwner, GameState.GameKeys pKey)
        {
            if (_Activated)
            {
                if (pKey == GameState.GameKeys.GameKey_Left)
                {
                    CurrentOption = OptionManager.MovePrevious();
                    OnActivateOption?.Invoke(this, new OptionActivated<T>(CurrentOption,pStateOwner));
                }
                else if (pKey == GameState.GameKeys.GameKey_Right)
                {
                    CurrentOption = OptionManager.MoveNext();
                    OnActivateOption?.Invoke(this,new OptionActivated<T>(CurrentOption,pStateOwner));
                }

                if (OptionManager != null && CurrentOption!=null)
                    base.Text = OptionManager.GetText(CurrentOption);
                base.ProcessGameKey(pStateOwner, pKey);
            }
        }
    }
    public class OptionActivated<T> : EventArgs
    {
        public T Option;
        public IStateOwner Owner = null;
        public OptionActivated(T pOption,IStateOwner pOwner)
        {
            Option = pOption;
            Owner = pOwner;
        }
    }
    public interface IMultiOptionManager<T>
    {
        T MovePrevious();
        T MoveNext();

        T PeekPrevious();
        T PeekNext();

        void SetCurrentIndex(int Index);
        String GetText(T Item);
    }
    public class MultiOptionManagerList<T>:IMultiOptionManager<T>
    {
        private int SelectedIndex;
        private T[] Options;
        public void SetCurrentIndex(int pIndex)
        {
            SelectedIndex = pIndex;
        }
        public MultiOptionManagerList(T[] pOptions,int pStartingIndex)
        {
            Options = pOptions;
            SelectedIndex = pStartingIndex;
        }
        public string GetText(T Value)
        {
            return Value.ToString();
        }

        public T PeekPrevious()
        {
            var TestIndex = SelectedIndex - 1;
            if (TestIndex < 0) TestIndex = Options.Length - 1;
            return Options[TestIndex];
        }

        public T PeekNext()
        {
            var TestIndex = SelectedIndex + 1;
            if (TestIndex > Options.Length-1) TestIndex = 0;
            return Options[TestIndex];
        }

        public T MovePrevious()
        {
            SelectedIndex--;
            if (SelectedIndex < 0) SelectedIndex = Options.Length - 1;
            return Options[SelectedIndex];
        }
        public T MoveNext()
        {
            SelectedIndex++;
            if (SelectedIndex > Options.Length-1) SelectedIndex = 0;
            return Options[SelectedIndex];
        }
    }
}
