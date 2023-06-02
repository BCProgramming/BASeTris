using BASeCamp.Rendering;
using BASeTris.AssetManager;
using BASeTris.BackgroundDrawers;
using BASeTris.GameStates;
using Microsoft.VisualBasic.ApplicationServices;
using OpenTK.Input;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia.GameStates
{
    [RenderingHandler(typeof(ControlSettingsViewState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class ControlSettingsViewStateSkiaRenderingHandler : StandardStateRenderingHandler<SKCanvas, ControlSettingsViewState, GameStateSkiaDrawParameters>
    {
       
        //private int ControllerIndex = 0;
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, ControlSettingsViewState Source, GameStateSkiaDrawParameters Element)
        {

            //background drawing. 
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

           

            var useData = AssetHelper.ControllerImageData[AssetHelper.AllControllerTypes[Source.ControllerDisplayIndex]];
            
            

            String sDiagramKey = useData.DiagramImageKey;
            SKBitmap ControllerImage = null;
            try
            {
                if(sDiagramKey!=null && TetrisGame.Imageman.HasSKBitmap(sDiagramKey))
                    ControllerImage = TetrisGame.Imageman.GetSKBitmap(sDiagramKey, 0.5f);
            }
            catch (Exception notfound)
            {
                
            }
            float usewidth = 300;

            if (ControllerImage != null)
            {
                SKSize DesiredSize = new SKSize((float)(pOwner.ScaleFactor * usewidth), (float)(usewidth * ((float)ControllerImage.Height / (float)ControllerImage.Width) * pOwner.ScaleFactor));

                SKPoint ControllerPos = new SKPoint((float)(300 * pOwner.ScaleFactor), (float)(100 * pOwner.ScaleFactor));
                //SKPoint ControllerPos = new SKPoint(Element.Bounds.Left + Element.Bounds.Width / 2 - ControllerImage.Width / 2, Element.Bounds.Top + Element.Bounds.Height / 2 - ControllerImage.Height / 2);
                var drawrect = new SKRect(ControllerPos.X, ControllerPos.Y, ControllerPos.X + DesiredSize.Width, ControllerPos.Y + DesiredSize.Height);
                pRenderTarget.DrawBitmap(ControllerImage, drawrect);
            }
            //first, we'll get all the gamepad buttons for the desired parts.
            SKPaint TextFore = new SKPaint() { Color = SKColors.Black, TextSize = (float)(24 * pOwner.ScaleFactor), Typeface = TetrisGame.RetroFontSK };
            SKPaint TextBack = new SKPaint() { Color = SKColors.White, TextSize = (float)(24 * pOwner.ScaleFactor), Typeface = TetrisGame.RetroFontSK };
            SKPaint TTextFore = new SKPaint() { Color = SKColors.Black, TextSize = (float)(48 * pOwner.ScaleFactor), Typeface = TetrisGame.RetroFontSK };
            SKPaint TTextBack = new SKPaint() { Color = SKColors.White, TextSize = (float)(48 * pOwner.ScaleFactor), Typeface = TetrisGame.RetroFontSK };
            DrawTextInformationSkia sktitle = new DrawTextInformationSkia();
            sktitle.CharacterHandler = new DrawCharacterHandlerSkia(new RotatingPositionCharacterPositionCalculatorSkia { Radius = 10});
            sktitle.ShadowPaint = TTextBack;
            sktitle.ForegroundPaint = TTextFore;
            sktitle.BackgroundPaint = new SKPaint() { Color = SKColors.Transparent };
            sktitle.Text = "Controls";
            sktitle.ScalePercentage = 1;
            sktitle.DrawFont = new Adapters.SKFontInfo(TetrisGame.RetroFontSK, (float)(48 * pOwner.ScaleFactor));
            
            SKRect titlebound = new SKRect();
            TTextFore.MeasureText(sktitle.Text, ref titlebound);

            sktitle.Position = new SKPoint(Element.Bounds.Width / 2 - titlebound.Width / 2, titlebound.Height*3) ;


            pRenderTarget.DrawTextSK(sktitle);


            Dictionary<GameState.GameKeys, (List<int>, List<int>)> keybuttonlookup = new Dictionary<GameState.GameKeys, (List<int>, List<int>)>();

            //Dictionary<GameState.GameKeys, List<int>> KeyForKeyLookup = new Dictionary<GameState.GameKeys, List<int>>();

            GameState.GameKeys[] retrievekeys = (GameState.GameKeys[])Enum.GetValues(typeof(GameState.GameKeys)); //  new GameState.GameKeys[] { GameState.GameKeys.GameKey_RotateCW, GameState.GameKeys.GameKey_RotateCCW, GameState.GameKeys.GameKey_Drop, GameState.GameKeys.GameKey_Hold };

            foreach (var iterate in retrievekeys)
            {
                var gotentries = pOwner.Settings.GetGamePadButtonFromGameKey(iterate);
                

                var gotkeyboardentries = pOwner.Settings.GetKeyBoardKeyFromGameKey(iterate);
                keybuttonlookup.Add(iterate, (gotentries,gotkeyboardentries));

            }
            float ButtonStartX = (float)(270 * pOwner.ScaleFactor);
            float StartY = (float)(350 * pOwner.ScaleFactor);
            float StartX = (float)(250 * pOwner.ScaleFactor);
            float CurrentY = StartY;
            

            foreach (var kvp in keybuttonlookup)
            {
                float MaxHeight = 25;
                if (kvp.Value.Item1 == null) continue;
                float CurrentX = StartX;
                String sKey = kvp.Key.ToString();
                
                if (sKey.StartsWith("gamekey_", StringComparison.OrdinalIgnoreCase))
                    sKey = sKey.Substring(8);
                if (kvp.Value.Item1 != null)
                {

                   


                    
                    
                    foreach (var button in kvp.Value.Item1)
                    {
                        //now, get the image key...
                        String getkey = useData.ButtonImageKeys[(XInput.Wrapper.X.Gamepad.GamepadButtons)button];

                        if (getkey != null)
                        {
                            var retrievedimage = TetrisGame.Imageman.GetSKBitmap(getkey);
                            if (retrievedimage != null)
                            {
                                pRenderTarget.DrawBitmap(retrievedimage, CurrentX, CurrentY);
                                MaxHeight = Math.Max(retrievedimage.Height, MaxHeight);
                                CurrentX += retrievedimage.Width + 10;

                            }
                        }
                    }
                }
                if (kvp.Value.Item2 != null)
                {
                    //draw keyboard keys...
                    CurrentX = (float)(pOwner.ScaleFactor * 400f);
                    foreach (var key in kvp.Value.Item2)
                    {
                        SKBitmap keybitmap = AssetHelper.GetSKBitmapForKeyboardKey((Key)key);
                        pRenderTarget.DrawBitmap(keybitmap, CurrentX, CurrentY);
                        CurrentX += keybitmap.Width;
                        MaxHeight = Math.Max(keybitmap.Height, MaxHeight);


                    }

                }
                CurrentX = (float)(pOwner.ScaleFactor * 600f);
                SKRect boundrect = new SKRect();
                TextFore.MeasureText(sKey, ref boundrect);
                DrawTextInformationSkia sktext = new DrawTextInformationSkia();
                sktext.CharacterHandler = new DrawCharacterHandlerSkia(new JitterCharacterPositionCalculatorSkia() { Height = 10 });
                sktext.ShadowPaint = TextBack;
                sktext.ForegroundPaint = TextFore;
                sktext.BackgroundPaint = new SKPaint() { Color = SKColors.Transparent };
                sktext.Text =    sKey ?? "";
                sktext.ScalePercentage = 1;
                sktext.DrawFont = new Adapters.SKFontInfo(TetrisGame.RetroFontSK, 36);
                sktext.Position = new SKPoint(CurrentX+50, CurrentY+MaxHeight/2+boundrect.Height);
                
                pRenderTarget.DrawTextSK(sktext);

                CurrentX = StartX;
                CurrentY += MaxHeight;

            }





        }

        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, ControlSettingsViewState Source, GameStateSkiaDrawParameters Element)
        {
            throw new NotImplementedException();
        }
    }
}
