using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BASeCamp.Rendering.Interfaces;

namespace BASeTris.Rendering
{
    //A rendering Provider is a class that accepts a Class Type, and a Data Element Type, and 
    //attempts to give back an appropriate Rendering Handler implementation for that class and element type.
    public interface IRenderingProvider:BASeCamp.Rendering.Interfaces.IRenderingProvider<IStateOwner>
    {
    }
   

}
