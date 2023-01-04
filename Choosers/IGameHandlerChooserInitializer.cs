using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Choosers
{
    /// <summary>
    /// Interface for Game Handlers to initialize a Chooser based on what sort of Nominoes are part of that Game.
    /// </summary>
    public interface IGameHandlerChooserInitializer
    {
        BlockGroupChooser CreateSupportedChooser(Type DesiredChooserType);
    }
}
