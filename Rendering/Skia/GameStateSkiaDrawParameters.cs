using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia
{
    public class GameStateSkiaDrawParameters
    {
        public SKRect Bounds;
        public Object TagData = null;
        public GameStateSkiaDrawParameters(SKRect pBounds)
        {
            Bounds = pBounds;
        }
    }

    public class GamePlayGameStateDataTagInfo
    {
        public bool SkipParticlePaint { get; set; }
    }
}

