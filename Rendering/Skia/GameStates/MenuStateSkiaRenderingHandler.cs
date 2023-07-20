
using BASeCamp.Rendering;
using BASeTris.BackgroundDrawers;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering.Adapters;
using BASeTris.Rendering.Skia.MenuItems;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia.GameStates
{
    [RenderingHandler(typeof(TitleMenuState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class TitleMenuSkiaRenderingHandler : AbstractMenuStateSkiaRenderingHandler<TitleMenuState>
    {
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, TitleMenuState Source, GameStateSkiaDrawParameters Element)
        {
            base.Render(pOwner, pRenderTarget, Source, Element);
            //custom addition should draw tetrominoes around the border. Primarily, the corners.
        }


    }
    



    [RenderingHandler(typeof(MenuState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class MenuStateSkiaRenderingHandler : AbstractMenuStateSkiaRenderingHandler<MenuState>
    {
    }

    public abstract class AbstractMenuStateSkiaRenderingHandler<TSourceType> : StandardStateRenderingHandler<SKCanvas, TSourceType, GameStateSkiaDrawParameters> where TSourceType : MenuState
    {
        
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, TSourceType Source, GameStateSkiaDrawParameters Element)
        {
            //draw the header text,
            //then draw each menu item.
            //throw new NotImplementedException();
            SKCanvas g = pRenderTarget;
            var Bounds = Element.Bounds;

            if (Source.FadedBGFadeState != null && Source.FadedBGFadeState.FadedParentState!=null)
            {
                
                RenderingProvider.Static.DrawElement(pOwner, g, Source.FadedBGFadeState.FadedParentState, Element);
                g.DrawRect(Bounds, new SKPaint() { Color = new SKColor(255, 255, 255, 128) });
            }

            else if (Source.BG != null)
            {
                RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source.BG, new SkiaBackgroundDrawData(Bounds));
            }
            int CurrentIndex = Source.StartItemOffset;
            float CurrentY = DrawHeader(pOwner, Source, g, Bounds);
            float MaxHeight = 0, MaxWidth = 0;

            //we want to find the widest item.
            foreach (var searchitem in Source.MenuElements)
            {
                if (searchitem is MenuStateSliderOption)
                {
                    ;
                }
                if (searchitem is MenuStateSizedMenuItem mss)
                {
                    var sizehandler = RenderingProvider.Static.GetHandler(typeof(SKCanvas), searchitem.GetType(), typeof(MenuStateMenuItemSkiaDrawData));
                    if (sizehandler is ISizableMenuItemSkiaRenderingHandler isizer)

                    {
                        var grabsize = isizer.GetSize(pOwner, mss);
                        grabsize = new SKPoint((float)(grabsize.X * pOwner.ScaleFactor*2), (float)(grabsize.Y * pOwner.ScaleFactor*2));
                        if (grabsize.Y > MaxHeight) MaxHeight = grabsize.Y;
                        if (grabsize.X > MaxWidth) MaxWidth = grabsize.X;

                    }

                }
            }
            //we draw each item at the maximum size.
            SKPoint ItemSize = new SKPoint(MaxWidth, MaxHeight);
            CurrentY += (float)(pOwner.ScaleFactor * 5);
            for (int menuitemindex = 0; menuitemindex < Source.MenuElements.Count; menuitemindex++)
            {

                try
                {
                    var drawitem = Source.MenuElements[menuitemindex];
                   
                    var XPos = (int)(Bounds.Width / 2 - ItemSize.X / 2) + Source.MainXOffset;
                    SKRect TargetBounds = new SKRect(XPos, (int)CurrentY, XPos + (int)(ItemSize.X), CurrentY + (int)(ItemSize.Y));
                    MenuStateMenuItem.StateMenuItemState useState = menuitemindex == Source.SelectedIndex ? MenuStateMenuItem.StateMenuItemState.State_Selected : MenuStateMenuItem.StateMenuItemState.State_Normal;
                    RenderingProvider.Static.DrawElement(pOwner, g, drawitem, new MenuStateMenuItemSkiaDrawData(TargetBounds, useState));
                }
                catch (Exception exp)
                {

                }
                //drawitem.Draw(pOwner, g, TargetBounds, useState);
                CurrentY += ItemSize.Y + 5;
            }
           
            //draw the footer

            if (!String.IsNullOrEmpty(Source.FooterText))
            {
                String[] sFooterLines = Source.FooterText.Split('\n');
                using (var skpFooterFore = new SKPaint(){ Color = SKColors.Black })
                using (var skpFooterBack = new SKPaint() { Color = SKColors.Yellow })
                {
                    float totalHeight = 0;
                    float UseY = Bounds.Height;


                    var useFooterFont = GetScaledFooterFont(pOwner, Source);
                    skpFooterFore.Typeface = skpFooterBack.Typeface = useFooterFont.TypeFace;
                    skpFooterFore.TextSize = skpFooterBack.TextSize = (float)(useFooterFont.FontSize);
                    SKRect FooterSize = new SKRect();
                    skpFooterFore.MeasureText(Source.FooterText ?? "", ref FooterSize);
                    while (FooterSize.Width > Bounds.Width)
                    {
                        skpFooterFore.TextSize = skpFooterBack.TextSize = skpFooterFore.TextSize * .9f;
                        skpFooterFore.MeasureText(Source.FooterText ?? "", ref FooterSize);
                    }
                    FooterSize = new SKRect(FooterSize.Left,FooterSize.Top,FooterSize.Right,FooterSize.Top+FooterSize.Height*sFooterLines.Length);
                    float UseX = 30f; // (Bounds.Width / 2) - (FooterSize.Width) ;
                    UseY = UseY-FooterSize.Height*1.1f;
                    DrawTextInformationSkia sktext = new DrawTextInformationSkia();
                    sktext.CharacterHandler = new DrawCharacterHandlerSkia(new JitterCharacterPositionCalculatorSkia() { Height = 10 });
                    sktext.ShadowPaint = skpFooterBack;
                    sktext.ForegroundPaint = skpFooterFore;
                    sktext.BackgroundPaint = new SKPaint() { Color = SKColors.Transparent };
                    sktext.Text = Source.FooterText ?? "";
                    sktext.ScalePercentage = 1;
                    sktext.DrawFont = useFooterFont;
                    sktext.Position = new SKPoint(UseX, UseY);

                    try
                    {

                        var Gradcolors = new SKColor[] {
        new SKColor(0, 255, 255),
        new SKColor(255, 0, 255),
        new SKColor(255, 255, 0),
        new SKColor(0, 255, 255)
    };
                        var BottomBounds = new SKRect(0, Bounds.Height-FooterSize.Height*3 , Bounds.Right, Bounds.Bottom);
                        var sweep = SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(Element.Bounds.Width, Element.Bounds.Height), Gradcolors, null, SKShaderTileMode.Repeat);
                        // create the second shader
                        var turbulence = SKShader.CreatePerlinNoiseTurbulence(0.05f, 0.05f, 4, 0);

                        // create the compose shader
                        var shader = SKShader.CreateCompose(sweep, turbulence, SKBlendMode.SrcOver);



                        GrayBG.BlendMode = SKBlendMode.Luminosity;
                        
                        GrayBG.Shader = shader;
                        g.DrawRect(BottomBounds, GrayBG);
                        var useShader = SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(0, BottomBounds.Height), new SKColor[] { SKColors.Pink, SKColors.Coral, SKColors.LightYellow, SKColors.LightGreen, SKColors.LightBlue, SKColors.PaleVioletRed}, null, SKShaderTileMode.Mirror);

                        SKPaint LinePaint = new SKPaint() { BlendMode = SKBlendMode.ColorBurn, StrokeWidth = 24, Shader = useShader };
                        g.DrawRect(BottomBounds, LinePaint);

                        pRenderTarget.DrawTextSK(sktext);
                    }
                    catch (Exception exr)
                    {
                        ;
                    }
                }


            }

            if (CursorBitmap == null)
            {
                CursorBitmap = TetrisGame.Imageman.GetSKBitmap("cursor");
                /*var CursorImage = SkiaSharp.Views.Desktop.Extensions.ToSKImage(new System.Drawing.Bitmap(TetrisGame.Imageman["cursor"]));
                
                SKImageInfo CursorInfo = new SKImageInfo(CursorImage.Width, CursorImage.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
                var skversion = new SKBitmap(CursorInfo);
                using (SKCanvas canvo = new SKCanvas(skversion))
                {
                    canvo.Clear(SKColors.Transparent);
                    canvo.DrawImage(CursorImage, new SKPoint(0, 0));
                }
                CursorBitmap = skversion;*/
            }


            g.DrawBitmap(CursorBitmap, Source.LastMouseMovement, null);

        }
        static SKPaint GrayBG = new SKPaint() { Color = SKColors.LightGreen, BlendMode = SKBlendMode.HardLight };
        SKBitmap CursorBitmap = null;
        protected SKFontInfo GetScaledHeaderFont(IStateOwner pOwner, TSourceType Source)
        {
            return MenuStateTextMenuItemSkiaRenderer.GetScaledFont(pOwner, Source.HeaderTypeSize);
            
        }
        protected SKFontInfo GetScaledFooterFont(IStateOwner pOwner, TSourceType Source)
        {
            return MenuStateTextMenuItemSkiaRenderer.GetScaledFont(pOwner, Source.FooterTypeSize);
        }
        static SKPaint Painter = null;
        static SKPaint BackPainter = null;
        public virtual float DrawHeader(IStateOwner pOwner, TSourceType Source, SKCanvas Target, SKRect Bounds)
        {
            if(Source.StateHeader=="Options")
            {
                Source.StateHeader = "Menu";
            }
            if (String.IsNullOrEmpty(Source.StateHeader)) return 0;
            SKFontInfo useHeaderFont = GetScaledHeaderFont(pOwner, Source);
            Painter = new SKPaint() { Color = SKColors.Black };
            BackPainter = new SKPaint() { Color = SKColors.White };
            BackPainter.Typeface = Painter.Typeface = useHeaderFont.TypeFace;
            BackPainter.TextSize = Painter.TextSize = (float)(useHeaderFont.FontSize * pOwner.ScaleFactor);


            SKRect HeaderSize = new SKRect();
            Painter.MeasureText(Source.StateHeader??"", ref HeaderSize);
            while(HeaderSize.Width > Bounds.Width)
            {
                BackPainter.TextSize = Painter.TextSize = BackPainter.TextSize * .9f;
                Painter.MeasureText(Source.StateHeader ?? "", ref HeaderSize);
            }
            float UseX = (Bounds.Width / 2) - (HeaderSize.Width / 2) + Source.MainXOffset;
            float UseY = HeaderSize.Height * 3f;
            DrawTextInformationSkia sktext = new DrawTextInformationSkia();
            sktext.CharacterHandler = new DrawCharacterHandlerSkia(new VerticalWavePositionCharacterPositionCalculatorSkia() {Height = HeaderSize.Height/2 });
            sktext.ShadowPaint = BackPainter;
            sktext.ForegroundPaint = Painter;
            sktext.BackgroundPaint = new SKPaint() { Color = SKColors.Transparent };
            sktext.Text = Source.StateHeader ?? "";
            sktext.ScalePercentage = 1;
            sktext.DrawFont = useHeaderFont;
            sktext.Position = new SKPoint(UseX, UseY);
            
            try
            {
                Target.DrawTextSK(sktext);
            }
            catch(Exception exr)
            {
                ;
            }
            //paint foreground.
            //Target.DrawText(Source.StateHeader ?? "", new SKPoint(UseX + 5, UseY + 5), BackPainter);
            //Target.DrawText(Source.StateHeader ?? "", new SKPoint(UseX, UseY), Painter);


            //TetrisGame.DrawTextSK(Target, Source.StateHeader,new SKPoint(UseX, UseY), useHeaderFont, SKColors.Black, SKColors.White);

            return UseY + HeaderSize.Height;
        }

        

        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, TSourceType Source, GameStateSkiaDrawParameters Element)
        {
            //throw new NotImplementedException();
        }
}
}
