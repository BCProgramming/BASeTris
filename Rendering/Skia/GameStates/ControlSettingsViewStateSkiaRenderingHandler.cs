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
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia.GameStates
{
    [RenderingHandler(typeof(ControlSettingsViewState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class ControlSettingsViewStateSkiaRenderingHandler : StandardStateRenderingHandler<SKCanvas, ControlSettingsViewState, GameStateSkiaDrawParameters>
    {
        
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

            //this is where we draw controller buttons.

            //for a quickie test we'll just display some test ones.

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


            Dictionary<GameState.GameKeys, List<int>> keybuttonlookup = new Dictionary<GameState.GameKeys, List<int>>();

            GameState.GameKeys[] retrievekeys = (GameState.GameKeys[])Enum.GetValues(typeof(GameState.GameKeys)); //  new GameState.GameKeys[] { GameState.GameKeys.GameKey_RotateCW, GameState.GameKeys.GameKey_RotateCCW, GameState.GameKeys.GameKey_Drop, GameState.GameKeys.GameKey_Hold };

            foreach (var iterate in retrievekeys)
            {
                var gotentries = pOwner.Settings.GetGamePadButtonFromGameKey(iterate);
                keybuttonlookup.Add(iterate, gotentries);
            }
            float ButtonStartX = (float)(270 * pOwner.ScaleFactor);
            float StartY = (float)(300 * pOwner.ScaleFactor);
            float StartX = (float)(250 * pOwner.ScaleFactor);
            float CurrentY = StartY;
            

            foreach (var kvp in keybuttonlookup)
            {
                if (kvp.Value == null) continue;

                String sKey = kvp.Key.ToString();
                if (sKey.StartsWith("gamekey_", StringComparison.OrdinalIgnoreCase))
                    sKey = sKey.Substring(8);


                float CurrentX = StartX;
                float MaxHeight = 25;
                foreach (var button in kvp.Value)
                {
                    //now, get the image key...
                    String getkey = AssetManager.AssetHelper.ImageKeyForXBoxControllerButton((XInput.Wrapper.X.Gamepad.GamepadButtons)button);

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
                SKRect boundrect = new SKRect();
                TextFore.MeasureText(sKey, ref boundrect);
                DrawTextInformationSkia sktext = new DrawTextInformationSkia();
                sktext.CharacterHandler = new DrawCharacterHandlerSkia(new JitterCharacterPositionCalculatorSkia() { Height = 10 });
                sktext.ShadowPaint = TextBack;
                sktext.ForegroundPaint = TextFore;
                sktext.BackgroundPaint = new SKPaint() { Color = SKColors.Transparent };
                sktext.Text = sKey ?? "";
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
