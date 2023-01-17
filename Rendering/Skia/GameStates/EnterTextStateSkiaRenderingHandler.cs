using BASeCamp.Rendering;
using BASeTris.AssetManager;
using BASeTris.BackgroundDrawers;
using BASeTris.GameStates;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia.GameStates
{
    [RenderingHandler(typeof(EnterTextState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class EnterTextStateSkiaRenderingHandler : StandardStateRenderingHandler<SKCanvas, EnterTextState, GameStateSkiaDrawParameters>
    {
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, EnterTextState Source, GameStateSkiaDrawParameters Element)
        {

            if (Source.BG == null)
            {

                StandardImageBackgroundSkia sk = StandardImageBackgroundSkia.GetMenuBackgroundDrawer();
                sk.Data.Movement = new SKPoint(3, 3);
                Source.BG = sk;

            }
            var Bounds = Element.Bounds;
            var g = pRenderTarget;
            if(Source.BG!=null)
            {
                RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source.BG, new SkiaBackgroundDrawData(Bounds));
            }
           
            

            using (SKPaint TitlePaintForeground = new SKPaint() { TextSize = (float)(24 * pOwner.ScaleFactor), Color = SKColors.White, Typeface = TetrisGame.RetroFontSK })
            {
                using (SKPaint TitlePaintBackground = new SKPaint() { TextSize = (float)(24 * pOwner.ScaleFactor), Color = SKColors.Black, Typeface = TetrisGame.RetroFontSK })
                {
                    using (SKPaint EntryPaintForeground = new SKPaint() { TextSize = (float)(16 * pOwner.ScaleFactor), Color = SKColors.Black, Typeface = TetrisGame.RetroFontSK })
                    {
                        using (SKPaint EntryPaintBackground = new SKPaint() { TextSize = (float)(16 * pOwner.ScaleFactor), Color = SKColors.Black, Typeface = TetrisGame.RetroFontSK })
                        {

                            SKPaint WhitePaint = new SKPaint() { TextSize = (float)(16 * pOwner.ScaleFactor), Color = SKColors.NavajoWhite, Typeface = TetrisGame.RetroFontSK };
                            SKPaint BlackPaint = new SKPaint() { TextSize = (float)(16 * pOwner.ScaleFactor), Color = SKColors.Black, Typeface = TetrisGame.RetroFontSK };

                            float Millipercent = (float)DateTime.Now.Ticks / 5000f; //(float)DateTime.Now.Millisecond / 1000;

                            int RotateAmount = (int)(Millipercent * 240);

                            SKColor useBackgroundColor = HSLColor.RotateHue(Color.DarkBlue, RotateAmount).ToSKColor();
                            SKColor useHighlightingColor = HSLColor.RotateHue(Color.Red, RotateAmount).ToSKColor();
                            SKColor useLightRain = HSLColor.RotateHue(Color.LightPink, RotateAmount).ToSKColor();
                            SKPaint LightRainPaint = new SKPaint() { TextSize = (float)(16 * pOwner.ScaleFactor), Color = useLightRain, Typeface = TetrisGame.RetroFontSK };
                            SKPaint useHighlightingPaint = new SKPaint() { TextSize = (float)(16 * pOwner.ScaleFactor), Color = useHighlightingColor, Typeface = TetrisGame.RetroFontSK };
                            SKPaint useBackgroundPaint = new SKPaint() { TextSize = (float)(16 * pOwner.ScaleFactor), Color = useBackgroundColor, Typeface = TetrisGame.RetroFontSK };
                            int StartYPosition = (int)(Bounds.Height * .15f);

                            SKRect MeasureBounds = default;
                            TitlePaintForeground.MeasureText(Source.EntryPrompt[0], ref MeasureBounds);



                            float ShadowOffset = (float)(pOwner.ScaleFactor * 5);

                            for (int i = 0; i < Source.EntryPrompt.Length; i++)
                            {
                                //draw this line centered at StartYPosition+Height*i...

                                int useYPosition = (int)(StartYPosition + (MeasureBounds.Height + 5) * i*1.5);
                                int useXPosition = Math.Max((int)(Bounds.Width / 2 - MeasureBounds.Width / 2), (int)(Bounds.Left + 15));
                                g.DrawText(Source.EntryPrompt[i], new SKPoint(useXPosition + ShadowOffset, useYPosition + MeasureBounds.Height / 2 + ShadowOffset), TitlePaintBackground);
                                g.DrawText(Source.EntryPrompt[i], new SKPoint(useXPosition + ShadowOffset, useYPosition + MeasureBounds.Height / 2 + ShadowOffset), TitlePaintForeground);
                                //g.DrawString(Source.EntryPrompt[i], Source.useFont, Brushes.Black, new PointF(useXPosition + 5, useYPosition + 5));
                                //g.DrawString(Source.EntryPrompt[i], Source.useFont, new SolidBrush(useLightRain), new PointF(useXPosition, useYPosition));
                            }

                            float NameEntryY = StartYPosition + (MeasureBounds.Height + 5) * (Source.EntryPrompt.Length + 1);

                            var AllCharacterBounds = Source.NameEntered.ToString().ToCharArray().Select((u) =>
                            {
                                SKRect MeasuredResult = default;
                                EntryPaintForeground.MeasureText(u.ToString(), ref MeasuredResult);
                                return MeasuredResult;
                            }).ToArray();


                            float nameEntryY = StartYPosition + (MeasureBounds.Height + 5) * (Source.EntryPrompt.Length + 1);


                            SKRect charMeasure = default;
                            EntryPaintForeground.MeasureText("#", ref charMeasure);

                            float useCharWidth = charMeasure.Width;
                            float useCharHeight = charMeasure.Height;
                            float TotalWidth;
                            if (Source.EntryStyle == EnterTextState.EntryDrawStyle.EntryDrawStyle_Centered)
                            {
                                TotalWidth = (useCharWidth + 5) * Source.NameEntered.ToString().Trim('_', ' ').Length;
                            }
                            else
                            {
                                TotalWidth = (useCharWidth + 5) * Source.NameEntered.ToString().Trim('_', ' ').Length;
                            }


                            TotalWidth = (useCharWidth + 5) * Source.NameEntered.Length;
                            float NameEntryX = (Bounds.Width / 2) - (TotalWidth / 2);
                            NameEntryX = Math.Max(NameEntryX, Bounds.Left + 10);
                            float LineStart = NameEntryX;
                            if (Source.EntryStyle == EnterTextState.EntryDrawStyle.EntryDrawStyle_Preblank)
                            {
                                for (int charpos = 0; charpos < Source.NameEntered.Length; charpos++)
                                {
                                    char thischar = Source.NameEntered[charpos];
                                    float useX = NameEntryX + ((useCharWidth + 3) * (charpos));
                                    if (useX + useCharWidth > Bounds.Width)
                                    {
                                        useX = LineStart;
                                        nameEntryY += useCharHeight + 3;
                                    }
                                    var DisplayPaint = (Source.CurrentPosition == charpos) ? useHighlightingPaint : WhitePaint;
                                    var ShadowPaint = (Source.CurrentPosition == charpos) ? LightRainPaint : BlackPaint;
                                    //Brush DisplayBrush = (Source.CurrentPosition == charpos) ? new SolidBrush(UseHighLightingColor) : Brushes.NavajoWhite;
                                    //Brush ShadowBrush = (Source.CurrentPosition == charpos) ? new SolidBrush(useLightRain) : Brushes.Black;
                                    g.DrawText(thischar.ToString(), new SKPoint((float)(useX + (2 * pOwner.ScaleFactor)), (float)(nameEntryY + (2 * pOwner.ScaleFactor))), ShadowPaint);
                                    g.DrawText(thischar.ToString(), new SKPoint(useX, nameEntryY), DisplayPaint);
                                }
                            }
                            else if (Source.EntryStyle == EnterTextState.EntryDrawStyle.EntryDrawStyle_Centered)
                            {
                                //"simpler"- we just draw the trimmed text.
                                //not implemented yet....
                                String TrimEntered = Source.NameEntered.ToString().Trim(' ', '_');



                            }
                        }
                    }
                }
            }
            /*
             float useCharWidth = charMeasure.Width;
            float useCharHeight = charMeasure.Height;
            float TotalWidth;
            if (Source.EntryStyle == EnterTextState.EntryDrawStyle.EntryDrawStyle_Centered)
            {
                TotalWidth = (useCharWidth + 5) * Source.NameEntered.ToString().Trim('_', ' ').Length;

            }
            TotalWidth = (useCharWidth + 5) * Source.NameEntered.Length;
            float NameEntryX = (Bounds.Width / 2) - (TotalWidth / 2);
            NameEntryX = Math.Max(NameEntryX, Bounds.Left + 10);
            float LineStart = NameEntryX;
            if (Source.EntryStyle == EnterTextState.EntryDrawStyle.EntryDrawStyle_Preblank)
            {
                for (int charpos = 0; charpos < Source.NameEntered.Length; charpos++)
                {
                    char thischar = Source.NameEntered[charpos];
                    float useX = NameEntryX + ((useCharWidth + 3) * (charpos));
                    if (useX + useCharWidth > Bounds.Width)
                    {
                        useX = LineStart;
                        nameEntryY += useCharHeight + 3;
                    }
                    Brush DisplayBrush = (Source.CurrentPosition == charpos) ? new SolidBrush(UseHighLightingColor) : Brushes.NavajoWhite;
                    Brush ShadowBrush = (Source.CurrentPosition == charpos) ? new SolidBrush(useLightRain) : Brushes.Black;
                    g.DrawString(thischar.ToString(), Source.EntryFont, ShadowBrush, new PointF(useX + 2, nameEntryY + 2));
                    g.DrawString(thischar.ToString(), Source.EntryFont, DisplayBrush, new PointF(useX, nameEntryY));
                }
            }
            else if (Source.EntryStyle == EnterTextState.EntryDrawStyle.EntryDrawStyle_Centered)
            {
                //"simpler"- we just draw the trimmed text.
                String TrimEntered = Source.NameEntered.ToString().Trim(' ', '_');



            }
             */






            /*
            var Bounds = Element.Bounds;
            var g = pRenderTarget;
            if (Source.useFont == null) Source.useFont = TetrisGame.GetRetroFont(15, pOwner.ScaleFactor);
            if (Source.EntryFont == null) Source.EntryFont = TetrisGame.GetRetroFont(15, pOwner.ScaleFactor);

            float Millipercent = (float)DateTime.Now.Ticks / 5000f; //(float)DateTime.Now.Millisecond / 1000;

            int RotateAmount = (int)(Millipercent * 240);

            Color UseBackgroundColor = HSLColor.RotateHue(Color.DarkBlue, RotateAmount);
            Color UseHighLightingColor = HSLColor.RotateHue(Color.Red, RotateAmount);
            Color useLightRain = HSLColor.RotateHue(Color.LightPink, RotateAmount);
            //throw new NotImplementedException();
            if (Source.BG != null)
                RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source.BG, new GDIBackgroundDrawData(Bounds));
            int StartYPosition = (int)(Bounds.Height * 0.15f);
            var MeasureBounds = g.MeasureString(Source.EntryPrompt[0], Source.useFont);
            for (int i = 0; i < Source.EntryPrompt.Length; i++)
            {
                //draw this line centered at StartYPosition+Height*i...

                int useYPosition = (int)(StartYPosition + (MeasureBounds.Height + 5) * i);
                int useXPosition = Math.Max((int)(Bounds.Width / 2 - MeasureBounds.Width / 2), (int)(Bounds.Left + 15));
                g.DrawString(Source.EntryPrompt[i], Source.useFont, Brushes.Black, new PointF(useXPosition + 5, useYPosition + 5));
                g.DrawString(Source.EntryPrompt[i], Source.useFont, new SolidBrush(useLightRain), new PointF(useXPosition, useYPosition));
            }

            float nameEntryY = StartYPosition + (MeasureBounds.Height + 5) * (Source.EntryPrompt.Length + 1);


            var AllCharacterBounds = (from c in Source.NameEntered.ToString().ToCharArray() select g.MeasureString(c.ToString(), Source.useFont)).ToArray();
            var charMeasure = g.MeasureString("#", Source.EntryFont);
            float useCharWidth = charMeasure.Width;
            float useCharHeight = charMeasure.Height;
            float TotalWidth;
            if (Source.EntryStyle == EnterTextState.EntryDrawStyle.EntryDrawStyle_Centered)
            {
                TotalWidth = (useCharWidth + 5) * Source.NameEntered.ToString().Trim('_', ' ').Length;

            }
            TotalWidth = (useCharWidth + 5) * Source.NameEntered.Length;
            float NameEntryX = (Bounds.Width / 2) - (TotalWidth / 2);
            NameEntryX = Math.Max(NameEntryX, Bounds.Left + 10);
            float LineStart = NameEntryX;
            if (Source.EntryStyle == EnterTextState.EntryDrawStyle.EntryDrawStyle_Preblank)
            {
                for (int charpos = 0; charpos < Source.NameEntered.Length; charpos++)
                {
                    char thischar = Source.NameEntered[charpos];
                    float useX = NameEntryX + ((useCharWidth + 3) * (charpos));
                    if (useX + useCharWidth > Bounds.Width)
                    {
                        useX = LineStart;
                        nameEntryY += useCharHeight + 3;
                    }
                    Brush DisplayBrush = (Source.CurrentPosition == charpos) ? new SolidBrush(UseHighLightingColor) : Brushes.NavajoWhite;
                    Brush ShadowBrush = (Source.CurrentPosition == charpos) ? new SolidBrush(useLightRain) : Brushes.Black;
                    g.DrawString(thischar.ToString(), Source.EntryFont, ShadowBrush, new PointF(useX + 2, nameEntryY + 2));
                    g.DrawString(thischar.ToString(), Source.EntryFont, DisplayBrush, new PointF(useX, nameEntryY));
                }
            }
            else if (Source.EntryStyle == EnterTextState.EntryDrawStyle.EntryDrawStyle_Centered)
            {
                //"simpler"- we just draw the trimmed text.
                String TrimEntered = Source.NameEntered.ToString().Trim(' ', '_');



            }
            */
        }

        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, EnterTextState Source, GameStateSkiaDrawParameters Element)
        {
            //throw new NotImplementedException();
        }
    }
}
