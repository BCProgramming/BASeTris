
using BASeCamp.Rendering;
using BASeTris.BackgroundDrawers;
using BASeTris.GameStates.Menu;
using BASeTris.Rendering.Adapters;
using BASeTris.Rendering.Skia.MenuItems;
using BASeTris.Theme.Block;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class MenuStateBackgroundCallbackCapsule : GamePresenterCallbackCapsule
    {
    }

    public abstract class AbstractMenuStateSkiaRenderingHandler<TSourceType> : StandardStateRenderingHandler<SKCanvas, TSourceType, GameStateSkiaDrawParameters> where TSourceType : MenuState
    {
        private static SKImage[] Corners = null;
        private static SimpleBlockTheme sbt = new SimpleBlockTheme() { AdjacentConnection = true, Christmas=true,ForceGray = true };
        
        private void LoadCorners()
        {
            Corners = new SKImage[]{
            SKImage.FromBitmap(TetrominoCollageRenderer.GetCornerDisplayBitmap_UpperLeft(sbt, 16)),
            SKImage.FromBitmap(TetrominoCollageRenderer.GetCornerDisplayBitmap_UpperRight(sbt, 16)),
            SKImage.FromBitmap(TetrominoCollageRenderer.GetCornerDisplayBitmap_LowerLeft(sbt, 16)),
            SKImage.FromBitmap(TetrominoCollageRenderer.GetCornerDisplayBitmap_LowerRight(sbt, 16))
                };
        }
        private void DrawCorners(IStateOwner pOwner, SKCanvas pRenderTarget, TSourceType Source, GameStateSkiaDrawParameters Element)
        {
            SKRect[] Targets = new SKRect[4];

            Targets[0] = new SKRect(0, 0, (float)(Corners[0].Width * pOwner.ScaleFactor),(float)( Corners[0].Height * pOwner.ScaleFactor));
            Targets[1] = new SKRect((float)(Element.Bounds.Right - Corners[1].Width * pOwner.ScaleFactor), 0, (float)(Element.Bounds.Right), (float)(Corners[1].Height * pOwner.ScaleFactor));
            
            Targets[2] = new SKRect((float)(Element.Bounds.Right - Corners[1].Width * pOwner.ScaleFactor), 
                (float)(Element.Bounds.Bottom - Corners[2].Height * pOwner.ScaleFactor)
                , (float)(Element.Bounds.Right), Element.Bounds.Bottom);


            Targets[3] = new SKRect(0, (float)(Element.Bounds.Bottom - Corners[3].Height * pOwner.ScaleFactor), (float)(Corners[3].Width * pOwner.ScaleFactor),Element.Bounds.Bottom);


            for (int i = 0; i <= 3; i++)
            {

                pRenderTarget.DrawImage(Corners[i], Targets[i]);
            }


        }
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, TSourceType Source, GameStateSkiaDrawParameters Element)
        {

            if (Corners == null)
            {
                LoadCorners();
            }
            Source.Rendered = true;

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

            DrawCorners(pOwner,pRenderTarget,Source,Element);
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
            //we don't want the menu items to be larger than the visible area. That would be annoying, ask me how I know.
            //max out the width to 80% of the bounds.
            MaxWidth = Math.Min(MaxWidth, Element.Bounds.Width*.8f);
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
                    RenderingProvider.Static.DrawElement(pOwner, g, drawitem, new MenuStateMenuItemSkiaDrawData(TargetBounds, useState,menuitemindex));
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
            //raise the MenuBackgroundDraw Capsule, if our owner is a presenter
            if (pOwner is IGamePresenter gp)
            {
                gp.AcceptCallback(new MenuStateBackgroundCallbackCapsule());
            }

            if (CursorBitmap == null)
            {
                CursorBitmap = SKImage.FromBitmap(TetrisGame.Imageman.GetSKBitmap("cursor"));
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
            if (Source.MouseInputData.MouseActive)
                g.DrawImage(CursorBitmap, Source.MouseInputData.LastMouseMovementPosition, null);


            
            
        




        }
        static SKPaint GrayBG = new SKPaint() { Color = SKColors.LightGreen, BlendMode = SKBlendMode.HardLight };
        public static SKImage CursorBitmap = null;
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
