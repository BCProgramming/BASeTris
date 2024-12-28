using BASeCamp.Elementizer;
using BASeTris.BackgroundDrawers;
using BASeTris.Choosers;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering;
using BASeTris.Rendering.Skia;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BASeTris.GameStates.GameHandlers
{

    public interface IPreparableGame
    {
        void SetPrepData(GamePreparerOptions gpo);
        GamePreparerOptions PrepInstance { get; }

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
    public class GamePreparerReadOnlyTextPropertyAttribute : GamePreparerPropertyAttribute
    {
        
        public GamePreparerReadOnlyTextPropertyAttribute(string pLabel) : base(pLabel)
        {
        
        }
    }

    /// <summary>
    /// represents an option that will be represented via a multichoice option menu item.
    /// </summary>
    public class PreparerOptionSetPropertyAttribute : GamePreparerPropertyAttribute
    {
        public String[] Options { get; protected set; }
        public PreparerOptionSetPropertyAttribute(String pLabel,String[] ValidOptions):base(pLabel)
        {
            Options = ValidOptions;
        }
        protected PreparerOptionSetPropertyAttribute(String pLabel):base(pLabel)
        {
        }
    }
    //implies ICustomPropertyPreparer implementation on the containing class.
    public class PreparerCustomItemAttribute : GamePreparerPropertyAttribute
    {
        public PreparerCustomItemAttribute(String pLabel) : base(pLabel)
        {
        }
    }
    public interface ICustomPropertyPreparer : IXmlPersistable
    {
        MenuStateMenuItem CreateItem(PropertyInfo Target, PreparerOptions Element);
        bool ItemActivated(MenuStateMenuItemActivatedEventArgs msmiargs);

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
    //Preparers are when we want to construct a "menu" that manipulates a 'preparerproperties' instance automatically. This was originally just a GamePreparer but got abstracted out so we could re-use it for Themes.
    public abstract class PreparerOptions : IXmlPersistable, ICustomPropertyPreparer
    {
        public virtual String BackgroundMusic { get; }
        public virtual Type CustomPreparerStateType { get { return null; } } 
        public Type HandlerType { get; set; }
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

        public PreparerOptions(Type CustomizationHandlerType)
        {
            HandlerType = CustomizationHandlerType;
        }
           public PreparerOptions(XElement Src, Object pData)
        {
            //need to interpret provided node to get back data saved by GetXmlData.
            String sHandlerType = Src.GetAttributeString("HandlerType", null);
            
            if (sHandlerType != null)
            {
                HandlerType = Type.GetType(sHandlerType, false);
            }
            
            
            foreach (var iterateprop in GetPreparerProperties(this.GetType()))
            {
                XAttribute findattribute = Src.Attribute(iterateprop.Item1.Name);
                if (findattribute != null)
                {

                    if (iterateprop.Item1.PropertyType is Type)
                    {
                        Type useType = StandardHelper.ClassFinder(findattribute.Value);
                        iterateprop.Item1.SetValue(this, useType);
                    }
                    else
                    {
                        iterateprop.Item1.SetValue(this, Convert.ChangeType(findattribute.Value, iterateprop.Item1.PropertyType));
                    }
                }
            }


        }
          public virtual XElement GetXmlData(String pNodeName, Object pContext)
        {
            XElement result = new XElement(pNodeName, new XAttribute("HandlerType", HandlerType.AssemblyQualifiedName));

            

            foreach (var iterateprop in GetPreparerProperties(this.GetType()))
            {
                XAttribute xattr = new XAttribute(iterateprop.Item1.Name, Convert.ToString(iterateprop.Item1.GetValue(this)));
                result.Add(xattr);
            }

            return result;

        }
        protected static IEnumerable<(PropertyInfo, GamePreparerPropertyAttribute)> GetPreparerProperties(Type PreparerType)
        {
            var SourceProperties = PreparerType.GetProperties();
            foreach (var checkproperty in SourceProperties)
            {
                if (checkproperty.CanRead && checkproperty.CanWrite)
                {
                    var grabattribute = (GamePreparerPropertyAttribute)checkproperty.GetCustomAttributes(typeof(GamePreparerPropertyAttribute), true).FirstOrDefault();
                    if (grabattribute != null) yield return (checkproperty, grabattribute);
                }
            }

        }

        public abstract MenuStateMenuItem CreateItem(PropertyInfo Target, PreparerOptions Element);
        

        public abstract bool ItemActivated(MenuStateMenuItemActivatedEventArgs msmiargs);
        
        public static GameState ConstructPreparationState<PREPSTATE>(IStateOwner pOwner, String pHeader, GameState ReversionState, IBackground bg, String sCancelText, PREPSTATE OptionsData, Action<PREPSTATE> ProceedFunc) where PREPSTATE:PreparerOptions
        {
             if (OptionsData.CustomPreparerStateType != null)
             {
                //if specified, we will want to construct the specified type. It should adhere to a particular constructor.
                if (!OptionsData.CustomPreparerStateType.IsAssignableTo(typeof(GameState)))
                {
                    throw new ArgumentException($"CustomPreparerProperty of type {OptionsData.CustomPreparerStateType.GetType().Name} is not a GameState.");
                }
                var useConstructor = OptionsData.CustomPreparerStateType.GetConstructor(new Type[] { typeof(GameState), typeof(IBackground), typeof(PREPSTATE), typeof(Action<PREPSTATE>) });
                if (useConstructor != null)
                {
                    var resultstate = (GameState)useConstructor.Invoke(new Object[] { ReversionState, bg, OptionsData, ProceedFunc });
                    return resultstate;
                }
                else
                {
                    throw new ArgumentException($"CustomPreparerProperty of type {OptionsData.CustomPreparerStateType.GetType().Name} does not have required constructor.");
                }

             }


             List<MenuStateMenuItem> BuildItems = new List<MenuStateMenuItem>();

            

            foreach (var reviewprop in GetPreparerProperties(OptionsData.GetType()))
            {
                Object CurrentPropertyValue = reviewprop.Item1.GetValue(OptionsData);
                MenuStateMenuItem mitem = ConstructItem(OptionsData, reviewprop.Item1, reviewprop.Item2, CurrentPropertyValue);
                BuildItems.Add(mitem);
            }


             MenuState ConstructState = MenuState.CreateMenu(pOwner, pHeader, ReversionState, bg, sCancelText,int.MaxValue,BuildItems.ToArray());
            ConstructState.BackgroundMusicKey = OptionsData.BackgroundMusic;

            


            ConstructState.MenuItemActivated += (o, e) =>
            {
               /* if (e.MenuElement == StartGameItem)
                {
                    ConstructState.BackgroundMusicKey = "";

                    ProceedFunc(OptionsData);
                }
                else if (e.MenuElement == ResumeSuspendedGame)
                {
                    if (!File.Exists(sSuspendedFile))
                    {
                        ConstructState.MenuElements.Remove(ResumeSuspendedGame);
                    }

                    XDocument? loadedDoc = null;

                    using (FileStream strm = new FileStream(sSuspendedFile, FileMode.Open, FileAccess.Read))
                    {

                        using (GZipStream gstream = new GZipStream(strm, CompressionMode.Decompress))

                            loadedDoc = XDocument.Load(gstream);
                    }
                    if (loadedDoc == null)
                    {
                        ConstructState.MenuElements.Remove(ResumeSuspendedGame);
                    }

                    ConstructState.BackgroundMusicKey = "";
                    ProceedFunc(OptionsData);
                    Func<bool> RestoreSuspendedGameProc = null;
                    RestoreSuspendedGameProc = () =>
                    {
                        //first frame it calls this routine, and we will load in the suspended game.
                        if (pOwner.CurrentState is GameplayGameState gpgs)
                        {

                            gpgs.RestoreState(pOwner,loadedDoc.Root);

                            //foreach(var iterate in gpgs.PlayField.Contents




                            RenderingProvider.Static.ClearExtendedData(gpgs.PlayField.GetType(), gpgs.PlayField);
                            gpgs.ReapplyTheme();
                            return false;
                        }
                        else
                        {
                            //if it is not,
                            //pOwner.EnqueueAction(RestoreSuspendedGameProc);
                            return true;
                        }



                    };


                    pOwner.EnqueueAction(RestoreSuspendedGameProc);
                }
                else*/
                if (OptionsData is ICustomPropertyPreparer cpp)
                {
                    cpp.ItemActivated(e);
                }
                
            };
            return ConstructState;


        }


        protected static MenuStateMenuItem ConstructItem(PreparerOptions preparer, PropertyInfo prop, GamePreparerPropertyAttribute src,Object CurrentValue)
        {
            MenuStateMenuItem ResultValue = null;
            preparer.PropertiesbyName.Add(prop.Name, prop);
            if (src is PreparerOptionSetPropertyAttribute gpos)
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
            else if (src is GamePreparerReadOnlyTextPropertyAttribute gtext)
            {
                MenuStateLabelMenuItem labelitem = new MenuStateLabelMenuItem() { Text = Convert.ToString(CurrentValue) };
                ResultValue = labelitem;
            }
            else if (src is PreparerCustomItemAttribute gpci)
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


    /// <summary>
    /// A Game Preparer is a class that holds information used to start a game; primarily, options. Consider Dr.Mario: before you start a game, it has the option screen for speed, starting level, and music. That is the sort of thing we are going for with these.
    /// Additionally, instead of requiring a bunch of custom work to create unique game states for each one, we instead have this setup where there is a data class that tags it's properties and those get utilized by a generic routine that can handle any instance.
    /// Another aspect is that some settings could be "unfair" and should be separate high score lists. This is facilitated by allowing classes to return a string for a high score key suffix. (the "default" settings should give back an empty string)
    /// </summary>
    //this class is persistable so that the settings can be saved. Specifically (at the time of me writing this, anyway) for the purpose of recording the initial state information for replays.
    public abstract class GamePreparerOptions : PreparerOptions
    {

        
        [PreparerCustomItem("Seed")]
        public int RandomSeed { get; set; } = Environment.TickCount;
        
       
        
        public abstract String HighScoreCategorySuffix();

        public override XElement GetXmlData(string pNodeName, object pContext)
        {
            var resultnode = base.GetXmlData(pNodeName, pContext);
            resultnode.Add(new XAttribute("Seed", RandomSeed));
            return resultnode;
        }
        public GamePreparerOptions(XElement src, Object pData) : base(src, pData)
        {
            RandomSeed = src.GetAttributeInt("Seed", Environment.TickCount);
        }
        public GamePreparerOptions(Type CustomizationHandlerType):base(CustomizationHandlerType)
        {
            HandlerType = CustomizationHandlerType;
        }
        
        public static GameState ConstructPreparationState(IStateOwner pOwner,String pHeader,GameState ReversionState,IBackground bg,String sCancelText, GamePreparerOptions OptionsData,Action<GamePreparerOptions> ProceedFunc)
        {
            //this needs refactored somewhat to have shared code in the make PreparerOptions, and only GameHandler specific preparation stuff in here.

            //precondition: any loading/initialization on the GamePreparerOptions instance has been done.
            //task: create a MenuState that has menu items for each property in the definition of the given class that has a GamePreparer Attribute.
            //This should include appropriate handling of menu changes such that the values are assigned to the information class as values are changed.
            //we are not responsible for adding any additional menu items, such as elements to return to the previous menu, or to proceed. Those should be dealt with by the caller. 


            var ConstructState = PreparerOptions.ConstructPreparationState<GamePreparerOptions>(pOwner, pHeader, ReversionState, bg, sCancelText, OptionsData, ProceedFunc);
            
            //Game handler specific
            //----------------------------
            
            MenuStateTextMenuItem StartGameItem = new MenuStateTextMenuItem() { Text = "Start Game",TipText = "Start the Game" };
            MenuStateTextMenuItem ResumeSuspendedGame = new MenuStateTextMenuItem() { Text = "Resume Game", TipText = "Resume Suspended Game"};

            //since our start and resume items are being added after we've already constructed the state, we need to prepare their font info.

            MenuState.SetTextMenuItemDefault(pOwner, StartGameItem);
            MenuState.SetTextMenuItemDefault(pOwner, ResumeSuspendedGame);


            if (ConstructState is MenuState ms) //possible it's a custom non-menu type, in which case, well, it's on it's own to add Start Game and Resume Game Options, I guess!
            {


                ms.MenuElements.Insert(0, StartGameItem);

                String sSuspendedFile = TetrisGame.GetSuspendedGamePath(OptionsData.HandlerType);
                if (!String.IsNullOrEmpty(sSuspendedFile) && File.Exists(sSuspendedFile))
                {
                    ResumeSuspendedGame.Text = $"Resume Game ({new FileInfo(sSuspendedFile).LastWriteTime.ToString("MM/dd/yy hh:mm}")})";
                    ms.MenuElements.Add(ResumeSuspendedGame);


                }



                //add handling for the menu element.

                ms.MenuItemActivated += (o, e) =>
                {
                    if (e.MenuElement == StartGameItem)
                    {
                        ms.BackgroundMusicKey = "";

                        ProceedFunc(OptionsData);
                    }
                    else if (e.MenuElement == ResumeSuspendedGame)
                    {
                        if (!File.Exists(sSuspendedFile))
                        {
                            ms.MenuElements.Remove(ResumeSuspendedGame);
                        }

                        XDocument? loadedDoc = null;

                        using (FileStream strm = new FileStream(sSuspendedFile, FileMode.Open, FileAccess.Read))
                        {

                            using (GZipStream gstream = new GZipStream(strm, CompressionMode.Decompress))

                                loadedDoc = XDocument.Load(gstream);
                        }
                        if (loadedDoc == null)
                        {
                            ms.MenuElements.Remove(ResumeSuspendedGame);
                        }

                        ms.BackgroundMusicKey = "";
                        ProceedFunc(OptionsData);
                        Func<bool> RestoreSuspendedGameProc = null;
                        RestoreSuspendedGameProc = () =>
                        {
                            //first frame it calls this routine, and we will load in the suspended game.
                            if (pOwner.CurrentState is GameplayGameState gpgs)
                            {

                                gpgs.RestoreState(pOwner, loadedDoc.Root);

                                //foreach(var iterate in gpgs.PlayField.Contents




                                RenderingProvider.Static.ClearExtendedData(gpgs.PlayField.GetType(), gpgs.PlayField);
                                gpgs.ReapplyTheme();
                                return false;
                            }
                            else
                            {
                                //if it is not,
                                //pOwner.EnqueueAction(RestoreSuspendedGameProc);
                                return true;
                            }



                        };


                        pOwner.EnqueueAction(RestoreSuspendedGameProc);
                    }
                };
            }
            return ConstructState;
        }

        
        public override bool ItemActivated(MenuStateMenuItemActivatedEventArgs msmi)
        {
            if (msmi.MenuElement == SeedItem)
            {
                RandomSeed = Environment.TickCount;
                SeedItem.Text = RandomSeed.ToString();
                msmi.CancelActivation = true;
                return true;
                
            }
            return false;
        }
        private MenuStateTextMenuItem  SeedItem = null;
        public override MenuStateMenuItem CreateItem(PropertyInfo Target, PreparerOptions Element)
        {
            if (Target.Name == "RandomSeed")
            {
                MenuStateTextMenuItem mstmi = new MenuStateTextMenuItem() { Text = Convert.ToString(Target.GetValue(Element)) };
                mstmi.TipText = "Enter to change random seed.";
                SeedItem = mstmi;
                return SeedItem;
            }
            return null;
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
        public StandardTetrisPreparer(XElement Source, Object pContext):base(Source,pContext)
        {

            StartingLevel = Source.GetAttributeDouble("StartingLevel", 0);

        }
        public override XElement GetXmlData(String pNodeName, Object pContext)
        {
            var xresult = base.GetXmlData(pNodeName, pContext);
        //    xresult.Add(new XAttribute("StartingLevel", StartingLevel));
            return xresult;
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

        public override MenuStateMenuItem CreateItem(PropertyInfo Target, PreparerOptions Element)
        {
            var result = base.CreateItem(Target, Element);
            if (result != null) return result;

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

        [PreparerCustomItem("Chooser")]
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
        public int MaximumNominoSize { get { return _MaximumNominoSize; } set 
            { 
                _MaximumNominoSize = value;
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


        [PreparerOptionSetProperty("Speed", new String[] { "Low", "Med", "Hi" })]
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
