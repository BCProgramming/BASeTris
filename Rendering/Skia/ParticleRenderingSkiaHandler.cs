﻿using BASeCamp.Rendering;
using BASeTris.GameStates;
using BASeTris.Particles;
using BASeTris.Rendering.Adapters;
using BASeTris.Rendering.GDIPlus;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia
{
    [RenderingHandler(typeof(BitmapParticle), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class BitmapParticleRenderingSkiaHandler : BaseParticleRenderingSkiaHandler
    {
        public void Render(IStateOwner pOwner, SKCanvas pRenderTarget, BitmapParticle Source, GameStateSkiaDrawParameters Element)
        {
            var PosX = Source.Position.X - Source.Width / 2;
            var PosY = Source.Position.Y - Source.Height / 2;
            SKMatrix cloned = SKMatrix.Identity;
            if (Source.Angle != 0)
            {
                cloned = pRenderTarget.TotalMatrix;
                pRenderTarget.RotateDegrees((float)Source.Angle);
            }
            
            pRenderTarget.DrawImage(Source.Image, new SKRect(PosX, PosY, PosX + Source.Width, PosY + Source.Height));
            if (Source.Angle != 0)
            {
                pRenderTarget.SetMatrix(cloned);
            }
        }
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, BaseParticle Source, GameStateSkiaDrawParameters Element)
        {
            Render(pOwner, pRenderTarget, (CharParticle)Source, Element);
        }
    }

    [RenderingHandler(typeof(CharParticle),typeof(SKCanvas),typeof(GameStateSkiaDrawParameters))]
    public class CharParticleRenderingSkiaHandler :BaseParticleRenderingSkiaHandler
    {
        SKPaint skp = new SKPaint() { Color = SKColors.White, TextSize = 18 };
        public void Render(IStateOwner pOwner,SKCanvas pRenderTarget,CharParticle Source,GameStateSkiaDrawParameters Element)
        {
            var Alphause = TranslateAlpha(Source);
            var CharPoint = TranslatePosition(pOwner, pRenderTarget, Source.Position, Element);
            var FontSizeTranslate = new BCPoint(1, Source.FontInfo.FontSize);
            var TranslatedFontSize = TranslatePosition(pOwner, pRenderTarget, FontSizeTranslate, Element);
            var useColor = new SKColor(Source.Color.R, Source.Color.G, Source.Color.B, Alphause);

            SKRect Bound = new SKRect();
            skp.MeasureText(Source.Text, ref Bound);
            skp.TextSize = TranslatedFontSize.Y;
            SKPaint Foreground = new SKPaint() { Color = useColor,TextSize = TranslatedFontSize.Y,Typeface = TetrisGame.RetroFontSK };
            SKPaint Background = new SKPaint() { Color = new SKColor(0,0,0,Alphause), TextSize = TranslatedFontSize.Y, Typeface = TetrisGame.RetroFontSK };
            DrawTextInformationSkia skinfo = new DrawTextInformationSkia()
            {
                Text = Source.Text,
                Position = CharPoint,
                
                ForegroundPaint = Foreground,
                ShadowPaint = Background
                
            };
            switch (Source.Flags)
            {
                case CharParticle.SpecialCharacterParticleFlags.Effect_Wave:
                    skinfo.CharacterHandler = new DrawCharacterHandlerSkia(new VerticalWavePositionCharacterPositionCalculatorSkia() { Height = (float)(pOwner.ScaleFactor * 6) });
                    break;
                case CharParticle.SpecialCharacterParticleFlags.Effect_Swirl:
                    skinfo.CharacterHandler = new DrawCharacterHandlerSkia(new RotatingPositionCharacterPositionCalculatorSkia  {   Radius= (float)(pOwner.ScaleFactor * 6) });
                    break;
                case CharParticle.SpecialCharacterParticleFlags.Effect_Jitter:
                    skinfo.CharacterHandler = new DrawCharacterHandlerSkia(new JitterCharacterPositionCalculatorSkia { Height = (float)(pOwner.ScaleFactor * 6) });
                    break;
            }
            skinfo.DrawFont = new SKFontInfo(TetrisGame.RetroFontSK, TranslatedFontSize.Y);
            //pRenderTarget.DrawText(Source.Text, CharPoint.X,CharPoint.Y,TetrisGame.RetroFontSK,  skp);
            //pRenderTarget.DrawTextSK(Source.Text, CharPoint, TetrisGame.RetroFontSK, useColor, skp.TextSize,1);
            SKMatrix cloned = SKMatrix.Identity;
            if (Source.Angle != 0)
            {
                cloned = pRenderTarget.TotalMatrix;
                pRenderTarget.RotateDegrees((float)Source.Angle);
            }

            pRenderTarget.DrawTextSK(skinfo);

            if (Source.Angle != 0)
            {
                pRenderTarget.SetMatrix(cloned);
            }
            //CharPoint -= new BCPoint(skp)
            //    skp.MeasureText(Source.Character);
        }
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, BaseParticle Source, GameStateSkiaDrawParameters Element)
        {
            Render(pOwner, pRenderTarget, (CharParticle)Source, Element);
        }
    }

    [RenderingHandler(typeof(ShapeParticle), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class ShapeParticleRenderingSkiaHandler : BaseParticleRenderingSkiaHandler
    {
        Dictionary<SKColor, SKPaint> PaintCache = new Dictionary<SKColor, SKPaint>();
        public void Render(IStateOwner pOwner, SKCanvas pRenderTarget, ShapeParticle Source, GameStateSkiaDrawParameters Element)
        {
            var Position = TranslatePosition(pOwner, pRenderTarget, Source.Position, Element);

            var Alphause = TranslateAlpha(Source);
            var grabColor = new BCColor(Source.Color.R,Source.Color.G,Source.Color.B,Alphause);

            SKPaint skp = null;
            if (!PaintCache.TryGetValue(grabColor, out skp))
            {
                var buildskp = new SKPaint() { Color = new SKColor(grabColor.R, grabColor.G, grabColor.B, Alphause), StrokeWidth = 2, Style = SKPaintStyle.StrokeAndFill };
                PaintCache.Add(grabColor, buildskp);
            }
                //pRenderTarget.DrawLine(SKPoint.Empty, Position, skp);
                pRenderTarget.DrawRect(new SKRect(Position.X - Source.Size, Position.Y - Source.Size, Position.X + Source.Size, Position.Y + Source.Size), PaintCache[grabColor]);
            
        }

        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, BaseParticle Source, GameStateSkiaDrawParameters Element)
        {
            Render(pOwner, pRenderTarget, (ShapeParticle)Source, Element);
        }

    }

    [RenderingHandler(typeof(LineParticle), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class LineParticleRenderingSkiaHandler : BaseParticleRenderingSkiaHandler //StandardRenderingHandler<SKCanvas, LineParticle, GameStateSkiaDrawParameters>
    {
        ConditionalWeakTable<LineParticle, SKPaint> PaintCache = new ConditionalWeakTable<LineParticle, SKPaint>();
        public void Render(IStateOwner pOwner, SKCanvas pRenderTarget, LineParticle Source, GameStateSkiaDrawParameters Element)
        {
            var PointA = TranslatePosition(pOwner, pRenderTarget, Source.Position, Element);
            var PointB = TranslatePosition(pOwner, pRenderTarget, Source.EndPoint, Element);
            var Alphause = TranslateAlpha(Source);
            SKPaint skp = null;
            if (!PaintCache.TryGetValue(Source, out skp))
            {
                var grabColor = Source.Color;
                skp = new SKPaint() { Color = new SKColor(grabColor.R, grabColor.G, grabColor.B, Alphause), StrokeWidth = 2 };
                PaintCache.Add(Source, skp);
            }
            pRenderTarget.DrawLine(PointA, PointB, skp);
            
        }
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, BaseParticle Source, GameStateSkiaDrawParameters Element)
        {
            Render(pOwner, pRenderTarget, (LineParticle)Source, Element);
        }

    }

    [RenderingHandler(typeof(BaseParticle), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class BaseParticleRenderingSkiaHandler : StandardRenderingHandler<SKCanvas, BaseParticle, GameStateSkiaDrawParameters>
    {
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, BaseParticle Source, GameStateSkiaDrawParameters Element)
        {
            
            BCPoint usePosition = new BCPoint(Source.Position.X, Source.Position.Y);
            //have to try to get the standardRenderingProvider. If we can then we will use the coordinates as if they are a block position- otherwise, we use the coordinates directly.
            usePosition = TranslatePosition(pOwner, pRenderTarget, Source.Position, Element);

            BCPoint PrevPosition = TranslatePosition(pOwner, pRenderTarget, Source.Position - Source.Velocity, Element);

            byte useAlpha =  TranslateAlpha(Source);
            
            if (SharePaint == null) SharePaint = new SKPaint() { IsAntialias =false, Color = new SKColor(Source.Color.R, Source.Color.G, Source.Color.B, useAlpha), StrokeWidth = 1.2f };
            else SharePaint.Color = new SKColor(Source.Color.R, Source.Color.G, Source.Color.B, useAlpha);
            if (PrevPosition != usePosition)
                pRenderTarget.DrawLine(PrevPosition, usePosition, SharePaint);
            else
            {
                pRenderTarget.DrawCircle(usePosition, 3, SharePaint);
            }
                //pRenderTarget.DrawRect(new SKRect(usePosition.X, usePosition.Y, usePosition.X + 2, usePosition.Y + 2), skp);
            
        }
        private SKPaint SharePaint = null;
        protected byte TranslateAlpha(BaseParticle Source)
        {
            byte useAlpha = 255;
            var PercentAlpha = 1 - ((float)Source.Age / (float)Source.TTL);
            //clamp
            PercentAlpha = PercentAlpha > 1 ? 1 : PercentAlpha < 0 ? 0 : PercentAlpha;
            useAlpha = (byte)(PercentAlpha * 255);
            return useAlpha;
        }
        
        public static BCPoint TranslatePosition(IStateOwner pOwner, SKCanvas pRenderTarget, BCPoint Position, GameStateSkiaDrawParameters Element)
        {
            BCPoint Result = Position;
            GameplayGameState foundstandard = null;
            var CurrState = pOwner.CurrentState;
            if (CurrState is GameplayGameState standard)
            {
                foundstandard = standard;
            }
            else if (CurrState is ICompositeState<GameplayGameState> composite)
            {
                foundstandard = composite.GetComposite();
            }

            if (foundstandard != null)
            {
                Result.X = foundstandard.PlayField.GetBlockWidth(Element.Bounds) * (Position.X+Element.Offset.X);
                Result.Y = foundstandard.PlayField.GetBlockHeight(Element.Bounds) * (Position.Y+Element.Offset.Y);
            }
            return Result;
        }
    }


    public abstract class ListRenderingHandlerBase<T> : StandardRenderingHandler<SKCanvas, List<T>, GameStateSkiaDrawParameters>
    {
        static Dictionary<Type, BASeCamp.Rendering.Interfaces.IRenderingHandler<IStateOwner>> CacheRenderProviders = new Dictionary<Type, BASeCamp.Rendering.Interfaces.IRenderingHandler<IStateOwner>>();
        protected BASeCamp.Rendering.Interfaces.IRenderingHandler<IStateOwner> GetProvider(Type forType)
        {
            if (!CacheRenderProviders.ContainsKey(forType))
            {
                var getrenderer = RenderingProvider.Static.GetHandler(typeof(SKCanvas), forType, typeof(GameStateSkiaDrawParameters));
                if (getrenderer != null)
                    CacheRenderProviders.Add(forType, getrenderer);

            }
            return CacheRenderProviders[forType];
        }
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, List<T> Source, GameStateSkiaDrawParameters Element)
        {
            if (Source.Count > 0)
            {
            }
            //var translated = BaseParticleRenderingSkiaHandler.TranslatePosition(pOwner, pRenderTarget, Element.Offset, Element);
            foreach (var iterate in Source)
            {
                if (iterate == null) continue;
                //retrieve rendering handler.
                var Grabrenderer = GetProvider(iterate.GetType());
                Grabrenderer.Render(pOwner, pRenderTarget, iterate, Element);
            }
            if (pOwner.CurrentState.GameProcSuspended)
            {
                ;
            }
        }
    }


    //[RenderingHandler(typeof(TetrisBlock), typeof(Graphics), typeof(TetrisBlockDrawParameters))]
    //public class TetrisBlockGDIRenderingHandler : StandardRenderingHandler<Graphics, TetrisBlock, TetrisBlockDrawParameters>
    [RenderingHandler(typeof(List<BaseParticle>), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class ParticleRenderingSkiaHandler :   ListRenderingHandlerBase<BaseParticle>   //StandardRenderingHandler<SKCanvas , List<BaseParticle>, GameStateSkiaDrawParameters>
    {
        public void UpdateParticles(IStateOwner pOwner,List<BaseParticle> Source)
        {
            List<BaseParticle> RemoveParticles = new List<BaseParticle>();
            lock (Source)
            {
                foreach (var iterate in Source)
                {
                    if (iterate.GameProc(pOwner))
                    {
                        RemoveParticles.Add(iterate);
                    }
                }
                foreach (var itrem in RemoveParticles)
                {
                    Source.Remove(itrem);
                }

            }
        }
       
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, List<BaseParticle> Source, GameStateSkiaDrawParameters Element)
        {
            base.Render(pOwner, pRenderTarget, Source, Element);
            
            UpdateParticles(pOwner,Source);
            
        }
    }
}
