using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Blocks;

namespace BASeTris.FieldInitializers
{
    public abstract class FieldInitializer
    {
        public abstract void Initialize(TetrisField Target);
    }
}