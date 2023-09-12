using BASeTris.BackgroundDrawers;
using BASeTris.Choosers;
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
        public String[] Options { get; protected set; }
        public GamePreparerOptionSetPropertyAttribute(String pLabel,String[] ValidOptions):base(pLabel)
        {
            Options = ValidOptions;
        }
        protected GamePreparerOptionSetPropertyAttribute(String pLabel):base(pLabel)
        {
        }
    }
    //implies ICustomPropertyPreparer implementation on the containing class.
    public class GamePreparerCustomItemAttribute : GamePreparerPropertyAttribute
    {
        public GamePreparerCustomItemAttribute(String pLabel) : base(pLabel)
        {
        }
    }
    public interface ICustomPropertyPreparer
    {
        MenuStateMenuItem CreateItem(PropertyInfo Target, GamePreparerOptions Element);

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
    

    /// <summary>
    /// A Game Preparer is a class that holds information used to start a game; primarily, options. Consider Dr.Mario: before you start a game, it has the option screen for speed, starting level, and music. That is the sort of thing we are going for with these.
    /// Additionally, instead of requiring a bunch of custom work to create unique game states for each one, we instead have this setup where there is a data class that tags it's properties and those get utilized by a generic routine that can handle any instance.
    /// Another aspect is that some settings could be "unfair" and should be separate high score lists. This is facilitated by allowing classes to return a string for a high score key suffix. (the "default" settings should give back an empty string)
    /// </summary>
    public abstract class GamePreparerOptions
    {
        protected Dictionary<PropertyInfo, MenuStateMenuItem> MenuPropertyValues = new Dictionary<PropertyInfo, MenuStateMenuItem>();
        protected Dictionary<String, PropertyInfo> PropertiesbyName = new Dictionary<string, PropertyInfo>();
        protected MenuStateMenuItem GetMenuFromPropertyName(String sName)
        {

            if (PropertiesbyName.ContainsKey(sName))
            {
                if (MenuPropertyValues.ContainsKey(PropertiesbyName[sName]))
                    {
                    return MenuPropertyValues[PropertiesbyName[sName]];
                }
            }
            return null;
        }
        public virtual String BackgroundMusic { get; }
        public abstract String HighScoreCategorySuffix();
        
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

            BuildItems.Insert(0,StartGameItem);
            MenuState ConstructState = MenuState.CreateMenu(pOwner, pHeader, ReversionState, bg, sCancelText,int.MaxValue,BuildItems.ToArray());
            ConstructState.BackgroundMusicKey = OptionsData.BackgroundMusic;
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
            MenuStateMenuItem ResultValue = null;
            preparer.PropertiesbyName.Add(prop.Name, prop);
            if (src is GamePreparerOptionSetPropertyAttribute gpos)
            {
                int StartIndex = Array.FindIndex(gpos.Options, (a) => String.Equals(CurrentValue.ToString(), a, StringComparison.OrdinalIgnoreCase));
                MultiOptionManagerList<String> ManagerList = new MultiOptionManagerList<string>(gpos.Options, StartIndex);
                MenuStateMultiOption<String> SelectMenu = new MenuStateMultiOption<String>(ManagerList) { };
                SelectMenu.OnChangeOption += (o, e) =>
                {
                    prop.SetValue(preparer, e.Option);
                };

                ResultValue = SelectMenu;

            }
            else if (src is GamePreparerNumericPropertyAttribute gnum)
            {

                MenuStateSliderOption slider = new MenuStateSliderOption(gnum.MinimumValue, gnum.MaximumValue, Convert.ToDouble(CurrentValue)) { Label = gnum.Label, ChangeSize = gnum.ChangeSize };
                slider.ValueChanged += (o2, e2) =>
                {
                    prop.SetValue(preparer, Convert.ChangeType(e2.Value, prop.PropertyType));
                };
                ResultValue = slider;

            }
            else if (src is GamePreparerCustomItemAttribute gpci)
            {
                if (preparer is ICustomPropertyPreparer icpp)
                {
                    var CreateItem = icpp.CreateItem(prop, preparer);
                    if (CreateItem is MenuStateTextMenuItem mt && mt.Text == null) mt.Text = src.Label;
                    ResultValue = CreateItem;
                }
            }

            preparer.MenuPropertyValues.Add(prop, ResultValue);
            return ResultValue;

        }


    }

  
    public class StandardTetrisPreparer : GamePreparerOptions,ICustomPropertyPreparer
    {
        public override string BackgroundMusic => "drm_config";


        [GamePreparerNumericProperty("Level", 0, 500, 1)]
        public double StartingLevel { get; set; }
        public StandardTetrisPreparer(Type Initializer) : base(Initializer)
        {
        }
        protected virtual bool IsDefault()
        {
            return StartingLevel == 0 && RowCount == TetrisField.DEFAULT_ROWCOUNT && ColumnCount == TetrisField.DEFAULT_COLCOUNT;
        }
        public override string HighScoreCategorySuffix()
        {
            if (IsDefault()) return "";
            return $" S{StartingLevel} {RowCount}x{ColumnCount}";
        }
        [GamePreparerNumericProperty("Columns", 5, 100, 1)]

        public double ColumnCount { get; set; } = TetrisField.DEFAULT_COLCOUNT;
        [GamePreparerNumericProperty("Rows", 15, 100, 1)]
        public double RowCount { get; set; } = TetrisField.DEFAULT_ROWCOUNT;

        static IEnumerable<Object> GetChoosers()
        {
            yield break;
        }

        public MenuStateMenuItem CreateItem(PropertyInfo Target, GamePreparerOptions Element)
        {
            var OptionList = new MultiOptionManagerList<Type>(new Type[] { typeof(BagChooser), typeof(NESChooser), typeof(GameBoyChooser) },0);
            var createresult =
                new MenuStateMultiOption<Type>(OptionList);

            OptionList.GetItemText = (y) =>
            {
                return y.Name;
            };

            createresult.OnChangeOption += (obj, arg) =>
            {
                (Element as StandardTetrisPreparer).Chooser = arg.Option;
            };
            return createresult;

        }

        [GamePreparerCustomItem("Chooser")]
        public Type Chooser { get; set; }

    }
    public class NTrisGamePreparer : StandardTetrisPreparer
    {
        private int _MinimumNominoSize = 4;
        [GamePreparerNumericProperty("Minimum Nomino Size", 3, 100, 1)]
        public int MinimumNominoSize { get { return _MinimumNominoSize; } set { _MinimumNominoSize = value;

                var maxmenu = GetMenuFromPropertyName("MaximumNominoSize") as MenuStateSliderOption;
                if (maxmenu != null)
                {
                    if (maxmenu.Value < _MinimumNominoSize) maxmenu.Value = _MinimumNominoSize;
                }

            
            } }

        private int _MaximumNominoSize = 4;
        [GamePreparerNumericProperty("Maximum Nomino Size", 3, 100, 1)]
        public int MaximumNominoSize { get { return _MinimumNominoSize; } set 
            { 
                _MinimumNominoSize = value;
                var minmenu = GetMenuFromPropertyName("MinimumNominoSize") as MenuStateSliderOption;
                if (minmenu != null)
                {
                    if (minmenu.Value > _MaximumNominoSize) minmenu.Value = _MaximumNominoSize;
                }
                var rowmenu = GetMenuFromPropertyName("Rows") as MenuStateSliderOption;
                var colmenu = GetMenuFromPropertyName("Columns") as MenuStateSliderOption;

                if (rowmenu != null) rowmenu.Value = Math.Max(rowmenu.Value,GetFieldRowHeight(value));
                if (colmenu != null) colmenu.Value = Math.Max(colmenu.Value,GetFieldColumnWidth(value));
            } }

        private int GetFieldColumnWidth(int MaxBlockCount)
        {
            return 6 + MaxBlockCount + (int)((Math.Max(0, MaxBlockCount - 4) * 1.1));
        }
        private int GetFieldRowHeight(int MaxBlockCount)
        {
            var CurrWidth = GetFieldColumnWidth(MaxBlockCount);
            int DesiredHeight = (22 / 10) * CurrWidth;
            return DesiredHeight;
            //return 18 + BlockCount + (int)((Math.Max(0, BlockCount - 4) * 2));
        }
        public override string HighScoreCategorySuffix()
        {
            return base.HighScoreCategorySuffix() + "-" + MinimumNominoSize + "-" + MaximumNominoSize;
        }

        public NTrisGamePreparer(Type Initializer) : base(Initializer)
        {
        }
    }
    public class CascadingBlockPreparer : GamePreparerOptions
    {
        [GamePreparerNumericProperty("Types Count", 2, 6, 1)]
        public double TypeCount { get; set; } = 3;



        [GamePreparerNumericProperty("Level", 0, 50, 1)]
        public double StartingLevel { get; set; }


        [GamePreparerOptionSetProperty("Speed", new String[] { "Low", "Med", "Hi" })]
        public String Speed { get; set; } = "Low";

        public CascadingBlockPreparer(Type Initializer) : base(Initializer)
        {
        }

        [GamePreparerNumericProperty("Columns", 5, 100, 1)]

        public double ColumnCount { get; set; } = TetrisField.DEFAULT_COLCOUNT;
        [GamePreparerNumericProperty("Rows", 15, 100, 1)]
        public double RowCount { get; set; } = TetrisField.DEFAULT_ROWCOUNT;


        public override string HighScoreCategorySuffix()
        {
            return $"{TypeCount}-{StartingLevel}-{Speed}-{RowCount}-{ColumnCount}";
        }
    }

    public class DrMarioBlockPreparer : CascadingBlockPreparer
    {
        public DrMarioBlockPreparer(Type Initializer) : base(Initializer)
        {
        }
    }
    
}
