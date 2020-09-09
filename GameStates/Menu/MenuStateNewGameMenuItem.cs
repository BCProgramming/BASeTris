using BASeTris.GameStates.GameHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.Menu
{

    public class MenuStateNewGameMenuItem : MenuStateMultiOption<MenuItemNewGameSelection>
    {
        private IStateOwner _Owner;
        //Tiny = 0.75
        //Small = 1
        //Large = 1.3
        //Biggliest = 1.6
        private MenuItemNewGameSelection DefaultOption = null;
        public MenuStateNewGameMenuItem(IStateOwner pOwner) : base(null)
        {
            _Owner = pOwner;

            var HandlerOptions = Program.GetGameHandlers();
            List<MenuItemNewGameSelection> Options = new List<MenuItemNewGameSelection>();
            var Default = new MenuItemNewGameSelection(null);
            DefaultOption= Default;
            //Options.Add(Default);
            foreach (var iterate in HandlerOptions)
            {
                var findconstruct = iterate.GetConstructor(new Type[] { });
                if (findconstruct != null)
                {
                    IGameCustomizationHandler newhandler = (IGameCustomizationHandler)findconstruct.Invoke(new object[] { });
                    MenuItemNewGameSelection newoption = new MenuItemNewGameSelection(newhandler);
                    Options.Add(newoption);
                }
            }
            
            base.OptionManager = new MultiOptionManagerList<MenuItemNewGameSelection>(Options.ToArray(), 1);
            this.Text = "New Game";
            this.CurrentOption = Default;
            //OnActivateOption += ScaleActivate;


        }
        public void Reset()
        {
            CurrentOption = DefaultOption;
        }
      
    }


    public class MenuItemNewGameSelection
    {
        IGameCustomizationHandler _Handler;
        public IGameCustomizationHandler Handler { get { return _Handler; } }
        public MenuItemNewGameSelection(IGameCustomizationHandler handler)
        {
            _Handler = handler;
        }
        public override string ToString()
        {
            return _Handler == null ? "Cancel" : _Handler.Name;
        }
    }

}
