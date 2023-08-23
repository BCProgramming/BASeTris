using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.Menu
{
    public class MenuStateSliderOption : MenuStateSizedMenuItem
    {
        public class SliderValueChangeEventArgs : EventArgs
        {
            public double Value { get; set; }
            public SliderValueChangeEventArgs(double pValue)
            {
                Value = pValue;
            }
        }
        public string Label { get; set; } = "";
        public event EventHandler<SliderValueChangeEventArgs> ValueChanged;
        public double Value { get; set; } = 50;
        public double MinimumValue { get; set; } = 0;
        public double MaximumValue { get; set; } = 100;
        public double ChangeSize { get; set; } = 1;

        public double LargeDetentCount { get; set; } = 4;
        public double SmallDetent { get; set; } = 5;
        public bool Activated { get; set; } = false;

        public MenuStateSliderOption(double pMinimum, double pMaximum, double pValue)
        {
            MinimumValue = pMinimum;
            MaximumValue = pMaximum;
            Value = pValue;
        }
        public override MenuEventResultConstants OnActivated(IStateOwner pOwner)
        {
            Activated = true;
            return MenuEventResultConstants.Handled; 
        }
        public override MenuEventResultConstants OnDeactivated(IStateOwner pOwner)
        {
            Activated = false;
            return MenuEventResultConstants.Handled;
        }


        public override bool GetSelectable()
        {
            return true;
        }
        public override void ProcessGameKey(IStateOwner pStateOwner, GameState.GameKeys pKey)
        {
            if (Activated)
            {
                if (pKey == GameState.GameKeys.GameKey_Left)
                {
                    double SetValue = Value - ChangeSize;
                    SetValue = TetrisGame.ClampValue(SetValue, MinimumValue, MaximumValue);
                    Value = SetValue;
                    ValueChanged?.Invoke(this, new SliderValueChangeEventArgs(Value));
                }
                else if (pKey == GameState.GameKeys.GameKey_Right)
                {
                    double SetValue = Value + ChangeSize;
                    SetValue = TetrisGame.ClampValue(SetValue, MinimumValue, MaximumValue);
                    Value = SetValue;
                    ValueChanged?.Invoke(this, new SliderValueChangeEventArgs(Value));
                }


                base.ProcessGameKey(pStateOwner, pKey);
            }
        }
    }
}
