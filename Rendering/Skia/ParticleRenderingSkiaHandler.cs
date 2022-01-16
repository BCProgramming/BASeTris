using BASeCamp.Rendering;
using BASeTris.GameObjects;
using BASeTris.GameStates;
using BASeTris.Rendering.Adapters;
using BASeTris.Rendering.GDIPlus;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                CharacterHandler = new DrawCharacterHandlerSkia(new VerticalWavePositionCharacterPositionCalculatorSkia() { Height = (float)(pOwner.ScaleFactor * 6) }),
                ForegroundPaint = Foreground,
                ShadowPaint = Background
                
            };
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

    [RenderingHandler(typeof(LineParticle), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class LineParticleRenderingSkiaHandler : BaseParticleRenderingSkiaHandler //StandardRenderingHandler<SKCanvas, LineParticle, GameStateSkiaDrawParameters>
    {
        public void Render(IStateOwner pOwner, SKCanvas pRenderTarget, LineParticle Source, GameStateSkiaDrawParameters Element)
        {
            var PointA = TranslatePosition(pOwner, pRenderTarget, Source.Position, Element);
            var PointB = TranslatePosition(pOwner, pRenderTarget, Source.EndPoint, Element);
            var Alphause = TranslateAlpha(Source);
            using (SKPaint skp = new SKPaint() { Color = new SKColor(Source.Color.R, Source.Color.G, Source.Color.B, Alphause), StrokeWidth = 2 })
            {
                pRenderTarget.DrawLine(PointA, PointB, skp);
            }
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
            if(Source.Color.R == 255 && Source.Color.B == 0 && Source.Color.G == 0)
            {
                ;
            }
            BCPoint usePosition = new BCPoint(Source.Position.X, Source.Position.Y);
            //have to try to get the standardRenderingProvider. If we can then we will use the coordinates as if they are a block position- otherwise, we use the coordinates directly.
            usePosition = TranslatePosition(pOwner, pRenderTarget, Source.Position, Element);

            BCPoint PrevPosition = TranslatePosition(pOwner, pRenderTarget, Source.Position - Source.Velocity, Element);

            byte useAlpha =  TranslateAlpha(Source);
            if (SharePaint == null) SharePaint = new SKPaint() { Color = new SKColor(Source.Color.R, Source.Color.G, Source.Color.B, useAlpha), StrokeWidth = 1.2f };
            else SharePaint.Color = new SKColor(Source.Color.R, Source.Color.G, Source.Color.B, useAlpha);
            pRenderTarget.DrawLine(PrevPosition, usePosition, SharePaint);
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
        
        protected BCPoint TranslatePosition(IStateOwner pOwner, SKCanvas pRenderTarget, BCPoint Position, GameStateSkiaDrawParameters Element)
        {
            BCPoint Result = Position;
            GameplayGameState foundstandard = null;
            if (pOwner.CurrentState is GameplayGameState standard)
            {
                foundstandard = standard;
            }
            else if (pOwner.CurrentState is ICompositeState<GameplayGameState> composite)
            {
                foundstandard = composite.GetComposite();
            }

            if (foundstandard != null)
            {
                Result.X = foundstandard.PlayField.GetBlockWidth(Element.Bounds) * Position.X;
                Result.Y = foundstandard.PlayField.GetBlockHeight(Element.Bounds) * Position.Y;
            }
            return Result;
        }
    }


    //[RenderingHandler(typeof(TetrisBlock), typeof(Graphics), typeof(TetrisBlockDrawParameters))]
    //public class TetrisBlockGDIRenderingHandler : StandardRenderingHandler<Graphics, TetrisBlock, TetrisBlockDrawParameters>
    [RenderingHandler(typeof(List<BaseParticle>), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class ParticleRenderingSkiaHandler : StandardRenderingHandler<SKCanvas , List<BaseParticle>, GameStateSkiaDrawParameters>
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
        static Dictionary<Type, BASeCamp.Rendering.Interfaces.IRenderingHandler<IStateOwner>> ParticleRenderProviders = new Dictionary<Type, BASeCamp.Rendering.Interfaces.IRenderingHandler<IStateOwner>>();
        private BASeCamp.Rendering.Interfaces.IRenderingHandler<IStateOwner> GetProvider(Type forType)
        {
            if(!ParticleRenderProviders.ContainsKey(forType))
            {
                var getrenderer = RenderingProvider.Static.GetHandler(typeof(SKCanvas), forType, typeof(GameStateSkiaDrawParameters));
                if (getrenderer != null)
                    ParticleRenderProviders.Add(forType, getrenderer);

            }
            return ParticleRenderProviders[forType];
        }
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, List<BaseParticle> Source, GameStateSkiaDrawParameters Element)
        {
            if (Source.Count > 0)
            {
                Debug.Print("Rendering Particle Set of " + Source.Count.ToString());
                Debug.Print("Trace:" + new StackTrace().ToString());
            }
            foreach (var iterate in Source)
            {
                //retrieve rendering handler.
                var Grabrenderer = GetProvider(iterate.GetType());
                Grabrenderer.Render(pOwner, pRenderTarget, iterate, Element);
               

            }
            if(Source.Count > 0)
            {
                ;
            }
            
                if(pOwner.CurrentState.GameProcSuspended)
                {
                    ;
                }
            
            UpdateParticles(pOwner,Source);
            
        }
    }
}
