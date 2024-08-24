using BASeTris.AssetManager;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.FrameBufferEffects
{
    //Frame Buffer Effects won't themselves have the actual frame data. Instead, that will be provided by a DI Interface that is supplied that provides the frame buffer history. Generally speaking this is going to be the Game Presenter, but could, really, be anything.
    //subsequently, that implementation will tend to rely on a FrameBufferRecorderQueue for actually keeping track of the buffer entries, and the interface implementation delegates to it.
    public abstract class FrameBufferEffect<T>
    {
        [Flags]
        public enum BufferEffectFlags
        {
            None,
            Background_Only,
        }
        IBufferMultiHistoryProvider<T> _Data;
        public FrameBufferEffect(IBufferMultiHistoryProvider<T> Provider)
        {
            _Data = Provider;
        }
        public abstract bool InitializationRequired();


    }

    public interface IBufferHistoryProvider<T>
    {
        T GetLastFrame();
    }
    public interface IBufferMultiHistoryProvider<T> : IBufferHistoryProvider<T>
    {
        IEnumerable<T> GetFrames();
        int FrameCount { get; }

        ulong LastFrameTick { get; set; }
    }
    public interface ISkiaBufferHistoryProvider : IBufferMultiHistoryProvider<SKImage>
    {
    }
    public class FrameBufferRecorderQueue<T>
    {
        public ulong LastFrameTick { get; set; } = ulong.MinValue;
        public int BufferSize { get; set; } = 1;
        public ConcurrentQueue<T> Frames = new ConcurrentQueue<T>();
        public int FrameCount { get { return Frames.Count; } }
        public void AddFrame(T SurfaceSnapshot, uint pTickTime)
        {
            if (BufferSize > 0)
            {
                T grabimage = SurfaceSnapshot;
                lock (Frames)
                {
                    Frames.Enqueue(grabimage);
                }
                while (Frames.Count > BufferSize)
                {
                    T getevicted = default;
                    Frames.TryDequeue(out getevicted);
                    if (grabimage.Equals(getevicted))
                    {
                        ;
                    }
                    if (getevicted is IDisposable idd) idd.Dispose();
                }
            }
            LastFrameTick = pTickTime;
        }
        public bool HasFrames => Frames.Any();
    }

}
