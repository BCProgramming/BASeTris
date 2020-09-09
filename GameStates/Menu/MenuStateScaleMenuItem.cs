using BASeTris.GameStates.GameHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.Menu
{
    /// <summary>
    /// Menu item which
    /// </summary>
    public class MenuStateScaleMenuItem : MenuStateMultiOption<MenuItemScaleItemSelection>
    {
        private IStateOwner _Owner;
        //Tiny = 0.75
        //Small = 1
        //Large = 1.3
        //Biggliest = 1.6
        private MenuItemScaleItemSelection[] ScaleOptions = new MenuItemScaleItemSelection[]
        {
            new MenuItemScaleItemSelection() {Text = "Tiny",Scale = 0.75f},
            new MenuItemScaleItemSelection() {Text = "Small",Scale = 1f},
            new MenuItemScaleItemSelection() {Text = "Large",Scale = 1.3f},
            new MenuItemScaleItemSelection() {Text = "Biggliest",Scale = 1.6f}
        };
        public MenuStateScaleMenuItem(IStateOwner pOwner):base(null)
        {
            _Owner = pOwner;
            base.OptionManager = new MultiOptionManagerList<MenuItemScaleItemSelection>(ScaleOptions,1);
            var closest = (from so in ScaleOptions orderby Math.Abs(so.Scale - pOwner.ScaleFactor) ascending select so).First();
            this.Text = closest.Text;
            OptionManager.SetCurrentIndex(Array.IndexOf(ScaleOptions, closest));
            OnChangeOption += ScaleActivate;

        }
        public void ScaleActivate(Object sender,OptionActivated<MenuItemScaleItemSelection> e)
        {
            _Owner.SetScale(e.Option.Scale);
        }
    }
    public class MenuItemScaleItemSelection
    {
        public MenuItemScaleItemSelection(String pText,float pScale)
        {
            Text = pText;
            Scale = pScale;
        }
        public MenuItemScaleItemSelection()
        {

        }
        public String Text { get; set; }
        public float Scale { get; set; }
        public override String ToString()
        {
            return Text;
        }

    }

}
