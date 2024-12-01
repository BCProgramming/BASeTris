using BASeCamp.Rendering;
using BASeTris.AssetManager;
using BASeTris.BackgroundDrawers;
using BASeTris.GameStates;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia.GameStates
{
    [RenderingHandler(typeof(DesignBackgroundState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class DesignBackgroundStateSkiaRenderingHandler : StandardStateRenderingHandler<SKCanvas, DesignBackgroundState, GameStateSkiaDrawParameters>
    {

        Dictionary<GameState.GameKeys, GameKeyBindInfo<SKImage>> GameKeyBitmapData = null;
        private void PrepareControllerButtonImages(IStateOwner pOwner)
        {
            if (GameKeyBitmapData != null) return;
            GameKeyBitmapData = new Dictionary<GameState.GameKeys, GameKeyBindInfo<SKImage>>();
            //AssetHelper.XBoxSeriesXImageKeyData
            foreach (var result in AssetHelper.RetrieveBindingImages(pOwner.Settings, AssetHelper.XBoxSeriesXImageKeyData))
            {
                GameKeyBitmapData.Add(result.Key, result);
            }
            SKPaint TextFore = new SKPaint() { Color = SKColors.Black, TextSize = (float)(12 * pOwner.ScaleFactor), Typeface = TetrisGame.RetroFontSK };
            SKPaint TextBack = new SKPaint() { Color = SKColors.White, TextSize = (float)(12 * pOwner.ScaleFactor), Typeface = TetrisGame.RetroFontSK };
            SKPaint TTextFore = new SKPaint() { Color = SKColors.Black, TextSize = (float)(12 * pOwner.ScaleFactor), Typeface = TetrisGame.RetroFontSK };
            SKPaint TTextBack = new SKPaint() { Color = SKColors.White, TextSize = (float)(12 * pOwner.ScaleFactor), Typeface = TetrisGame.RetroFontSK };
            skkey = new DrawTextInformationSkia();
            skkey.CharacterHandler = new DrawCharacterHandlerSkia(new JitterCharacterPositionCalculatorSkia() { Height=2f*(float)pOwner.ScaleFactor});
            skkey.ShadowPaint = TTextBack;
            skkey.ForegroundPaint = TTextFore;
            skkey.BackgroundPaint = new SKPaint() { Color = SKColors.Transparent };

            skkey.ScalePercentage = 1;
            skkey.DrawFont = new Adapters.SKFontInfo(TetrisGame.RetroFontSK, (float)(12 * pOwner.ScaleFactor));
            
        }
        private DrawTextInformationSkia skkey = null;
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, DesignBackgroundState Source, GameStateSkiaDrawParameters Element)
        {
            //throw new NotImplementedException();

            //stuff we want to render:

            //background:
            PrepareControllerButtonImages(pOwner); //prep the button images. We want to have "help" displayed, to indicate the keys.
                                                   //Since I keep forgetting myself and there's literally no documentation this would seem prudent....
            
            //we are responsible for filling in the Source's BoxBound as well as PositionalMapping.
            
            if (Source.BG == null)
            {

                StandardImageBackgroundSkia sk = StandardImageBackgroundSkia.GetMenuBackgroundDrawer();
                sk.Data.Movement = new SKPoint(3, 3);
                Source.BG = sk;

            }
            var Bounds = Element.Bounds;
            var g = pRenderTarget;
            if (Source.BG != null)
            {
                RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source.BG, new SkiaBackgroundDrawData(Bounds));
            }

            //Now, we <could> build a collage, and render that. But, we want to display the selected nomino more prominently, so we should instead render all the Nominoes directly, so we can render the selected one differently (with an Alpha SKPaint that pulses, I would argue).
            //however we will probably have a way to create a collage for the purpose of testing the background.
            //TetrisBlockDrawSkiaParameters tbd = new TetrisBlockDrawSkiaParameters(g, new SKRect(DrawBlockX, DrawBlockY, DrawBlockX + BlockSize.Width, DrawBlockY + BlockSize.Height), null,   pOwner.Settings);

            //desiredsize of the edit area is a third of the screen in the middle. we'll scale to fit that size. (Note, however, that elements can render outside that area... for the moment...
            for (int layerindex = 0; layerindex < Source.Layers.Length; layerindex++)
            {
                if (layerindex > Source.LayerIndex) continue; //don't paint layers above the current layer.
                var Layer = Source.Layers[layerindex];
                float AbsoluteWidth = Bounds.Width / 2;
                

                //SKRect EditArea = new SKRect(Bounds.Width / 4, Bounds.Height / 4, Bounds.Width/2-AbsoluteWidth/2, Bounds.Height - Bounds.Height / 4);

                float Ratio = (float)Layer.DesignRows / (float)Layer.DesignColumns;
                var AbsoluteHeight = ((float)AbsoluteWidth * Ratio);
                if (AbsoluteHeight > Bounds.Height)
                {
                    AbsoluteHeight = Bounds.Height;
                    AbsoluteWidth = Bounds.Height / Ratio;
                }
                var TopValue = Bounds.Height / 2 - AbsoluteHeight / 2;
                var LeftValue = Bounds.Width / 2 - AbsoluteWidth / 2;
                var EditArea = new SKRect(LeftValue, TopValue, LeftValue + AbsoluteWidth, TopValue +AbsoluteHeight);
                SKSize NominoBlockSize = new SKSize((float)(EditArea.Width / (float)(Layer.DesignColumns)),  (float)(EditArea.Height / (float)(Layer.DesignRows)));
                var duplicated = Layer.DesignNominoes;
                var useSelected = Layer.SelectedIndex;


                for (int xgrid = 0; xgrid < Layer.DesignColumns; xgrid++)
                {
                    for (int ygrid = 0; ygrid < Layer.DesignRows; ygrid++)
                    {
                        SKPoint Location = new SKPoint(
                            (float)(EditArea.Left + (double)(NominoBlockSize.Width) * (double)(xgrid)),
                            (float)(EditArea.Top + (double)NominoBlockSize.Height * (double)(ygrid)

                            ));
                        SKRect BlockBound = new SKRect(Location.X, Location.Y, (float)(Location.X + NominoBlockSize.Width), (float)(Location.Y + NominoBlockSize.Height));
                        pRenderTarget.DrawRect(BlockBound, new SKPaint() { Color = Layer.GridColor, StrokeWidth = 1, Style = SKPaintStyle.Stroke });

                    }


                }

                //pRenderTarget.DrawRect(EditArea, new SKPaint() { Color = SKColors.Yellow, StrokeWidth = 1,Style=SKPaintStyle.Stroke });
                SKPaint RenderPaintBlock = new SKPaint();

                for (int index = 0; index < duplicated.Count; index++)
                {
                    if (Source.SelectedIndex == index)
                    {
                        RenderPaintBlock.Color = new SKColor(255, 255, 255, (byte)(100 + (78 * (Math.Sin((float)TetrisGame.GetTickCount() / 500f)))));
                    }
                    var CurrentElement = duplicated[index];

                    
                    foreach (var renderblock in CurrentElement.GetBlockData())
                    {
                        if (renderblock.Y + CurrentElement.Y == 0)
                        {
                            ;
                        }
                        SKPoint Location = new SKPoint(EditArea.Left + (float)(NominoBlockSize.Width) * (float)(renderblock.X + CurrentElement.X), EditArea.Top + (float)NominoBlockSize.Height * (float)(renderblock.Y + CurrentElement.Y));
                        SKRect BlockBound = new SKRect(Location.X, Location.Y, (float)(Location.X + NominoBlockSize.Width), (float)(Location.Y + NominoBlockSize.Height));
                        using (SKAutoCanvasRestore restore = new SKAutoCanvasRestore(pRenderTarget))
                        {
                            pRenderTarget.ClipRect(BlockBound);
                            RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, renderblock.Block, new TetrisBlockDrawSkiaParameters(pRenderTarget, BlockBound, CurrentElement, pOwner.Settings));
                            if (Layer.SelectedIndex == index)
                            {
                                pRenderTarget.DrawRect(BlockBound, RenderPaintBlock);
                            }
                        }

                    }


                }
            }
            //we want to render the controls along the bottom, too. Since we can map controls to bitmaps, may as well right?
            //also it would look sort a professional? Maybe.

            var NextNominoButtonImages = GameKeyBitmapData[GameState.GameKeys.GameKey_DesignerNextNomino].PadButtonElements;

            GameState.GameKeys[] DrawHelpKeys = new GameState.GameKeys[]{
                GameState.GameKeys.GameKey_DesignerNextNomino,
                GameState.GameKeys.GameKey_DesignerPrevNomino,
                GameState.GameKeys.GameKey_DesignerChangeNomino,
                GameState.GameKeys.Gamekey_DesignerDeleteNomino,

                };
            float ButtonSize = 25f * (float)pOwner.ScaleFactor;
            bool HorizontalStack = true;
            float StartX, StartY;
            if (HorizontalStack)
            {
                StartX = Bounds.Left + 50f * (float)pOwner.ScaleFactor;
                StartY = Bounds.Bottom - (ButtonSize*3); //* (float)pOwner.ScaleFactor;
            }
            else
            {
                StartX = Bounds.Left + 50f * (float)pOwner.ScaleFactor;
                StartY = Bounds.Top + 120f * (float)pOwner.ScaleFactor;
            }
            float CurrentX = StartX;
            float CurrentY = StartY;
            
            foreach (GameState.GameKeys drawkey in DrawHelpKeys)
            {
                bool RenderedButtonImage = false;

                var getPadImageData = GameKeyBitmapData.ContainsKey(drawkey)?GameKeyBitmapData[drawkey].PadButtonElements:Array.Empty<SKImage>();
                var getKeyImageData = GameKeyBitmapData.ContainsKey(drawkey)?GameKeyBitmapData[drawkey].KeyboardElements: Array.Empty<SKImage>();
                foreach (var DrawButton in getPadImageData.Concat(getKeyImageData))
                {
                    RenderedButtonImage = true;
                    pRenderTarget.DrawImage(DrawButton, new SKRect(CurrentX, CurrentY, CurrentX + ButtonSize, CurrentY + ButtonSize));
                    CurrentX += ButtonSize * 1.1f;
                    
                }
                

                //SKRect titlebound = new SKRect();
                //TTextFore.MeasureText(skkey.Text, ref titlebound);
                skkey.Text = GameState.GetGameKeyFriendlyName(drawkey).Replace("(BG Design)","");
                skkey.Position = new SKPoint(CurrentX, CurrentY + ButtonSize);
                SKRect TextBound = new SKRect();
                skkey.ForegroundPaint.MeasureText(skkey.Text, ref TextBound);

                if(RenderedButtonImage)
                    pRenderTarget.DrawTextSK(skkey);

                if (HorizontalStack)
                {
                    CurrentX += ButtonSize * 1.1f + TextBound.Width;
                    if (CurrentX > Bounds.Right) CurrentX = StartX;
                }
                else
                {
                    CurrentY += ButtonSize * 1.1f;
                    CurrentX = StartX;
                }

                
                
            }



        }

        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, DesignBackgroundState Source, GameStateSkiaDrawParameters Element)
        {
            //throw new NotImplementedException();
        }
    }
}
