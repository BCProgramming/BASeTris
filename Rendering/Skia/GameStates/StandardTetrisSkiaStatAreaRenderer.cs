using BASeTris.GameStates;
using BASeTris.GameStates.GameHandlers;
using BASeTris.Rendering.Skia;
using BASeTris.Tetrominoes;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BASeTris.Rendering.Skia.GameStates
{
    public class StandardTetrisSkiaStatAreaRenderer : IGameCustomizationStatAreaRenderer<SKCanvas, GameplayGameState, GameStateSkiaDrawParameters, IStateOwner>
    {
        public bool AlwaysDrawDefaultTetrominoes { get; set; } = true;
        const int MAXIMUM_TETROMINO_STATUS_ROWS = 48;
        const int DEFAULT_TETRIS_TETROMINO_COUNT = 7;
        SKPaint BlackBrush = new SKPaint() { Color = SKColors.Black, Style = SKPaintStyle.StrokeAndFill };
        SKPaint WhiteBrush = new SKPaint() { Color = SKColors.White, Style = SKPaintStyle.StrokeAndFill };
        public void Render(IStateOwner pOwner, SKCanvas pRenderTarget, GameplayGameState Source, GameStateSkiaDrawParameters Element)
        {
            var g = pRenderTarget;
            SKTypeface standardFont = TetrisGame.RetroFontSK;
            Type[] useTypes = new Type[] { typeof(Tetromino_I), typeof(Tetromino_O), typeof(Tetromino_J), typeof(Tetromino_T), typeof(Tetromino_L), typeof(Tetromino_S), typeof(Tetromino_Z) };
            Object[] useTetrominoSources = (from t in useTypes select t).ToArray();
            List<TetrisStatistics.TetrisStatusRenderLine> RenderLines = new List<TetrisStatistics.TetrisStatusRenderLine>();
            var useStats = Source.GameStats;
            var Bounds = Element.Bounds;
            var Factor = Bounds.Height / 280.28d;

            if (useStats is TetrisStatistics ts)
            {
                
                RenderLines = ts.GetElementStats(AlwaysDrawDefaultTetrominoes?TetrisStatistics.ElementStatFlags.Flags_DefaultTetrominoes_Always:TetrisStatistics.ElementStatFlags.Flags_None);


                //PieceCounts = new int[] { ts.I_Piece_Count, ts.O_Piece_Count, ts.J_Piece_Count, ts.T_Piece_Count, ts.L_Piece_Count, ts.S_Piece_Count, ts.Z_Piece_Count };
            }
            else
            {
                //PieceCounts = new int[] { 0, 0, 0, 0, 0, 0, 0 };
            }
            
            var DesiredFontPixelHeight = 22d; //  PixelsToPoints((int)(Bounds.Height * (30d / 644d)));
            var SizeScale = (float)(DEFAULT_TETRIS_TETROMINO_COUNT / (float)Math.Min(Math.Max(DEFAULT_TETRIS_TETROMINO_COUNT, RenderLines.Count), MAXIMUM_TETROMINO_STATUS_ROWS)); 
            //the original design had sizing based on the main tetrominoes, however we won't shrink the scaling beyond that amount, instead, it should be "small enough" that we can instead create a second column.
            float DesiredFontSize = SizeScale*((float)(DesiredFontPixelHeight * pOwner.ScaleFactor));
            float StartYPos = Bounds.Top; // + (int)(140 * Factor);
            float useXPos = Bounds.Left;// + (int)(30 * Factor);
            double currYPos = StartYPos;
            //first we need to go through just to figure out the height we need to use for each item. Since we want the arrangement to be even.
            /*
            for (int i = 0; i < RenderLines.Count; i++)
            {
                Object currentTet = RenderLines[i].ElementSource;
                SKBitmap TetrominoImage = currentTet is Type ? Source.GetTetrominoSKBitmap((Type)currentTet) : Source.GetTetrominoSKBitmap(pOwner, (String)currentTet);

                SKSize DesiredTetrominoSize = new SKSize(TetrominoImage.Width * SizeScale, TetrominoImage.Height * SizeScale);
                PointF ImagePos = new PointF(0, 0 + (StatTextSize.Height / 2 - DesiredTetrominoSize.Height / 2));
                SKRect DrawRect = new SKRect(ImagePos.X, ImagePos.Y, ImagePos.X + DesiredTetrominoSize.Width * 1.5f, ImagePos.Y + DesiredTetrominoSize.Height * 1.5f);
            }
            */

            //ImageAttributes ShadowTet = TetrisGame.GetShadowAttributes();
            for(int i=0;i<RenderLines.Count;i++)
            //for (int i = 0; i < useTetrominoSources.Length; i++)
            {
                Object currentTet = RenderLines[i].ElementSource;
                 if (Source.GameHandler is StandardTetrisHandler)
                {

                    

                    BlackBrush.TextSize = DesiredFontSize;
                    WhiteBrush.TextSize = DesiredFontSize;
                    SKPoint BaseCoordinate = new SKPoint(useXPos, (float)currYPos);
                    

                    String StatText = "" + RenderLines[i].PieceCount;  //PieceCounts[i];
                    SKRect StatTextSize = new SKRect();
                    BlackBrush.MeasureText(StatText, ref StatTextSize);
                    SKPoint TextPos = new SKPoint(useXPos + (int)(130d * Factor*SizeScale), BaseCoordinate.Y + StatTextSize.Height);
                    //SizeF StatTextSize = g.MeasureString(StatText, standardFont);
                    SKBitmap TetrominoImage =  currentTet is Type?Source.GetTetrominoSKBitmap((Type)currentTet):Source.GetTetrominoSKBitmap(pOwner,(String)currentTet);

                    SKSize DesiredTetrominoSize = new SKSize(TetrominoImage.Width * SizeScale, TetrominoImage.Height * SizeScale);
                    PointF ImagePos = new PointF(BaseCoordinate.X, BaseCoordinate.Y + (StatTextSize.Height / 2 - DesiredTetrominoSize.Height / 2));
                    SKRect DrawRect = new SKRect(ImagePos.X, ImagePos.Y, ImagePos.X + DesiredTetrominoSize.Width * 1.5f, ImagePos.Y + DesiredTetrominoSize.Height * 1.5f);

                    g.DrawBitmap(TetrominoImage, DrawRect, null);

                    g.DrawTextSK(StatText, new SKPoint((float)(Bounds.Left + TextPos.X + 4* pOwner.ScaleFactor), (float)(Bounds.Top + TextPos.Y + 4* pOwner.ScaleFactor)), standardFont, SKColors.White, DesiredFontSize, pOwner.ScaleFactor);
                    g.DrawTextSK(StatText, TextPos, standardFont, SKColors.Black, DesiredFontSize, pOwner.ScaleFactor);

                    currYPos += 40d * Factor;// DrawRect.Height*1.1f;

                    if (currYPos > Bounds.Bottom-50f*pOwner.ScaleFactor)
                    {
                        useXPos += (float)((4 * pOwner.ScaleFactor) + StatTextSize.Width + (20*pOwner.ScaleFactor) + DrawRect.Width);
                        currYPos = StartYPos;


                    }


                }
                //g.DrawString(StatText, standardFont, Brushes.White, new PointF(TextPos.X + 4, TextPos.Y + 4));
                //g.DrawString(StatText, standardFont, Brushes.Black, TextPos);
            }
        }

        public void Render(IStateOwner pOwner, object pRenderTarget, object RenderSource, object Element)
        {
            if(pRenderTarget is SKCanvas && RenderSource is GameplayGameState && Element is GameStateSkiaDrawParameters)
            {
                Render(pOwner, pRenderTarget as SKCanvas, RenderSource as GameplayGameState, Element as GameStateSkiaDrawParameters);
            }
        }
    }
}
