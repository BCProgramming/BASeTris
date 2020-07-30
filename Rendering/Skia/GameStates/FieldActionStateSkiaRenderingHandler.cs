using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeCamp.Rendering;
using BASeTris.Blocks;
using BASeTris.GameStates;
using BASeTris.Rendering.GDIPlus;
using SkiaSharp;

namespace BASeTris.Rendering.Skia
{

    [RenderingHandler(typeof(FieldLineActionGameState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class FieldLineActionStateSkiaRenderingHandler : FieldActionStateSkiaRenderingHandler

    {
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, FieldActionGameState Source, GameStateSkiaDrawParameters Element)
        {
            if(Source is FieldLineActionGameState state)
            {
                this.Render(pOwner, pRenderTarget, state, Element);
            }
            
        }
        public void Render(IStateOwner pOwner, SKCanvas pRenderTarget, FieldLineActionGameState Source, GameStateSkiaDrawParameters Element)
        {
            var Bounds = Element.Bounds;
            

            float BlockWidth = Bounds.Width /   Source.PlayField.ColCount;
            float BlockHeight = Bounds.Height / Source.PlayField.VisibleRows; //remember, we don't draw the top two rows- we start the drawing at row index 2, skipping 0 and 1 when drawing.
            bool FoundAnimated = false;

            if (Source._BaseState != null)
            {
                var newElement = new GameStateSkiaDrawParameters(Element.Bounds);
                newElement.TagData = new GamePlayGameStateDataTagInfo() { SkipParticlePaint = true };
                RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source._BaseState, newElement);
            }
             
            if (Source.ClearRowInfo != null)
            {
                foreach (var iterate in Source.ClearRowInfo)
                {
                    int currentRow = iterate.Key;
                    NominoBlock[] RowData = iterate.Value;
                    for (int drawCol = 0; drawCol < RowData.Length; drawCol++)
                    {
                        float YPos = (currentRow - Source.PlayField.HIDDENROWS) * BlockHeight;
                        float XPos = drawCol * BlockWidth;
                        var TetBlock = RowData[drawCol];
                        if (TetBlock != null)
                        {
                            SKRect BlockBounds = new SKRect(XPos, YPos, XPos + BlockWidth, YPos + BlockHeight);
                            TetrisBlockDrawSkiaParameters tbd = new TetrisBlockDrawSkiaParameters(pRenderTarget, BlockBounds, null, pOwner.Settings);
                            RenderingProvider.Static.DrawElement(pOwner, tbd.g, TetBlock, tbd);
                        }
                        else
                        {
                            ;
                        }
                    }
                }
            }
            //we told the main state not to paint particles, so we should paint them now.
            RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source.GetComposite().Particles, Element);


            if (Source is FieldLineActionGameState linestate)
            {
                if (linestate.FlashState)
                {
                    pRenderTarget.DrawRect(Element.Bounds, FlashBrush);
                }
            }


        }

        public override void RenderStats(IStateOwner pOwner, object pRenderTarget, object Element, object ElementData)
        {
            if(Element is FieldLineActionGameState state)
            {
                this.RenderStats((IStateOwner)pOwner, (SKCanvas)pRenderTarget, state, (GameStateSkiaDrawParameters)ElementData);
            }
            
        }
        public void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, FieldLineActionGameState Source, GameStateSkiaDrawParameters Element)
        {
            base.RenderStats(pOwner, pRenderTarget, Source, Element);
            

        }
    }

    [RenderingHandler(typeof(FieldActionGameState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class FieldActionStateSkiaRenderingHandler : StandardStateRenderingHandler<SKCanvas, FieldActionGameState, GameStateSkiaDrawParameters>
    {
        protected SKPaint FlashBrush = new SKPaint() { Color = new SKColor(255,255,255,100) };
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, FieldActionGameState Source, GameStateSkiaDrawParameters Element)
        {
            //needed change: right now the animation operates by changing the information in the field, and graphically we redraw that field.
            //however, that has performance implications since we repaint the entire field which (for some reason?) is now a costly operation.
            //for the line clears for example it should instead take those line clears, null them out, repaint the field once, and then for the duration of the clear operation we only repaint the blocks being cleared, in place of the now blank spot.
            //And of course after the clear operation those now empty lines get stripped and the others fall and the block field is redrawn.
            //and draw, but the 
            var g = pRenderTarget;
            if (Source._BaseState != null)
            {
                RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source._BaseState, Element);
                if (Source is FieldLineActionGameState linestate)
                {
                    if (linestate.FlashState)
                    {
                        g.DrawRect(Element.Bounds, FlashBrush);
                    }
                }
            }
        }

        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, FieldActionGameState Source, GameStateSkiaDrawParameters Element)
        {
            if (Source._BaseState != null)
            {
                
                RenderingProvider.Static.DrawStateStats(pOwner, pRenderTarget, Source._BaseState, Element);
            }
        }
    }
}
