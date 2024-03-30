using BASeCamp.Logging;
using BASeCamp.Rendering;
using BASeTris.AssetManager;
using BASeTris.BackgroundDrawers;
using BASeTris.GameStates;
using ManagedBass;
using OpenTK.Graphics.ES20;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia.GameStates
{
    //TODO: fix this up to have the snapshots properly handle re-entrancy.
    public class TransitionStateBitmapCacheElement
    {
        public SKBitmap Image { get; private set; }
        public GameState SourceState { get; private set; }
        public TransitionStateBitmapCacheElement(GameState pState, SKBitmap pImage)
        {
            Image = pImage;
            SourceState = pState;
        }
        
    }
    public abstract class TransitioningStateSkiaRenderingHandler : StandardStateRenderingHandler<SKCanvas, TransitionState, GameStateSkiaDrawParameters>
    {
        [Flags]
        public enum TransitionTypeConstants
        {
            TransitionType_Neither = 0,
            TransitType_Prev_Composite = 1,
            TransitType_Next_Composite = 2,
            TransitType_Dual = 3
        }




        //SKBitmap CachePrev = null;
        //SKBitmap CacheNext = null;
        
        //GameState CachedPrevState = null;
        //GameState CachedNextState = null; //keep track of what state ewe actually have a cached bitmap for.
        protected TransitionTypeConstants TransitionType = TransitionTypeConstants.TransitType_Dual;

        private void PaintState(IStateOwner pOwner,GameState State, SKCanvas skc,GameStateSkiaDrawParameters Element)
        {
            skc.Clear(SKColors.Pink);
            if (State.SupportedDisplayMode == GameState.DisplayMode.Partitioned)
            {
                StandardTetrisGameStateSkiaRenderingHandler.PaintPartitionedState(pOwner, State, skc, Element,out _,out _);
            }
            else
            {
                RenderingProvider.Static.DrawElement(pOwner, skc, State, Element);
            }

        }
        public const String NEXTSNAPKEY = "TransitState::NextSnapshot";
        public const String PREVSNAPKEY = "TransitState::PrevSnapshot";
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, TransitionState Source, GameStateSkiaDrawParameters Element)
        {
            //we want to render both, and then blit from those sources to the target.


            SKBitmap BitmapPrev = null;
            SKBitmap BitmapNext = null;

            //for (hopefully!) better performance, we'll do the drawing for the two states at the same time via async. 
            Action NextAction = () =>
            {
                if (TransitionType.HasFlag(TransitionTypeConstants.TransitType_Prev_Composite))
                {

                    if (Source.SnapshotSettings.HasFlag(TransitionState.SnapshotConstants.Snapshot_Previous) && Source.HasCustomProperty(PREVSNAPKEY))
                    {



                        BitmapPrev = (SKBitmap)Source.GetCustomProperty(PREVSNAPKEY);
                    }
                    else
                    {
                        if (!Source.GameProcDelegationMode.HasFlag(TransitionState.DelegateProcConstants.Delegate_Previous) && Source.SnapshotSettings.HasFlag(TransitionState.SnapshotConstants.Snapshot_Previous)) Source.PreviousState.GameProc(pOwner); //for snapshot, if not running gameproc, we want to call it once.
                        BitmapPrev = new SKBitmap((int)Element.Bounds.Width, (int)Element.Bounds.Height);

                        using (SKCanvas skc = new SKCanvas(BitmapPrev))
                        {
                            PaintState(pOwner, Source.PreviousState, skc, Element);
                        }
                        if (Source.SnapshotSettings.HasFlag(TransitionState.SnapshotConstants.Snapshot_Previous))
                        {
                            BitmapPrev.SetImmutable();
                            Source.SetCustomProperty(PREVSNAPKEY, BitmapPrev);
                        }
                    }
                }
            };
            Action PrevAction = () =>
            {
                if (TransitionType.HasFlag(TransitionTypeConstants.TransitType_Next_Composite))
                {
                    if (Source.SnapshotSettings.HasFlag(TransitionState.SnapshotConstants.Snapshot_Next) && Source.HasCustomProperty(NEXTSNAPKEY))
                    {
                        BitmapNext = (SKBitmap)Source.GetCustomProperty(NEXTSNAPKEY);

                    }
                    else
                    {
                        if (!Source.GameProcDelegationMode.HasFlag(TransitionState.DelegateProcConstants.Delegate_Next) && Source.SnapshotSettings.HasFlag(TransitionState.SnapshotConstants.Snapshot_Next)) Source.NextState.GameProc(pOwner); //for snapshot, if not running gameproc for next state, we want to call it at least once.
                        BitmapNext = new SKBitmap((int)Element.Bounds.Width, (int)Element.Bounds.Height);
                        using (SKCanvas skb = new SKCanvas(BitmapNext))
                        {
                            PaintState(pOwner, Source.NextState, skb, Element);
                        }
                        if (Source.SnapshotSettings.HasFlag(TransitionState.SnapshotConstants.Snapshot_Next))
                        {
                            BitmapNext.SetImmutable();
                            Source.SetCustomProperty(NEXTSNAPKEY, BitmapNext);
                        }
                    }
                }
            };

            Task PrevTask = Task.Run(() =>
            {
                PrevAction();
            });
            Task NextTask = Task.Run(() =>
            {
                NextAction();
            });

            Task.WaitAll(PrevTask, NextTask);

            //this is not in the State GameProc itself, because the drawing itself can sometimes take some time. If the transition time elapses in that time than we won't really see it.
            Debug.Assert(BitmapPrev != null);
            Debug.Assert(BitmapNext != null);
            
            RenderTransition(pOwner, pRenderTarget, BitmapPrev, BitmapNext, Source, Element);
            Thread.Sleep(0);
            if (Source.StartTime == null) Source.StartTime = DateTime.Now;
            

        }
        public abstract void RenderTransition(IStateOwner pOwner, SKCanvas Target, SKBitmap Previous, SKBitmap Next, TransitionState Source, GameStateSkiaDrawParameters Element);

        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, TransitionState Source, GameStateSkiaDrawParameters Element)
        {
           
            //throw new NotImplementedException();
        }
    }

    [RenderingHandler(typeof(TransitionState_BoxWipe), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class BoxWipeTransitionStateSkiaRenderingHandler : TransitioningStateSkiaRenderingHandler
    {
        

        
        public override void RenderTransition(IStateOwner pOwner, SKCanvas Target, SKBitmap Previous, SKBitmap Next, TransitionState Source, GameStateSkiaDrawParameters Element)
        {
            var elem = Source as TransitionState_BoxWipe;
            //paint the old state bitmap directly.
            Target.DrawBitmap(Previous, new SKPoint(0, 0));
            //now, blit parts of the new bitmap.
            double Percentage = Source.TransitionPercentage;
            for (int x = 0; x < Element.Bounds.Width; x += elem.BoxWidth)
            {
                for (int y = 0; y < Element.Bounds.Height; y += elem.BoxHeight)
                {
                    float UseWidth = (float)(elem.BoxWidth * Percentage);
                    float UseHeight = (float)(elem.BoxHeight * Percentage);


                    SKRect ShadeBox = new SKRect(x, y, (float)(x + UseWidth), (float)(y + UseHeight));
                    Target.DrawBitmap(Next, ShadeBox, ShadeBox);
                }
            }




        }
    }

    [RenderingHandler(typeof(TransitionState_AlphaBlend), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class AlphaBlendTransitionStateSkiaRenderingHandler : TransitioningStateSkiaRenderingHandler
    {
        


        public override void RenderTransition(IStateOwner pOwner, SKCanvas Target, SKBitmap Previous, SKBitmap Next, TransitionState Source, GameStateSkiaDrawParameters Element)
        {

            //paint the old state bitmap directly.
            var filter = SKColorMatrices.GetFader((float)(Source.TransitionPercentage));

            Target.DrawBitmap(Previous, new SKPoint(0, 0));
            Target.DrawBitmap(Next, new SKPoint(0, 0), new SKPaint() { ColorFilter = filter });
            //now, blit parts of the new bitmap.





        }


    }
   
    [RenderingHandler(typeof(TransitionState_Melt), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class TransitionState_MeltSkiaRenderingHandler : TransitioningStateSkiaRenderingHandler
    {
        public TransitionState_MeltSkiaRenderingHandler()
        {
            TransitionType = TransitionTypeConstants.TransitType_Dual;
        }
        public override void RenderTransition(IStateOwner pOwner, SKCanvas Target, SKBitmap Previous, SKBitmap Next, TransitionState Source, GameStateSkiaDrawParameters Element)
        {
            if (Source is TransitionState_Melt tmelt)
            {
                using (SKAutoCanvasRestore restore = new SKAutoCanvasRestore(Target))
                {
                    Target.DrawBitmap(Next, new SKPoint(0, 0));
                }
                if (tmelt.MeltOffset == null)
                {
                    List<int> OffsetList = new List<int>();
                    //initialize melt offsets
                    for (int i = 0; i < Element.Bounds.Width; i += tmelt.Size)
                    {
                        //offset determines the offset (minimum zero) of a particular vertical slice, from the origin point melting down the screen. The maximum is Element.Bounds.Height and the movement is between the top of the screen and the height plus an added quarter.

                        int ChooseOffset = -(int)(TetrisGame.StatelessRandomizer.NextDouble() * Element.Bounds.Height / 20);
                        OffsetList.Add(ChooseOffset);


                    }
                    tmelt.MeltOffset = OffsetList;
                }


                //ok, now, first: calculate the melt position. This is between the top and the height plus a quarter.

                double MeltPos = Source.TransitionPercentage * Element.Bounds.Height * 1.05;
                using (SKImage PrevImage = SKImage.FromBitmap(Previous))
                {
                    //now go through all slices....
                    int meltindex = 0;
                    double UseWidth = Element.Bounds.Width / tmelt.MeltOffset.Count;
                    foreach (int Offset in tmelt.MeltOffset)
                    {
                        double CurrX = meltindex * tmelt.Size;
                        double UseY = Math.Max(0, MeltPos + (double)Offset);
                        SKRect SrcRect = new SKRect((float)CurrX, 0, (float)(CurrX + UseWidth), Element.Bounds.Height);
                        SKRect DestRect = new SKRect((float)CurrX, (float)UseY, (float)(CurrX + UseWidth), (float)(UseY + Element.Bounds.Height));
                        Target.DrawImage(PrevImage, SrcRect, DestRect);
                        meltindex++;
                    }

                }


                
            }
            //next is underneath
            




        }
    }
    [RenderingHandler(typeof(TransitionState_Blur), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class TransitionState_BlurSkiaRenderingHandler : TransitioningStateSkiaRenderingHandler
    {
        public override void RenderTransition(IStateOwner pOwner, SKCanvas Target, SKBitmap Previous, SKBitmap Next, TransitionState Source, GameStateSkiaDrawParameters Element)
        {
            double CurrentTotalPercentage = Source.TransitionPercentage;
            double usePercentage = 0;
            DebugLogger.Log.WriteLine("Transition percentage:" + CurrentTotalPercentage);
            if (CurrentTotalPercentage < .45)
            {
                if (CurrentTotalPercentage > 0.2) {; }
                usePercentage = CurrentTotalPercentage / .45;
            }
            else if (CurrentTotalPercentage < .55)
                usePercentage = 1.00;

            else
            {
                usePercentage = 1 - (CurrentTotalPercentage - .55) / .45;
            }
            usePercentage = TetrisGame.ClampValue(usePercentage, 0, 1);
            SKBitmap chooseSource = CurrentTotalPercentage < .50 ? Previous : Next;

            PaintBlur(Target, chooseSource, (float)usePercentage, Source, Element);
        }
        private void PaintBlur(SKCanvas Target, SKBitmap Source, float Percentage, TransitionState SourceState, GameStateSkiaDrawParameters Element, int XOffset = 0, int YOffset = 0)
        {
            SKMaskFilter mask = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 50 * Percentage);
            SKImageFilter ImageFilter = SKImageFilter.CreateBlur(20 * Percentage, 20 * Percentage);
            SKPaint paint = new SKPaint()
            {
                ImageFilter = ImageFilter,
                Color = new SKColor(0, 0, 0, 200),
                MaskFilter = mask,
            };
            Target.DrawBitmap(Source, SKPoint.Empty, paint);

        }
    }
  
    /*
     SKMaskFilter mask = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 50);
       SKImageFilter ImageFilter = SKImageFilter.CreateBlur(5, 5);
       SKPaint paint = new SKPaint() {
           ImageFilter = ImageFilter,
           Color = new SKColor(0, 0, 0, 200),
           MaskFilter = mask,
       };
            g.DrawRect(Element.Bounds, paint);
     
     */
    [RenderingHandler(typeof(TransitionState_Pixelate), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class TransitionState_PixelateSkiaRenderingHandler : TransitioningStateSkiaRenderingHandler
    {
        public override void RenderTransition(IStateOwner pOwner, SKCanvas Target, SKBitmap Previous, SKBitmap Next, TransitionState Source, GameStateSkiaDrawParameters Element)
        {
            //0->45% we paint the previous state and progress through "pixelation".
            double CurrentTotalPercentage = Source.TransitionPercentage;
            double usePercentage = 0;
            DebugLogger.Log.WriteLine("Transition percentage:" + CurrentTotalPercentage);
            if (CurrentTotalPercentage < .45)
            {
                if (CurrentTotalPercentage > 0.2) {; }
                    usePercentage = CurrentTotalPercentage/ .45;
            }
            else if (CurrentTotalPercentage < .55)
                usePercentage = 1.00;

            else
            {
                usePercentage = 1 - (CurrentTotalPercentage - .55) / .45;
            }
            usePercentage = TetrisGame.ClampValue(usePercentage, 0, 1);
            SKBitmap chooseSource = CurrentTotalPercentage < .50 ? Previous : Next;

            PaintPixelated(Target, chooseSource, (float)usePercentage, Source, Element);
            //45%-50% we paint the previous state at maximum pixelation.
            //50-55% we paint the next state at maximum pixelation.
            //55-100% we paint the next state and progress in reverse order through pixelation.
        }
        private int ChoosePixelSize(float Percentage, GameStateSkiaDrawParameters Element)
        {
            //max pixelation is a quarter the size of the bounds.
            //minimum is 1.
            var calculated =  (int)((float)Percentage * (Math.Max(Element.Bounds.Width, Element.Bounds.Height)/8 - 1f) + 1f);
            calculated = Math.Max(3, calculated);
            return calculated;
        }
        
        private void PaintPixelated(SKCanvas Target, SKBitmap Source,float Percentage, TransitionState SourceState, GameStateSkiaDrawParameters Element,int XOffset=0,int YOffset=0)
        {
            var SrcImage = SKImage.FromBitmap(Source);
            //percentage: amount of pixelation.
            int ChosenPixelSize = ChoosePixelSize(Percentage, Element);
            if (ChosenPixelSize <=4)
            {
                Target.DrawBitmap(Source, new SKPoint(0, 0));
            }
            else
            {
                for (int y = 0; y < Element.Bounds.Height; y += ChosenPixelSize)
                {
                    for (int x = 0; x < Element.Bounds.Width; x += ChosenPixelSize)
                    {
                        //sample pixel from Source image
                        //SKColor useColor = Source.GetPixel(x + ChosenPixelSize / 2, y + ChosenPixelSize / 2);
                        int ChoseX = x + ChosenPixelSize / 2;
                        int ChoseY = y + ChosenPixelSize / 2;
                        SKRect skr = new SKRect(x, y, x + ChosenPixelSize, y + ChosenPixelSize);
                        Target.DrawImage(SrcImage, new SKRect(ChoseX, ChoseY, ChoseX + 1, ChoseY + 1), skr);

                    }
                }
            }

        }
    }


    //TransitionState_BlockBuild 
    [RenderingHandler(typeof(TransitionState_BlockRandom), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class TransitionState_BlockRandomSkiaRenderingHandler : TransitioningStateSkiaRenderingHandler
    {
        
        public TransitionState_BlockRandomSkiaRenderingHandler()
        {
            
           // TransitionType = TransitionTypeConstants.TransitType_Dual;
        }
        private List<SKRect> ShuffleBuild(List<SKRect> Source)
            {
            List<SKRect> Result = new List<SKRect>();

            //separate by columns.
            Dictionary<int,List<SKRect>> ColumnSets = new Dictionary<int,List<SKRect>>();
            foreach (var iterate in Source)
            {
                int Columnindex = (int)(iterate.Left / iterate.Width);
                if (!ColumnSets.ContainsKey(Columnindex))
                    ColumnSets.Add(Columnindex, new List<SKRect>());

                ColumnSets[Columnindex].Add(iterate);
            }

            Dictionary<int, Queue<SKRect>> ColumnQueue = new Dictionary<int, Queue<SKRect>>();
            foreach (var iterate in ColumnSets)
            {
                Queue<SKRect> BuildColumnQueue = new Queue<SKRect>(from p in iterate.Value orderby p.Top descending select p);
                ColumnQueue.Add(iterate.Key, BuildColumnQueue);
            }

            while (ColumnQueue.Any((d) => d.Value.Any()))
            {
                var choosequeue = TetrisGame.Choose(ColumnQueue.Values);
                if (choosequeue.Any())
                {
                    Result.Add(choosequeue.Dequeue());
                }


            }
            return Result;




            }
        public override void RenderTransition(IStateOwner pOwner, SKCanvas Target, SKBitmap Previous, SKBitmap Next, TransitionState Source, GameStateSkiaDrawParameters Element)
        {
            using (SKAutoCanvasRestore restore = new SKAutoCanvasRestore(Target))
            {
                Target.DrawBitmap(Previous, new SKPoint(0, 0));
            }
            if (Source is TransitionState_BlockRandom tsb)
            {
                List<SKRect> BlockList = new List<SKRect>();
                if (tsb.ShuffledBlocks == null)
                {
                    //initialize. First create a full list of all the "blocks" we want to draw.
                    for (int x = 0; x < Element.Bounds.Width; x+=tsb.BlockSize)
                    {
                        for (int y = 0; y < Element.Bounds.Height; y += tsb.BlockSize)
                        {
                            SKRect createrect = new SKRect(x, y, x + tsb.BlockSize, y + tsb.BlockSize);
                            BlockList.Add(createrect);
                        }
                    }

                    //with the blocks added, shuffle it and assign it to the state.
                    //var shuffled = (from t in RandomHelpers.Static.Shuffle(BlockList, new Random()) orderby t.Top,TetrisGame.StatelessRandomizer.NextDouble() select t).ToArray();
                    var shuffled = ShuffleBuild(BlockList);
                    tsb.ShuffledBlocks = shuffled.ToList();

                }
                SKPaint Fill = new SKPaint() { Color = SKColors.Black };
                Fill.IsStroke = false;
                int NumDraw = (int)(tsb.TransitionPercentage * tsb.ShuffledBlocks.Count);
                if (NumDraw > tsb.ShuffledBlocks.Count - 1) NumDraw = tsb.ShuffledBlocks.Count - 1;
                for (int drawindex = 0; drawindex < NumDraw; drawindex++)
                {
                    Target.DrawBitmap(Next, tsb.ShuffledBlocks[drawindex], tsb.ShuffledBlocks[drawindex]);
                }
            }
            //RenderingProvider.Static.DrawElement(pOwner, Target, Source.BG, new SkiaBackgroundDrawData(Element.Bounds));
            

        }
    }

}
