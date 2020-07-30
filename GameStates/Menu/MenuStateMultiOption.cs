using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
    public class MenuStateMultiOption<T> : MenuStateMultiOption
    {
        public IMultiOptionManager<T> OptionManager { get { return OptionManagerBase as IMultiOptionManager<T>; } set { OptionManagerBase = value; } }
        public event EventHandler<OptionActivated<T>> OnActivateOption;
        public event EventHandler<OptionActivated<T>> OnDeactivateOption;

        public T CurrentOption { get { return (T)CurrentOptionBase; } set { CurrentOptionBase = value; } }
        public MenuStateMultiOption(IMultiOptionManager<T> pOptionManager):base(pOptionManager)
        {
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
            OnDeactivateOption?.Invoke(this, new OptionActivated<T>(CurrentOption, LastOwner));
            _Activated = false;
            return MenuEventResultConstants.Handled;
        }

        private IStateOwner LastOwner = null;
        public override void ProcessGameKey(IStateOwner pStateOwner, GameState.GameKeys pKey)
        {
            LastOwner = pStateOwner;
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
    }
    public abstract class AbstractMultiOptionManager<T> : IMultiOptionManager<T>
    {
        public abstract string GetText(T Item);

        public string GetTextBase(object Item)
        {
            return GetText((T)Item);
        }

        public abstract T MoveNext();
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
