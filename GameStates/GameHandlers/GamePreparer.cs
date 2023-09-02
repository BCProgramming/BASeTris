using BASeTris.BackgroundDrawers;
using BASeTris.GameStates.Menu;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.GameHandlers
{

    public interface IPreparableGame
    {
        void SetPrepData(GamePreparerOptions gpo);

    }
    public class GamePreparerAttribute : Attribute
    {
        private Type _PreparerOptionsType;
        public Type PreparerOptionsType { get { return _PreparerOptionsType; } set { _PreparerOptionsType = value; } }

        public GamePreparerAttribute(Type pOptionsType)
        {
            if (!typeof(GamePreparerOptions).IsAssignableFrom(pOptionsType))
                throw new ArgumentException("Option type must be derived from GamePreparerOptions class.");

            _PreparerOptionsType = pOptionsType;
        }
        public static GamePreparerAttribute HasPreparerAttribute(Type HandlerType)
        {
            return HandlerType.GetCustomAttributes<GamePreparerAttribute>(true).FirstOrDefault();
        }
    }

    public abstract class GamePreparerPropertyAttribute : Attribute
    {
        public String Label { get; private set; }
        public GamePreparerPropertyAttribute(String pLabel)
        {
            Label = pLabel;
        }
        //used to indicate the type of value this represents. abstract class implemented by each type.
    }
    /// <summary>
    /// represents an option that will be represented via a multichoice option menu item.
    /// </summary>
    public class GamePreparerOptionSetPropertyAttribute : GamePreparerPropertyAttribute
    {
        public String[] Options { get; private set; }
        public GamePreparerOptionSetPropertyAttribute(String pLabel,String[] ValidOptions):base(pLabel)
        {
            Options = ValidOptions;
        }

    }
    public class GamePreparerNumericPropertyAttribute : GamePreparerPropertyAttribute
    {
        public double MinimumValue { get; private set; }
        public double MaximumValue { get; private set; }
        public double ChangeSize { get; private set; }
        public GamePreparerNumericPropertyAttribute(String pLabel,double pMinimum, double pMaximum, double pChangeSize):base(pLabel)
        {
            MinimumValue = pMinimum;
            MaximumValue = pMaximum;
            ChangeSize = pChangeSize;
        }
    }
    public abstract class GamePreparerOptions
    {

        public GamePreparerOptions(Type CustomizationHandlerType)
        {
        }
        public static MenuState ConstructPreparationState(IStateOwner pOwner,String pHeader,GameState ReversionState,IBackground bg,String sCancelText, GamePreparerOptions OptionsData,Action<GamePreparerOptions> ProceedFunc)
        {
            //precondition: any loading/initialization on the GamePreparerOptions instance has been done.
            //task: create a MenuState that has menu items for each property in the definition of the given class that has a GamePreparer Attribute.
            //This should include appropriate handling of menu changes such that the values are assigned to the information class as values are changed.
            //we are not responsible for adding any additional menu items, such as elements to return to the previous menu, or to proceed. Those should be dealt with by the caller. 

            List<MenuStateMenuItem> BuildItems = new List<MenuStateMenuItem>();
            var SourceProperties = OptionsData.GetType().GetProperties();

            foreach (var iterateprop in SourceProperties)
            {
                Object CurrentPropertyValue = null;
                if (iterateprop.CanRead)
                {
                    CurrentPropertyValue = iterateprop.GetValue(OptionsData);
                }
                var grabattribute = (GamePreparerPropertyAttribute)iterateprop.GetCustomAttributes(typeof(GamePreparerPropertyAttribute), true).FirstOrDefault();
                if (grabattribute!=null)
                {
                    MenuStateMenuItem mitem = ConstructItem(OptionsData,iterateprop,grabattribute,CurrentPropertyValue);
                    BuildItems.Add(mitem);
                }

            }

            MenuStateTextMenuItem StartGameItem = new MenuStateTextMenuItem() { Text = "Start Game" };

            BuildItems.Add(StartGameItem);
            MenuState ConstructState = MenuState.CreateMenu(pOwner, pHeader, ReversionState, bg, sCancelText,int.MaxValue,BuildItems.ToArray());

            ConstructState.MenuItemActivated += (o, e) =>
            {
                if (e.MenuElement == StartGameItem)
                {
                    ProceedFunc(OptionsData);
                }
            };
            return ConstructState;
        }

        private static MenuStateMenuItem ConstructItem(GamePreparerOptions preparer, PropertyInfo prop, GamePreparerPropertyAttribute src,Object CurrentValue)
        {
            if (src is GamePreparerOptionSetPropertyAttribute gpos)
            {
                int StartIndex = Array.FindIndex(gpos.Options, (a) => String.Equals(CurrentValue.ToString(), a, StringComparison.OrdinalIgnoreCase));
                MultiOptionManagerList<String> ManagerList = new MultiOptionManagerList<string>(gpos.Options, StartIndex);
                MenuStateMultiOption<String> SelectMenu = new MenuStateMultiOption<String>(ManagerList) { } ;
                SelectMenu.OnChangeOption += (o, e) =>
                {
                    prop.SetValue(preparer, e.Option);
                };
                
                return SelectMenu;

            }
            else if (src is GamePreparerNumericPropertyAttribute gnum)
            {

                MenuStateSliderOption slider = new MenuStateSliderOption(gnum.MinimumValue, gnum.MaximumValue, Convert.ToDouble(CurrentValue)) { Label = gnum.Label, ChangeSize = gnum.ChangeSize };
                slider.ValueChanged += (o2, e2) =>
                {
                    prop.SetValue(preparer, e2.Value);
                };
                return slider;

            }

            return null;

        }


    }

    public class StupidTestOptions : GamePreparerOptions
    {
        [GamePreparerNumericProperty("Level", 0, 50, 1)]
        public double SomeSlider { get; set; }
        public StupidTestOptions(Type Initializer) : base(Initializer)
        {
        }
        
    }
    public class StandardTetrisPreparer : GamePreparerOptions
    {
        [GamePreparerNumericProperty("Level", 0, 50, 1)]
        public double StartingLevel { get; set; }
        public StandardTetrisPreparer(Type Initializer) : base(Initializer)
        {
        }


    }
    
}
