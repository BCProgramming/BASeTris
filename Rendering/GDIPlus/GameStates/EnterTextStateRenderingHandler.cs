using System;
using System.Drawing;
using System.Linq;
using BASeTris.AssetManager;
using BASeTris.GameStates;

namespace BASeTris.Rendering.GDIPlus
{
    public class EnterTextStateRenderingHandler : StandardStateRenderingHandler<Graphics, EnterTextState,GameStateDrawParameters>
    {
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, EnterTextState Source, GameStateDrawParameters Element)
        {
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
            if(Source.BG!=null)
                RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source.BG, new BackgroundDrawData(Bounds));
            int StartYPosition = (int)(Bounds.Height * 0.15f);
            var MeasureBounds = g.MeasureString(Source.EntryPrompt[0], Source.useFont);
            for (int i = 0; i < Source.EntryPrompt.Length; i++)
            {
                //draw this line centered at StartYPosition+Height*i...

                int useYPosition = (int)(StartYPosition + (MeasureBounds.Height + 5) * i);
                int useXPosition = (int)(Bounds.Width / 2 - MeasureBounds.Width / 2);
                g.DrawString(Source.EntryPrompt[i], Source.useFont, Brushes.Black, new PointF(useXPosition + 5, useYPosition + 5));
                g.DrawString(Source.EntryPrompt[i], Source.useFont, new SolidBrush(useLightRain), new PointF(useXPosition, useYPosition));
            }

            float nameEntryY = StartYPosition + (MeasureBounds.Height + 5) * (Source.EntryPrompt.Length + 1);


            var AllCharacterBounds = (from c in Source.NameEntered.ToString().ToCharArray() select g.MeasureString(c.ToString(), Source.useFont)).ToArray();
            float useCharWidth = g.MeasureString("_", Source.EntryFont).Width;
            float TotalWidth;
            if (Source.EntryStyle == EnterTextState.EntryDrawStyle.EntryDrawStyle_Centered)
            {
                TotalWidth = (useCharWidth + 5) * Source.NameEntered.ToString().Trim('_', ' ').Length;

            }
            TotalWidth = (useCharWidth + 5) * Source.NameEntered.Length;
            float NameEntryX = (Bounds.Width / 2) - (TotalWidth / 2);
            if (Source.EntryStyle == EnterTextState.EntryDrawStyle.EntryDrawStyle_Preblank)
            {
                for (int charpos = 0; charpos < Source.NameEntered.Length; charpos++)
                {
                    char thischar = Source.NameEntered[charpos];
                    float useX = NameEntryX + ((useCharWidth + 5) * (charpos));
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
        }

        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, EnterTextState Source, GameStateDrawParameters Element)
        {
            //throw new NotImplementedException();
        }
    }
}