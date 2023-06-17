using BASeCamp.Rendering;
using BASeTris.BackgroundDrawers;
using BASeTris.GameStates;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia.GameStates
{
    [RenderingHandler(typeof(DesignBackgroundState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class DesignBackgroundStateSkiaRenderingHandler : StandardStateRenderingHandler<SKCanvas, DesignBackgroundState, GameStateSkiaDrawParameters>
    {
        
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, DesignBackgroundState Source, GameStateSkiaDrawParameters Element)
        {
            //throw new NotImplementedException();

            //stuff we want to render:

            //background:

           
            
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

            //Now, we <could> build a collage, and render that. But, we want to display the selected nomino more prominently, so we should instead render all the Nominoes directly, so we can render the selected one differently (with an Alpha SKPaint that pulses, I would argue).
            //however we will probably have a way to create a collage for the purpose of testing the background.
            //TetrisBlockDrawSkiaParameters tbd = new TetrisBlockDrawSkiaParameters(g, new SKRect(DrawBlockX, DrawBlockY, DrawBlockX + BlockSize.Width, DrawBlockY + BlockSize.Height), null,   pOwner.Settings);

            //desiredsize of the edit area is a third of the screen in the middle. we'll scale to fit that size. (Note, however, that elements can render outside that area... for the moment...
            for (int layerindex = 0; layerindex < Source.Layers.Length; layerindex++)
            {
                if (layerindex > Source.LayerIndex) continue; //don't paint layers above the current layer.
                var Layer = Source.Layers[layerindex];
                
                SKRect EditArea = new SKRect(Bounds.Width / 4, Bounds.Height / 4, Bounds.Width - Bounds.Width / 4, Bounds.Height - Bounds.Height / 4);
                SKSize NominoBlockSize = new SKSize((float)(EditArea.Width / (float)(Layer.DesignColumns)), (float)(EditArea.Height / (float)(Layer.DesignRows)));
                var duplicated = Layer.DesignNominoes;
                var useSelected = Layer.SelectedIndex;


                for (int xgrid = 0; xgrid < Layer.DesignColumns; xgrid++)
                {
                    for (int ygrid = 0; ygrid < Layer.DesignRows; ygrid++)
                    {
                        SKPoint Location = new SKPoint(
                            (float)(EditArea.Left + (double)(NominoBlockSize.Width) * (double)(xgrid)),
                            (float)(EditArea.Top + (double)NominoBlockSize.Height * (double)(ygrid)

                            ));
                        SKRect BlockBound = new SKRect(Location.X, Location.Y, (float)(Location.X + NominoBlockSize.Width), (float)(Location.Y + NominoBlockSize.Height));
                        pRenderTarget.DrawRect(BlockBound, new SKPaint() { Color = Layer.GridColor, StrokeWidth = 1, Style = SKPaintStyle.Stroke });

                    }


                }

                //pRenderTarget.DrawRect(EditArea, new SKPaint() { Color = SKColors.Yellow, StrokeWidth = 1,Style=SKPaintStyle.Stroke });
                SKPaint RenderPaintBlock = new SKPaint();

                for (int index = 0; index < duplicated.Count; index++)
                {
                    if (Source.SelectedIndex == index)
                    {
                        RenderPaintBlock.Color = new SKColor(255, 255, 255, (byte)(100 + (78 * (Math.Sin((float)TetrisGame.GetTickCount() / 500f)))));
                    }
                    var CurrentElement = duplicated[index];


                    foreach (var renderblock in CurrentElement.GetBlockData())
                    {

                        SKPoint Location = new SKPoint(EditArea.Left + (float)(NominoBlockSize.Width) * (float)(renderblock.X + CurrentElement.X), EditArea.Top + (float)NominoBlockSize.Height * (float)(renderblock.Y + CurrentElement.Y));
                        SKRect BlockBound = new SKRect(Location.X, Location.Y, (float)(Location.X + NominoBlockSize.Width), (float)(Location.Y + NominoBlockSize.Height));
                        using (SKAutoCanvasRestore restore = new SKAutoCanvasRestore(pRenderTarget))
                        {
                            pRenderTarget.ClipRect(BlockBound);
                            RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, renderblock.Block, new TetrisBlockDrawSkiaParameters(pRenderTarget, BlockBound, CurrentElement, pOwner.Settings));
                            if (Layer.SelectedIndex == index)
                            {
                                pRenderTarget.DrawRect(BlockBound, RenderPaintBlock);
                            }
                        }

                    }


                }
            }
            //we want to render the controls along the bottom, too. Since we can map controls to bitmaps, may as well right?
            //also it would look sort a professional? Maybe.




        }

        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, DesignBackgroundState Source, GameStateSkiaDrawParameters Element)
        {
            //throw new NotImplementedException();
        }
    }
}
