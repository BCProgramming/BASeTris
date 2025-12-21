using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.BackgroundDrawers
{
    public class CompositeDrawData : BackgroundDrawData
    {
        public class CompositeData
        {
            public IBackground Background { get; init; }
            public CompositeData(IBackground bg)
            {
                Background = bg;
            }
        }
        public CompositeData[] Backgrounds;
        public CompositeDrawData(params CompositeData[] pBackgrounds)
        {
            Backgrounds = pBackgrounds;
        }

    }
    public class CompositeBackground
    {
        public CompositeDrawData DrawData { get; init; }
        public CompositeBackground(CompositeDrawData pDrawData)
        {
            DrawData = pDrawData;
        }
    }
}
