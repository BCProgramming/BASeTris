using BASeTris.AssetManager;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.FrameBufferEffects
{
    //todo: should be an abstract base class for FrameBuffer effects like this one, so we can have other effects employed and used by BASeTrisTK.
    public class GhostlyFrameBufferEffect : FrameBufferEffect<SKImage>
    {

        long FrameTickDelay = 5;
        //FrameBufferRecorderQueue<SKImage> Buffer = null;
        IBufferMultiHistoryProvider<SKImage> BufferSrc = null;

        public Object GhostObjectLock = new object();
        public int GhostAlphaPaintCount { get { return GhostAlphaPaint == null ? 0 : GhostAlphaPaint.Length; } }
        SKPaint[] GhostAlphaPaint = null;
        private int _NumGhostedFrames = 1; //experimental. This probably needs to be changed to be based on time instead of a specific count of frames. 0=disabled.

        private IBufferMultiHistoryProvider<SKImage> BufferSource = null;

        GhostlyFrameBufferEffect(IBufferMultiHistoryProvider<SKImage> EFBSource) : base(EFBSource)
        {
            BufferSrc = EFBSource;
        }

        public SKPaint PaintItem(int Index)
        {
            return GhostAlphaPaint[Index];
        }
        public int NumGhostedFrames { get { return _NumGhostedFrames; } set { _NumGhostedFrames = value; } }
        public GhostlyFrameBufferEffect(IBufferMultiHistoryProvider<SKImage> EFBSource, int pNumGhostedFrames = 1, float pStartAlpha = 0.9f, float pEndAlpha = 0.9f) : this(EFBSource)
        {

            _GhostStartAlpha = pStartAlpha;
            _GhostEndAlpha = pEndAlpha;
        }
        public override bool InitializationRequired()
        {
            return BufferSrc.FrameCount != GhostAlphaPaintCount;
        }
        public void Initialize()
        {
            lock (GhostObjectLock)
            {
                if (GhostAlphaPaint != null) foreach (var iteratepaint in GhostAlphaPaint)
                    {
                        //if (iteratepaint != null) iteratepaint.Dispose();
                    }
                GhostAlphaPaint = new SKPaint[_NumGhostedFrames];
                for (int i = 0; i < _NumGhostedFrames; i++)
                {
                    float UseAlpha = _GhostStartAlpha + ((_GhostStartAlpha - _GhostEndAlpha) / (float)BufferSrc.FrameCount) * (float)i;
                    SKPaint BuildAlphaPaint = new SKPaint() { ColorFilter = SKColorMatrices.GetFader(UseAlpha) };
                    GhostAlphaPaint[i] = BuildAlphaPaint;
                }
            }

        }
        private float _GhostStartAlpha = .9f;
        private float _GhostEndAlpha = .9f;
        public bool IsEffectFrame(ulong Tick)
        {
            return _NumGhostedFrames > 0 && (BufferSrc.LastFrameTick + (ulong)FrameTickDelay < Tick);
        }


    }
}
