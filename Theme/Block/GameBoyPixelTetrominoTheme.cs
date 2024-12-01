using BASeTris.GameStates.GameHandlers;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering.Skia.GameStates;
using BASeTris.Tetrominoes;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Theme.Block
{
    [HandlerTheme("GB (Faithful)", typeof(StandardTetrisHandler))]
    [ThemeDescription("GB Theme. Not inexplicably upscaled.")]
    public class GameBoyPixelTheme : CompositeBlockTheme
    {



        public override string Name => "Game Boy Theme (Pixel)";

        private GameBoyBigDotTheme _BigDot = new GameBoyBigDotTheme();
        private GameBoyDotTheme _Dot = new GameBoyDotTheme();
        private GameBoyBlockTheme _Block = new GameBoyBlockTheme();
        private GameBoyRaisedTheme _Raised = new GameBoyRaisedTheme();
        private GameBoyMottledTheme2 _Mottle = new GameBoyMottledTheme2();
        private GameBoyHoleTheme _Hole = new GameBoyHoleTheme();

        public override NominoTheme[] GetAllThemes()
        {
            return new NominoTheme[] {
            _BigDot,
            _Dot,_Block,_Raised,_Mottle,_Hole

            };
    }

        public override NominoTheme GetGroupTheme(Nomino Group, IBlockGameCustomizationHandler GameHandler, TetrisField Field)
        {
            return Group switch
            {
                Tetromino_I => _Mottle,
                Tetromino_O _ => _BigDot,
                Tetromino_J _ => _Hole,
                Tetromino_L _ => _Block,
                Tetromino_T _ => _Raised,
                Tetromino_Z => _Dot,
                Tetromino_S => _BigDot,
                _ => TetrisGame.Choose(GetAllThemes())
            };
        }
        private Bitmap LightImage = null;
        private PlayFieldBackgroundInfo _Cached = null;
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IBlockGameCustomizationHandler GameHandler)
        {

            return HandleBGCache(() =>
            {
                if (LightImage == null)
                {
                    //var borderbg = TetrisGame.Imageman.getLoadedImage("gb_border_image");



                    LightImage = new Bitmap(250, 250);
                    using (Graphics drawdark = Graphics.FromImage(LightImage))
                    {
                        drawdark.Clear(Color.PeachPuff);
                        drawdark.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                        //drawdark.DrawImageUnscaled(borderbg, new Point(0, 0));
                    }
                }
                var buildBG = StandardTetrisGameStateSkiaRenderingHandler.CreateBackgroundFromImage(LightImage, Color.Transparent);

                buildBG.Overlayer = new BackgroundDrawers.StandardImageBackgroundBorderSkia(new BackgroundDrawers.StandardImageBackgroundBorderSkia.BorderImageKeyData(null, "gb_border_brick_16", "gb_border_brick_16", "gb_border_brick_16", "gb_border_brick_16"));

                return new PlayFieldBackgroundInfo(LightImage, Color.Transparent) { SkiaBG = buildBG };
            });
        }
        public override MarginInformation GetDisplayMargins()
        {
            return new MarginInformation(16,0,16,0);
        }

    }

    public abstract class GameBoyCompositionThemeBase : ConnectedImageBlockTheme
    {
        protected override SKColor GetGroupBlockColor(Nomino Group)
        {

            return SKColors.LightGreen;
        }
        private Bitmap LightImage = null;
        
        public override PlayFieldBackgroundInfo GetThemePlayFieldBackground(TetrisField Field, IBlockGameCustomizationHandler GameHandler)
        {
            return HandleBGCache(() =>
            {
                if (LightImage == null)
                {
                    LightImage = new Bitmap(250, 500);
                    using (Graphics drawdark = Graphics.FromImage(LightImage))
                    {
                        drawdark.Clear(Color.PeachPuff);
                    }
                }
                return new PlayFieldBackgroundInfo(LightImage, Color.Transparent);
            });
            
        }
    }
    public class GameBoyBigDotTheme : GameBoyCompositionThemeBase
    {
        protected override string GetImageKeyBase()
        {
            return "gb_bigdot";
        }
    }

    public class GameBoyBlockTheme : GameBoyCompositionThemeBase
    {
        protected override string GetImageKeyBase()
        {
            return "gb_block";
        }
    }
    public class GameBoyDotTheme : GameBoyCompositionThemeBase
    {
        protected override string GetImageKeyBase()
        {
            return "gb_dot";
        }
    }
    public class GameBoyRaisedTheme : GameBoyCompositionThemeBase
    {
        protected override string GetImageKeyBase()
        {
            return "gb_raised";
        }
    }

    public class GameBoyHoleTheme : GameBoyCompositionThemeBase
    {
        protected override string GetImageKeyBase()
        {
            return "gb_hole";
        }
    }


    [HandlerTheme("Mottled", typeof(StandardTetrisHandler))]
    [ThemeDescription("Mottled Theme")]
    public class GameBoyMottledTheme2 : GameBoyCompositionThemeBase
    {
        public int GeneratedImageSize { get; set; } = 8;
        private static SKColor MottleColoring = new SKColor(0xad, 0x29, 0x21);
        private static SKColor MottleBG = new SKColor(0xde, 0x94, 0x4a);
        private static SKColor MottleLine = new SKColor(0x31, 0x18, 0x52);
        private static SKPaint LineDraw = new SKPaint() { Color = MottleLine, StrokeWidth = 1, StrokeCap = SKStrokeCap.Square, Style = SKPaintStyle.Stroke, StrokeJoin = SKStrokeJoin.Miter };
        public GameBoyMottledTheme2()
        {
            //if needed, we need to generate the images and add them to the image manager.
            if (!ThemeImagesGenerated)
            {
                ThemeImagesGenerated = true;
                GenerateThemeImages();
            }
        }




        private static bool ThemeImagesGenerated = false;
        private void GenerateThemeImages()
        {
            //to generate the Mottled images, we first create a set of possible 'mottlings'. these are 8x8 transparent images with random pixels coloured.
            SKImage[] Mottlings = GenerateMottlings(32).ToArray();

            foreach (var allconnect in CardinalConnectionSet.SuffixLookup.Keys)
            {
                String suseSuffix = CardinalConnectionSet.SuffixLookup[allconnect];
                String sGenerateKey = GetImageKeyBase() + "_block_connected_" + suseSuffix;
                if (String.IsNullOrEmpty(suseSuffix)) sGenerateKey = GetImageKeyBase() + "_block_normal";

                SKBitmap BuildImage = new SKBitmap(GeneratedImageSize, GeneratedImageSize);
                var usebase = TetrisGame.Choose(Mottlings);
                using (SKCanvas skc = new SKCanvas(BuildImage))
                {

                    skc.DrawImage(usebase, new SKPoint(0, 0));
                    DrawLines(skc, 8, InvertFlag(allconnect));

                }
                TetrisGame.Imageman[sGenerateKey] = SkiaSharp.Views.Desktop.Extensions.ToBitmap(BuildImage);
                if (allconnect == CardinalConnectionSet.ConnectedStyles.None) TetrisGame.Imageman[GetImageKeyBase()] = TetrisGame.Imageman[GetImageKeyBase() + "_block"] = TetrisGame.Imageman[sGenerateKey];
            }

        }
        private CardinalConnectionSet.ConnectedStyles InvertFlag(CardinalConnectionSet.ConnectedStyles s)
        {
            CardinalConnectionSet.ConnectedStyles result = CardinalConnectionSet.ConnectedStyles.None;
            foreach (CardinalConnectionSet.ConnectedStyles iterate in Enum.GetValues(typeof(CardinalConnectionSet.ConnectedStyles)))
            {

                if (iterate == CardinalConnectionSet.ConnectedStyles.None || iterate == CardinalConnectionSet.ConnectedStyles.MaxValue) continue;
                if (!s.HasFlag((Enum)iterate)) result |= iterate;
            }
            return result;


        }
        private void DrawLines(SKCanvas Target, int CanvasSize, CardinalConnectionSet.ConnectedStyles linepositions)
        {

            CanvasSize = CanvasSize - 1;
            //draw the lines on the specified positions on the target canvas.

            Target.DrawPoints(SKPointMode.Points, new SKPoint[] { new SKPoint(0, 0), new SKPoint(CanvasSize, 0), new SKPoint(CanvasSize, CanvasSize), new SKPoint(0, CanvasSize) }, LineDraw);


            if (linepositions.HasFlag(CardinalConnectionSet.ConnectedStyles.North))
            {
                //draw line on the top.
                Target.DrawLine(new SKPoint(0, 0), new SKPoint(CanvasSize, 0), LineDraw);
            }

            if (linepositions.HasFlag(CardinalConnectionSet.ConnectedStyles.West))
            {
                //line to the left
                Target.DrawLine(new SKPoint(0, 0), new SKPoint(0, CanvasSize), LineDraw);

            }
            if (linepositions.HasFlag(CardinalConnectionSet.ConnectedStyles.South))
            {
                Target.DrawLine(new SKPoint(0, CanvasSize), new SKPoint(CanvasSize, CanvasSize), LineDraw);
            }
            if (linepositions.HasFlag(CardinalConnectionSet.ConnectedStyles.East))
            {
                Target.DrawLine(new SKPoint(CanvasSize, 0), new SKPoint(CanvasSize, CanvasSize), LineDraw);
            }


        }


        private SKImage[] GenerateMottlings(int NumImages, int Size = 0, int NumPixels = 8)
        {
            Size = Size == 0 ? GeneratedImageSize : Size;

            return Enumerable.Range(0, NumImages - 1).Select((a) => GenerateMottling(Size, NumPixels)).ToArray();






        }
        private SKImage GenerateMottling(int Size = 8, int NumPixels = 8)
        {
            Size = Size == 0 ? GeneratedImageSize : Size;
            SKBitmap sbmp = new SKBitmap(Size, Size);
            using (SKCanvas g = new SKCanvas(sbmp))
            {
                g.Clear(MottleBG);

                var PoissonPoints = FastPoissonDiskSampling.Sampling(new SKPoint(0, 0), new SKPoint(7, 7), 2);

                foreach (var iterate in PoissonPoints)
                {
                    g.DrawPoint(iterate, MottleColoring);
                }
                /*for (int i = 0; i < NumPixels; i++)
                {
                    int XPosition = Random.Shared.Next(Size);
                    int YPosition = Random.Shared.Next(Size);
                    g.DrawPoint(new SKPoint(XPosition, YPosition), MottleColoring);

                }*/
            }
            return SKImage.FromBitmap(sbmp);



        }
        protected override string GetImageKeyBase()
        {
            return "gb_mottle";
        }
    }

}
