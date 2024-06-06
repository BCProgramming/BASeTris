using BASeCamp.Rendering;
using BASeTris.AssetManager;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering.Adapters;
using BASeTris.Theme.Block;
//using OpenTK.Platform.MacOS;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BASeTris.GameStates.Menu.MenuStateTextMenuItem;

namespace BASeTris.Rendering.Skia.MenuItems
{
    public interface ISizableMenuItemSkiaRenderingHandler
    {
        SKPoint GetSize(IStateOwner pOwner, MenuStateSizedMenuItem item);
    }

    [RenderingHandler(typeof(MenuStateMenuItem), typeof(SKCanvas), typeof(MenuStateMenuItemSkiaDrawData))]
    public class MenuStateMenuItemSkiaRenderer : IRenderingHandler<SKCanvas, MenuStateMenuItem, MenuStateMenuItemSkiaDrawData>
    {
        static Dictionary<String, SKImage> RenderedNominoElements = new Dictionary<String, SKImage>();
        
        
      
        private SKImage GetSelectionNomino(MenuStateMenuItem item)
        {
            String sKey = "";
            if (item is MenuStateTextMenuItem text)
                sKey = text.Text;

            if (!RenderedNominoElements.ContainsKey(sKey))
            {
                NominoTheme chosen = TetrisGame.Choose<Func<NominoTheme>>(NominoTheme.GetVisualizationThemes())();
                using (var SelectionNominoA = TetrominoCollageRenderer.GetNominoBitmap(chosen))
                {
                    using (var SelectionNominoB = ImageManager.ReduceImageSK(SelectionNominoA, new SKSizeI((int)(SelectionNominoA.Width / 10), (int)(SelectionNominoA.Height / 10))))
                    {

                        using (var SelectionNominoC = TetrisGame.OutlineImageSK(SelectionNominoB, 10))
                        {
                            RenderedNominoElements.Add(sKey, SKImage.FromBitmap(SelectionNominoC));
                        }
                    }
                }
            }
            return RenderedNominoElements[sKey];
            
        }
        public delegate void TransitionFunctionDelegate(SKCanvas pRenderTarget, MenuStateMenuItem Source, MenuStateMenuItemSkiaDrawData Element);
        public TransitionFunctionDelegate TransitionFunction { get; set; } = StaggeredTranslationTransitionFunction;
        public static void StaggeredTranslationTransitionFunction(SKCanvas pRenderTarget, MenuStateMenuItem Source, MenuStateMenuItemSkiaDrawData Element)
        {
            if (Source.TransitionPercentage < 1)
            {
                if (Element.Index % 2 == 0)
                {
                    pRenderTarget.Translate((float)((-Element.Bounds.Width) + ((Element.Bounds.Width ) * Source.TransitionPercentage)), 0);
                }
                else
                {
                    pRenderTarget.Translate((float)((Element.Bounds.Width) - ((Element.Bounds.Width ) * Source.TransitionPercentage)), 0);
                    //   pRenderTarget.Translate((float)((Element.Bounds.Right+Element.Bounds.Width / 2) - ((Element.Bounds.Width / 2) * Source.TransitionPercentage)), 0);
                }
            }



        }
        public void MenuTransitioner(Action CallRoutine,TransitionFunctionDelegate TransitionAction, SKCanvas pRenderTarget, MenuStateMenuItem Source, MenuStateMenuItemSkiaDrawData Element)
        {
            using (SKAutoCanvasRestore skc = new SKAutoCanvasRestore(pRenderTarget, true))
            {
                TransitionAction(pRenderTarget,Source,Element);
                CallRoutine();
            }
        }

        public void Render(IStateOwner pOwner, SKCanvas pRenderTarget, MenuStateMenuItem Source, MenuStateMenuItemSkiaDrawData Element)
        {


          

                var SelectionNomino = GetSelectionNomino(Source);
                Source.LastBounds = Element.Bounds;
                //draw a selection thingie
                if (Element.DrawState == MenuStateMenuItem.StateMenuItemState.State_Selected)
                {
                    SKPoint Origin = new SKPoint(Element.Bounds.Left - Element.Bounds.Height, Element.Bounds.Top);
                    SKPoint Origin2 = new SKPoint(Element.Bounds.Right, Element.Bounds.Top);
                    float HeightFactor = (float)SelectionNomino.Width / (float)SelectionNomino.Height;
                    SKRect useRect = new SKRect(Origin.X, Origin.Y, Origin.X + Element.Bounds.Height, Origin.Y + (Element.Bounds.Height / HeightFactor));
                    SKRect useRect2 = new SKRect(Origin2.X, Origin2.Y, Origin2.X + Element.Bounds.Height, Origin2.Y + (Element.Bounds.Height / HeightFactor));
                    int InflationAmount = 20;
                    useRect.Inflate(new SKSize(InflationAmount, InflationAmount / HeightFactor));
                    useRect2.Inflate(new SKSize(InflationAmount, InflationAmount / HeightFactor));
                    SKPoint CenterPosition = new SKPoint(useRect.Left + useRect.Width / 2, useRect.Top + useRect.Height / 2);
                    SKPoint CenterPosition2 = new SKPoint(useRect2.Left + useRect2.Width / 2, useRect2.Top + useRect2.Height / 2);

                    //SKRect useRect = Element.Bounds;
                    //pRenderTarget.DrawRect(useRect, new SKPaint() { Color = SKColors.Red });

                    using (SKAutoCanvasRestore rest = new SKAutoCanvasRestore(pRenderTarget, true))
                    {

                        //pRenderTarget.Translate(new SKPoint(-CenterPosition.X, -CenterPosition.Y));
                        pRenderTarget.RotateDegrees(360 * ((float)DateTime.Now.Millisecond / 1000), CenterPosition.X, CenterPosition.Y);

                        //pRenderTarget.Translate(CenterPosition);
                        pRenderTarget.DrawImage(SelectionNomino, useRect);

                    }

                    using (SKAutoCanvasRestore rest = new SKAutoCanvasRestore(pRenderTarget, true))
                    {

                        //pRenderTarget.Translate(new SKPoint(-CenterPosition.X, -CenterPosition.Y));
                        pRenderTarget.RotateDegrees(360 * (1 - ((float)DateTime.Now.Millisecond / 1000)), CenterPosition2.X, CenterPosition2.Y);

                        //pRenderTarget.Translate(CenterPosition);
                        pRenderTarget.DrawImage(SelectionNomino, useRect2);

                    }

                    

                    //pRenderTarget.DrawImage(SelectionNomino,new SKRect(0,0,50,50),new SKPaint() { } );
                    //pRenderTarget.DrawImage(SelectionNomino, new SKRect(50, 50, 100, 100), new SKPaint() { });
                    //pRenderTarget.DrawImage(SelectionNomino, new SKRect(200, 200, 200, 200), new SKPaint() { });
                    //pRenderTarget.DrawImage(SelectionNomino, new SKPoint(0, 0));
                    // pRenderTarget.DrawCircle(new SKPoint(Element.Bounds.Left - 40, Element.Bounds.Top + Element.Bounds.Height / 2), 40, new SKPaint() { Color = SKColors.Black });
                }
                DrawItemLabel(pOwner, pRenderTarget, Source, Element);




        }
        public virtual void DrawItemLabel(IStateOwner pOwner, SKCanvas pRenderTarget, MenuStateMenuItem Source, MenuStateMenuItemSkiaDrawData Element)
        {
            if (!String.IsNullOrWhiteSpace(Source.Label))
            {
                using SKPaint textforeground = new SKPaint() { Typeface = TetrisGame.RetroFontSK, TextSize = Element.Bounds.Height / 4, Color = SKColors.Black };
                using SKPaint textshadow = new SKPaint() { Typeface = TetrisGame.RetroFontSK, TextSize = Element.Bounds.Height / 4, Color = SKColors.White };
                RenderHelpers.SetPaintBlur(textshadow);
                DrawTextInformationSkia tTitle = new DrawTextInformationSkia() { ForegroundPaint = textforeground, ShadowPaint = textshadow };
                DrawTextInformationSkia dtis = new DrawTextInformationSkia() { ForegroundPaint = textforeground, ShadowPaint = textshadow };
                SKRect bnd = new SKRect();
                textforeground.MeasureText(dtis.Text, ref bnd);
                tTitle.DrawFont = new SKFontInfo(TetrisGame.RetroFontSK, textforeground.TextSize);
                tTitle.Text = Source.Label;


                
                tTitle.Position = new SKPoint((float)(Element.Bounds.Left + 10 * pOwner.ScaleFactor), Element.Bounds.Top + Element.Bounds.Height/2);
                //if (Source.Activated)
                // {
                //     tTitle.CharacterHandler = new DrawCharacterHandlerSkia(new JitterCharacterPositionCalculatorSkia { Height = (float)(pOwner.ScaleFactor * 6) });
                // }
                pRenderTarget.DrawTextSK(tTitle);
            }
        }
        public virtual void Render(IStateOwner pOwner, object pRenderTarget, object RenderSource, object Element)
        {
            Render(pOwner, (SKCanvas)pRenderTarget, (MenuStateMenuItem)RenderSource, (MenuStateMenuItemSkiaDrawData)Element);


        }
        public SKPoint GetSize(IStateOwner pOwner, MenuStateMenuItem Source)
        {
            if (Source is MenuStateSizedMenuItem sizer)
            {
                var RealRenderer = RenderingProvider.Static.GetHandler(Source.GetType(), typeof(SKCanvas), typeof(MenuStateMenuItemSkiaDrawData));
                if (RealRenderer != null)
                {
                    if (RealRenderer is ISizableMenuItemSkiaRenderingHandler sizerender)
                    {
                        return sizerender.GetSize(pOwner, sizer);
                    }
                }
            }
            return SKPoint.Empty;

        }
    }
    [RenderingHandler(typeof(MenuStateSliderOption), typeof(SKCanvas), typeof(MenuStateMenuItemSkiaDrawData))]
    public class MenuStateSliderOptionSkiaRenderer : MenuStateMenuItemSkiaRenderer, IRenderingHandler<SKCanvas, MenuStateSliderOption, MenuStateMenuItemSkiaDrawData> , ISizableMenuItemSkiaRenderingHandler
    {
        private static SKImage SliderImage = null;
        private static SKImage SliderBG = null;
        public MenuStateSliderOptionSkiaRenderer() : base()
        {
            
        }
        public SKPoint GetSize(IStateOwner pOwner, MenuStateSizedMenuItem item)
        {
            return new SKPoint((float)pOwner.ScaleFactor * 100, (float)pOwner.ScaleFactor * 20);
        }
        public override void Render(IStateOwner pOwner, object pRenderTarget, object RenderSource, object Element)
        {
            Render(pOwner, (SKCanvas)pRenderTarget, (MenuStateSliderOption)RenderSource, (MenuStateMenuItemSkiaDrawData)Element);
        }
        //Would be a good idea to redesign this to draw using tetromino elements. Long piece for a slider indicator for example. Maybe little tetrominoes for the border and detents?
        public void Render(IStateOwner pOwner, SKCanvas pRenderTarget, MenuStateSliderOption Source, MenuStateMenuItemSkiaDrawData Element)
        {

            MenuTransitioner(() =>
            {

                if (SliderImage == null) SliderImage = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap("slider_pointer"));
                if (SliderBG == null) SliderBG = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap("slider_bg"));
                double ShadowOffset = 1 * pOwner.ScaleFactor;
                SKColor ForeColor = SKColors.Magenta;
                SKColor ShadowColor = SKColors.Magenta;
                if (Element.DrawState == MenuStateMenuItem.StateMenuItemState.State_Normal)
                {
                    ForeColor = SKColors.Black;
                    ShadowColor = SKColors.White;
                }
                else if (Element.DrawState == MenuStateMenuItem.StateMenuItemState.State_Selected)
                {
                    ForeColor = SKColors.White;
                    ShadowColor = SKColors.Black;
                    using (SKAutoCanvasRestore sk = new SKAutoCanvasRestore(pRenderTarget))
                    {
                        pRenderTarget.ClipRect(Element.Bounds, SKClipOperation.Intersect, false);
                        pRenderTarget.DrawColor(SKColors.OrangeRed, SKBlendMode.ColorBurn);
                    }
                }
                else if (Element.DrawState == MenuStateMenuItem.StateMenuItemState.State_Unavailable)
                {
                    ForeColor = SKColors.Gray;
                    ShadowColor = SKColors.DarkGray;
                }

                SKPaint ForePaint = new SKPaint() { IsStroke = true, Color = ForeColor, StrokeWidth = 3 };
                SKPaint ShadowPaint = new SKPaint() { IsStroke = true, Color = ShadowColor, StrokeWidth = 3 };
                SKPaint ForeFill = new SKPaint() { Color = ForeColor, Style = SKPaintStyle.Fill, StrokeWidth = 7 };
                SKPaint ShadowFill = new SKPaint() { Color = ShadowColor, Style = SKPaintStyle.Fill, StrokeWidth = 7 };

                double MiddleY = Element.Bounds.Top + Element.Bounds.Height / 2;
                //step one
                // var shadowoff = Element.Bounds;
                // shadowoff.Offset((float)ShadowOffset, (float)ShadowOffset);
                // pRenderTarget.DrawRect(shadowoff, ShadowPaint );
                // pRenderTarget.DrawRect(Element.Bounds,ForePaint);
                //pRenderTarget.DrawImage(SliderBG, Element.Bounds);

                double FullRange = (Source.MaximumValue - Source.MinimumValue);
                int detentCount = 0;
                for (double detent = Source.MinimumValue; detent < Source.MaximumValue; detent += Source.SmallDetent)
                {
                    //calculate our position for this detent.

                    double XPosition = Element.Bounds.Left + Element.Bounds.Width * ((detent - Source.MinimumValue) / FullRange);

                    double detentSize = Math.Abs(detentCount % Source.LargeDetentCount) < 0.001 ? Element.Bounds.Height / 5 : Element.Bounds.Height / 10;
                    double Ypos = Element.Bounds.Top + Element.Bounds.Height / 2 - detentSize / 2;

                    DrawLine(pRenderTarget, new SKPoint((float)XPosition, (float)Ypos), new SKPoint((float)XPosition, (float)(Ypos + (float)detentSize)), ForePaint, ShadowPaint, ShadowOffset);


                    detentCount++;
                }
                double SliderPosition = Element.Bounds.Left + Element.Bounds.Width * ((Source.Value - Source.MinimumValue) / FullRange);



                float SliderWidth = 10;
                var shadoffset = new SKPoint((float)ShadowOffset, (float)ShadowOffset);
                pRenderTarget.DrawImage(SliderImage, new SKRect((float)(SliderPosition - SliderWidth), Element.Bounds.Top, (float)(SliderPosition + SliderWidth), Element.Bounds.Top + Element.Bounds.Height / 2), null);

                SKPaint textforeground = new SKPaint() { Typeface = TetrisGame.RetroFontSK, TextSize = Element.Bounds.Height / 4, Color = SKColors.Black };
                SKPaint textshadow = new SKPaint() { Typeface = TetrisGame.RetroFontSK, TextSize = Element.Bounds.Height / 4, Color = SKColors.White };
                DrawTextInformationSkia dtis = new DrawTextInformationSkia() { ForegroundPaint = textforeground, ShadowPaint = textshadow };
                dtis.DrawFont = new SKFontInfo(TetrisGame.RetroFontSK, Element.Bounds.Height / 4);
                dtis.Text = Source.Value.ToString("0.##");
                dtis.Position = new SKPoint((float)SliderPosition + SliderWidth, Element.Bounds.Top + Element.Bounds.Height * 0.15f);
                if (Source.Activated)
                {
                    (textforeground, textshadow) = (textshadow, textforeground);
                    dtis.CharacterHandler = new DrawCharacterHandlerSkia(new JitterCharacterPositionCalculatorSkia { Height = (float)(pOwner.ScaleFactor * 6) });
                }
                pRenderTarget.DrawTextSK(dtis);



                
                SKRect bnd = new SKRect();
                textforeground.MeasureText(dtis.Text, ref bnd);
                

                base.Render(pOwner, pRenderTarget, Source, Element);


                //pRenderTarget.DrawPoints(SKPointMode.Polygon, SliderShadow, ShadowFill);
                //pRenderTarget.DrawPoints(SKPointMode.Polygon, SliderPoly, ForeFill);
                //FillPoly(pRenderTarget, SliderShadow, ShadowFill);
                //FillPoly(pRenderTarget, SliderPoly, ForeFill);
            }, TransitionFunction, pRenderTarget, Source, Element);




        }
        public override void DrawItemLabel(IStateOwner pOwner, SKCanvas pRenderTarget, MenuStateMenuItem Src, MenuStateMenuItemSkiaDrawData Element)
        {
            using SKPaint textforeground = new SKPaint() { Typeface = TetrisGame.RetroFontSK, TextSize = Element.Bounds.Height / 4, Color = SKColors.Black };
            using SKPaint textshadow = new SKPaint() { Typeface = TetrisGame.RetroFontSK, TextSize = Element.Bounds.Height / 4, Color = SKColors.White };
            DrawTextInformationSkia tTitle = new DrawTextInformationSkia() { ForegroundPaint = textforeground, ShadowPaint = textshadow };
            if (Src is MenuStateSliderOption Source)
            {
                tTitle.DrawFont = new SKFontInfo(TetrisGame.RetroFontSK, textforeground.TextSize);
                tTitle.Text = Source.Label;



                tTitle.Position = new SKPoint((float)(Element.Bounds.Left + 10 * pOwner.ScaleFactor), Element.Bounds.Top + Element.Bounds.Height);
                if (Source.Activated)
                {
                    tTitle.CharacterHandler = new DrawCharacterHandlerSkia(new JitterCharacterPositionCalculatorSkia { Height = (float)(pOwner.ScaleFactor * 6) });
                }
                pRenderTarget.DrawTextSK(tTitle);

            }
        }

        private void FillPoly(SKCanvas Target, SKPoint[] array, SKPaint fill)
        {
            using (SKPath skp = new SKPath() { FillType = SKPathFillType.Winding,  Convexity = SKPathConvexity.Concave })
            {
                skp.MoveTo(array.First());
                for (var index = 1; index < array.Length; index++)
                {
                    skp.LineTo(array[index]);
                }
                skp.Close();
                Target.DrawPath(skp, fill);
            }



        }
        private void DrawLine(SKCanvas Target, SKPoint A, SKPoint B, SKPaint Fore, SKPaint Shadow, double ShadowOffset)
        {
            SKPoint offset = new SKPoint((float)ShadowOffset, (float)ShadowOffset);
            Target.DrawLine(A + offset, B + offset, Shadow);
            Target.DrawLine(A, B, Shadow);


        }
        
    }

    [RenderingHandler(typeof(MenuStateTextMenuItem), typeof(SKCanvas), typeof(MenuStateMenuItemSkiaDrawData))]
    public class MenuStateTextMenuItemSkiaRenderer : MenuStateMenuItemSkiaRenderer ,IRenderingHandler<SKCanvas, MenuStateTextMenuItem, MenuStateMenuItemSkiaDrawData>, ISizableMenuItemSkiaRenderingHandler
    {
        //protected Graphics Temp = Graphics.FromImage(new Bitmap(1, 1));
        /*static Dictionary<double, Dictionary<String, SKFontInfo>> FontSizeData = new Dictionary<double, Dictionary<String, SKFontInfo>>();
         
        public static SKFontInfo GetScaledFont(IStateOwner pOwner, String FontFace, float FontSize)
        {
            float findSize = (float)(FontSize * pOwner.ScaleFactor);

            return GetFont(FontFace, findSize);
        }
        public static SKFontInfo GetFont(String FontFace, float FontSize)
        {
            lock (FontSizeData)
            {
                if (!FontSizeData.ContainsKey(FontSize))
                {
                    var newdict = new Dictionary<String, SKFontInfo>(StringComparer.OrdinalIgnoreCase);
                    FontSizeData.Add(FontSize, newdict);
                }
                var SizeDict = FontSizeData[FontSize];
                if (!SizeDict.ContainsKey(FontFace))
                {
                    Font buildfont = new SKFontInfo(FontFace, FontSize);
                    SizeDict.Add(FontFace, buildfont);
                }
                return SizeDict[FontFace];
            }
        }*/
        
        public static SKFontInfo GetFont(float FontSize)
        {
            return new SKFontInfo(TetrisGame.RetroFontSK, FontSize);
        }
        public static SKFontInfo GetScaledFont(IStateOwner powner,float FontSize)
        {
            return GetFont((float)(FontSize * powner.ScaleFactor));
        }
        public virtual SKPoint GetSize(IStateOwner pOwner, MenuStateSizedMenuItem Source)
        {
            return GetSize(pOwner, (MenuStateTextMenuItem)Source);
        }
        public virtual SKPoint GetSize(IStateOwner pOwner, MenuStateTextMenuItem Source)
        {
            //TODO- implement
            SKPaint MeasurePaint = new SKPaint();
            var RetroFont = TetrisGame.RetroFontSK;
            MeasurePaint.Typeface = RetroFont;
            MeasurePaint.TextSize = Source.FontSize;
            SKRect result = new SKRect();
            
            MeasurePaint.MeasureText(new String(Enumerable.Repeat('█',(Source.Text??"").Length).ToArray()), ref result);
            return new SKPoint(result.Width, result.Height);
            //var testfont = GetScaledFont(pOwner, Source.FontFace, Source.FontSize);
            //var MeasureText = Temp.MeasureString(Source.Text, testfont);
            //return MeasureText.ToSize();
        }

        private static float GetDrawX(SKRect pBounds, SKRect DrawSize, MenuHorizontalAlignment pAlign)
        {
            switch (pAlign)
            {
                case MenuHorizontalAlignment.Center:
                    return (pBounds.Left + pBounds.Width / 2) - DrawSize.Width / 2;
                case MenuHorizontalAlignment.Left:
                    return pBounds.Left;
                case MenuHorizontalAlignment.Right:
                    return pBounds.Right - DrawSize.Height;
            }
            return 0;
        }
        private static SKPoint GetDrawPosition(SKRect pBounds, SKRect DrawSize, MenuHorizontalAlignment pAlign)
        {
            float useX = GetDrawX(pBounds, DrawSize, pAlign);
            float useY = (pBounds.Top + pBounds.Height / 2 - (DrawSize.Height / 2));
            return new SKPoint(useX, useY+DrawSize.Height);
        }
        private static SKPaint TransparentPaint = new SKPaint() { Color = SKColors.Transparent };
        public void Render(IStateOwner pOwner, SKCanvas pRenderTarget, MenuStateTextMenuItem Source, MenuStateMenuItemSkiaDrawData Element)
        {

            MenuTransitioner(() =>
            {

                base.Render(pOwner, pRenderTarget, Source, Element);
                var useFont = GetScaledFont(pOwner, Source.FontSize);
                var MeasureText = TetrisGame.MeasureSKText(useFont.TypeFace, useFont.FontSize, Source.Text);


                SKPoint DrawPosition = GetDrawPosition(Element.Bounds, MeasureText, Source.TextAlignment);
                SKPaint BackPaint = null;


                if (Element.DrawState == MenuStateMenuItem.StateMenuItemState.State_Selected)
                    BackPaint = new SKPaint() { Color = SKColors.DarkGreen, BlendMode = SKBlendMode.HardLight };
                else
                    BackPaint = new SKPaint() { Color = SkiaSharp.Views.Desktop.Extensions.ToSKColor(Source.BackColor) };
                pRenderTarget.DrawRect(Element.Bounds, BackPaint);


                SKPaint ForePaint = null;
                SKPaint ShadePaint = null;
                if (Element.DrawState == MenuStateMenuItem.StateMenuItemState.State_Selected)
                    ForePaint = new SKPaint() { Color = SKColors.Aqua };
                else
                    ForePaint = new SKPaint() { Color = SkiaSharp.Views.Desktop.Extensions.ToSKColor(Source.ForeColor), TextAlign = SKTextAlign.Center };

                ShadePaint = new SKPaint() { Color = SkiaSharp.Views.Desktop.Extensions.ToSKColor(Source.ShadowColor), TextAlign = SKTextAlign.Center };


                ForePaint.Typeface = useFont.TypeFace;
                ForePaint.TextSize = (int)(Source.FontSize * pOwner.ScaleFactor);
                ShadePaint.Typeface = useFont.TypeFace;
                ShadePaint.TextSize = (int)(Source.FontSize * pOwner.ScaleFactor);
                //RenderHelpers.SetPaintBlur(ShadePaint);
                var useStyle = new DrawTextInformationSkia()
                {
                    Text = Source.Text,
                    BackgroundPaint = new SKPaint() { Color = SKColors.Transparent },
                    DrawFont = useFont,
                    ForegroundPaint = ForePaint,
                    ShadowPaint = ShadePaint,
                    Position = new SKPoint(DrawPosition.X, DrawPosition.Y + MeasureText.Height / 2),
                    ShadowOffset = new SKPoint(5f, 5f),
                };
                if (Source.TransitionPercentage < 1)
                {
                    useStyle.CharacterHandler.SetPositionCalculator(new RotatingPositionCharacterPositionCalculatorSkia() { Radius = (float)(Element.Bounds.Width - (Element.Bounds.Width * Source.TransitionPercentage)) });
                }

                else if (Element.DrawState == MenuStateMenuItem.StateMenuItemState.State_Selected)
                {
                    useStyle.CharacterHandler.SetPositionCalculator(new RotatingPositionCharacterPositionCalculatorSkia());
                }

                


                pRenderTarget.DrawTextSK(useStyle);

                //draw an arrow if necessary.
                if (Source.MenuFlags.HasFlag(AdditionalMenuFlags.MenuFlags_ShowSubmenuArrow))
                {
                    //this is really broken, but it makes no sense why it would be. tiny fucking arrows and stupid fucking math bullshit. I fucking copied the Y location from above!
                    /*
                    useStyle.CharacterHandler.ClearPositionCalculators();
                    var MeasuredArrow = TetrisGame.MeasureSKText(SKTypeface.FromFamilyName("Marlett"), (float)(32f* pOwner.ScaleFactor), "8");
                    useStyle.Position = new SKPoint(Element.Bounds.Right - MeasuredArrow.Width, (float)(Element.Bounds.Top + (Math.Abs(Element.Bounds.Height))));
                    useStyle.Text = "8";
                    useStyle.ScalePercentage = 2;
                    useStyle.DrawFont = new SKFontInfo(SKTypeface.FromFamilyName("Marlett"), (float)(32f*pOwner.ScaleFactor));
                    pRenderTarget.DrawTextSK(useStyle);
                    //useStyle.Position = new SKPoint(50, 50);
                    //pRenderTarget.DrawTextSK(useStyle);

                    */




                }
            }, StaggeredTranslationTransitionFunction, pRenderTarget, Source, Element);
            
            
            

            //            TetrisGame.DrawText(Target, useFont, Text, ForeBrush, ShadowBrush, DrawPosition.X, DrawPosition.Y, 5f, 5f, central);

            //Cheating...
            // Source.Draw(pOwner,pRenderTarget,Element.Bounds,Element.DrawState);
        }
        const char MarlettRightArrow = '8';
        public override void Render(IStateOwner pOwner, object pRenderTarget, object RenderSource, object Element)
        {
            Render(pOwner, (SKCanvas)pRenderTarget, (MenuStateTextMenuItem)RenderSource, (MenuStateMenuItemSkiaDrawData)Element);
        }
    }
    [RenderingHandler(typeof(MenuStateMultiOption), typeof(SKCanvas), typeof(MenuStateMenuItemSkiaDrawData))]


    public class MenuStateMultiOptionItemSkiaRenderer : MenuStateTextMenuItemSkiaRenderer, IRenderingHandler<SKCanvas, MenuStateMultiOption, MenuStateMenuItemSkiaDrawData>, ISizableMenuItemSkiaRenderingHandler
    {

        public override SKPoint GetSize(IStateOwner pOwner, MenuStateSizedMenuItem Source)
        {
            return GetSize(pOwner, (MenuStateMultiOption)Source);
        }
        public SKPoint GetSize(IStateOwner pOwner, MenuStateMultiOption Source)
        {
            return base.GetSize(pOwner, Source);
        }
        public override void Render(IStateOwner pOwner, object pRenderTarget, object RenderSource, object Element)
        {
            Render(pOwner, (SKCanvas)pRenderTarget, (MenuStateMultiOption)RenderSource, (MenuStateMenuItemSkiaDrawData)Element);
        }
        public void Render(IStateOwner pOwner, SKCanvas pRenderTarget, MenuStateMultiOption Source, MenuStateMenuItemSkiaDrawData Element)
        {
            SKFontInfo useFont = MenuStateTextMenuItemSkiaRenderer.GetScaledFont(pOwner, Source.FontSize);
            var OptionManager = Source.OptionManagerBase;
            var Bounds = Element.Bounds;
            String sLeftCover = "< ";
            String sRightCover = ">";
            String PrevItem = null, NextItem = null;
            try
            {
                PrevItem = OptionManager.GetTextBase(OptionManager.PeekPreviousBase());
                NextItem = OptionManager.GetTextBase(OptionManager.PeekNextBase());
            }
            catch (IndexOutOfRangeException ire)
            {
                PrevItem = "";
                NextItem = "";
            }
            sLeftCover = PrevItem + sLeftCover;
            sRightCover = sRightCover + NextItem;
            
            SKPaint Foreground = new SKPaint() { Color = SkiaSharp.Views.Desktop.Extensions.ToSKColor(Source.ForeColor) };
            SKPaint Background = new SKPaint() { Color = SkiaSharp.Views.Desktop.Extensions.ToSKColor(Source.BackColor) };
            SKPaint Shadow = new SKPaint() { Color = SkiaSharp.Views.Desktop.Extensions.ToSKColor(Source.ShadowColor) };
            DrawTextInformationSkia dtis = new DrawTextInformationSkia() { ForegroundPaint = Foreground, BackgroundPaint = Background, ShadowPaint = Shadow };
            dtis.DrawFont = new SKFontInfo(TetrisGame.RetroFontSK, Source.FontSize);
            //TODO: need to get this implemented via a SKPaint, but the TextMenu item should probably have draw data in an "abstracted" form...
            SKRect MeasureLeft = new SKRect();
            SKRect MeasureRight = new SKRect();
            Foreground.MeasureText(sLeftCover,ref MeasureLeft);
            
            Foreground.MeasureText(sRightCover,ref MeasureRight);
            //var MeasureLeft = pRenderTarget.MeasureString(sLeftCover, useFont);
            //var MeasureRight = pRenderTarget.MeasureString(sRightCover, useFont);
            SKPoint LeftPos = new SKPoint(Bounds.Left - MeasureLeft.Width, Bounds.Top + (Bounds.Height / 2) - MeasureLeft.Height / 2);
            SKPoint RightPos = new SKPoint(Bounds.Right, Bounds.Top + (Bounds.Height / 2) - MeasureRight.Height / 2);

            
            if (Source.Activated)
            {
                dtis.Text = sLeftCover;
                pRenderTarget.DrawTextSK(dtis);
                dtis.Text = sRightCover;
                pRenderTarget.DrawTextSK(dtis);
            }
            base.Render(pOwner, pRenderTarget, Source, Element);
        }
    }


}
