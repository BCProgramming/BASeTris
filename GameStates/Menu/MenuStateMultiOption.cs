using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.Menu
{
    public class MenuStateMultiOption :  MenuStateTextMenuItem
    {
        public IMultiOptionManager OptionManagerBase = null;
        public virtual Object CurrentOptionBase { get; set; } = null;
        protected bool _Activated = false;
        public bool Activated {  get { return _Activated; } set { _Activated = value; } }
        protected bool _Selected = false;
        public bool Selected {  get { return _Selected; } set { _Selected = value; } }
        public MenuStateMultiOption(IMultiOptionManager pOptionManager)
        {
            OptionManagerBase = pOptionManager;
        }

    }
    /// <summary>
    /// A text Menu item that can be activated to select/change between a set of options.
    /// </summary>
    public class MenuStateMultiOption<T> : MenuStateMultiOption where T:class
    {
        public bool SubMenuSelection { get; set; } = false;
        public IMultiOptionManager<T> OptionManager { get { return OptionManagerBase as IMultiOptionManager<T>; } set { OptionManagerBase = value; } }
        public event EventHandler<OptionActivated<T>> OnChangeOption;
        public event EventHandler<OptionActivated<T>> OnDeactivateOption;
        public event EventHandler<OptionActivated<T>> OnActivateOption;
        public T CurrentOption { get { return (T)CurrentOptionBase; } set { CurrentOptionBase = value; } }
        public MenuStateMultiOption(IMultiOptionManager<T> pOptionManager):base(pOptionManager)
        {
        }

        public override MenuEventResultConstants OnSelected(IStateOwner pOwner)
        {
            _Selected = true;
            return base.OnSelected(pOwner);
        }

        public override MenuEventResultConstants OnDeselected(IStateOwner pOwner)
        {
            _Selected = false;
            return base.OnDeselected(pOwner);
        }

        public override MenuEventResultConstants OnActivated(IStateOwner pOwner)
        {
            if (SubMenuSelection)
            {
                //'LastOwner' should be an IStateOwner. we can use that. Maybe.

                //create a list of the text items for our options, and plop the tag into it as well.
                MenuStateTextMenuItem[] ThemeItems = (from t in OptionManager.GetAllOptions() select new MenuStateTextMenuItem() { Text = OptionManager.GetText(t),TipText=OptionManager.GetTipText(t), Tag = (Object)t }).ToArray();

                MenuState OptionSubMenu = MenuState.CreateMenu(pOwner,"Choose Theme", pOwner.CurrentState, null,"Cancel", ThemeItems);

                OptionSubMenu.MenuItemActivated += (a, b) =>
                {
                    T TagItem = (b.MenuElement.Tag as T);
                    OnChangeOption?.Invoke(this, new OptionActivated<T>(TagItem, pOwner));

                };
                OptionSubMenu.MenuItemSelected += (a, b) =>
                {
                    T TagItem = (b.MenuElement.Tag as T);
                    if (TagItem != null)
                    {
                        OptionSubMenu.FooterText = OptionManager.GetTipText(TagItem);
                    }
                };
                pOwner.CurrentState = OptionSubMenu;


            }
            else
            {
                OnActivateOption?.Invoke(this, new OptionActivated<T>(CurrentOption, pOwner));
                _Activated = true;
            }
            return MenuEventResultConstants.Handled;
        }
        public override MenuEventResultConstants OnDeactivated(IStateOwner pOwner)
        {
            OnDeactivateOption?.Invoke(this, new OptionActivated<T>(CurrentOption, pOwner));
            _Activated = false;
            return MenuEventResultConstants.Handled;
        }
        
        public override void ProcessGameKey(IStateOwner pStateOwner, GameState.GameKeys pKey)
        {
            
            if (_Activated && !SubMenuSelection)
            {
                if (pKey == GameState.GameKeys.GameKey_Left)
                {
                    CurrentOption = OptionManager.MovePrevious();
                    OnChangeOption?.Invoke(this, new OptionActivated<T>(CurrentOption,pStateOwner));
                }
                else if (pKey == GameState.GameKeys.GameKey_Right)
                {
                    CurrentOption = OptionManager.MoveNext();
                    OnChangeOption?.Invoke(this,new OptionActivated<T>(CurrentOption,pStateOwner));
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
    public interface IMultiOptionManager
    {
        Object MovePreviousBase();
        Object MoveNextBase();
        Object PeekPreviousBase();
        Object PeekNextBase();
        String GetTextBase(Object Item);
    }
    public interface IMultiOptionManager<T>: IMultiOptionManager
    {
        T MovePrevious();
        T MoveNext();

        T PeekPrevious();
        T PeekNext();

        void SetCurrentIndex(int Index);
        String GetText(T Item);

        String GetTipText(T Item);

        T[] GetAllOptions();
    }
    public abstract class AbstractMultiOptionManager<T> : IMultiOptionManager<T>
    {
        public abstract string GetText(T Item);
        public abstract string GetTipText(T Item);

        public string GetTextBase(object Item)
        {
            return GetText((T)Item);
        }
        public String GetTipTextBase(object Item)
        {
            return GetTipTextBase((T)Item);
        }

        public abstract T MoveNext();

        public abstract T[] GetAllOptions();
        public object MoveNextBase()
        {
            return MoveNext();
        }

        public abstract T MovePrevious();
        

        public object MovePreviousBase()
        {
            return MovePrevious();
        }

        public abstract T PeekNext();
        

        public object PeekNextBase()
        {
            return PeekNext();
        }

        public abstract T PeekPrevious();
        

        public object PeekPreviousBase()
        {
            return PeekPrevious();
        }

        public abstract void SetCurrentIndex(int Index);
        
    }
    public class MultiOptionManagerList<T>: AbstractMultiOptionManager<T>
    {
        private int SelectedIndex;
        private T[] Options;
        public Func<T, String> GetItemTipTextFunc { get; set; }
        public override void SetCurrentIndex(int pIndex)
        {
            SelectedIndex = pIndex;
        }
        public MultiOptionManagerList(T[] pOptions,int pStartingIndex)
        {
            Options = pOptions;
            SelectedIndex = pStartingIndex;
        }
        public override string GetText(T Value)
        {
            return Value.ToString();
        }
        public override string GetTipText(T Item)
        {
            if (GetItemTipTextFunc == null) return null;
            return GetItemTipTextFunc(Item);
        }
        public override T[] GetAllOptions()
        {
            return Options;
        }
        public override T PeekPrevious()
        {
            var TestIndex = SelectedIndex - 1;
            if (TestIndex < 0) TestIndex = Options.Length - 1;
            return Options[TestIndex];
        }

        public override T PeekNext()
        {
            var TestIndex = SelectedIndex + 1;
            if (TestIndex > Options.Length-1) TestIndex = 0;
            return Options[TestIndex];
        }

        public override T MovePrevious()
        {
            SelectedIndex--;
            if (SelectedIndex < 0) SelectedIndex = Options.Length - 1;
            return Options[SelectedIndex];
        }
        public override T MoveNext()
        {
            SelectedIndex++;
            if (SelectedIndex > Options.Length-1) SelectedIndex = 0;
            return Options[SelectedIndex];
        }
    }
}
