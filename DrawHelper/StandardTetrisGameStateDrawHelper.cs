using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.AssetManager;
using BASeTris.GameStates;
using BASeTris.Rendering;
using BASeTris.Rendering.GDIPlus;
using BASeTris.Rendering.RenderElements;
using BASeTris.Blocks;

namespace BASeTris.DrawHelper
{
    public class StandardTetrisGameStateDrawHelper
    {
        private RectangleF StoredBackground = RectangleF.Empty;
        private Brush GhostBrush = new SolidBrush(Color.FromArgb(75, Color.DarkBlue));
        RectangleF StoredBlockImageRect = RectangleF.Empty;
        Image StoredBlockImage = null;
        Bitmap useBackground = null;
        private void RefreshBackground(GameplayGameState pState,RectangleF buildSize)
        {
            StoredBackground = buildSize;
            useBackground = new Bitmap((int)buildSize.Width, (int)buildSize.Height, PixelFormat.Format32bppPArgb);
            using (Graphics bgg = Graphics.FromImage(useBackground))
            {
                var bgdrawdata = pState.PlayField.Theme.GetThemePlayFieldBackground(pState.PlayField,pState.GameHandler);
                var drawbg = bgdrawdata.BackgroundImage;
                bgg.CompositingQuality = CompositingQuality.AssumeLinear;
                bgg.InterpolationMode = InterpolationMode.NearestNeighbor;
                bgg.SmoothingMode = SmoothingMode.HighSpeed;
                if (bgdrawdata.TintColor != Color.Transparent)
                {
                    ImageAttributes useAttributes = new ImageAttributes();
                    var getmatrix = ColorMatrices.GetColourizer(bgdrawdata.TintColor);
                    useAttributes.SetColorMatrix(getmatrix);
                    bgg.DrawImage(drawbg, new RectangleF(0f, 0f, buildSize.Width, buildSize.Height),useAttributes);
                }
                else
                {
                    bgg.DrawImage(drawbg, new RectangleF(0f, 0f, buildSize.Width, buildSize.Height), new RectangleF(0f, 0f, drawbg.Width, drawbg.Height), GraphicsUnit.Pixel);
                }
            }
        }
        
        public void DrawProc(GameplayGameState pState, IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            if (useBackground == null || !StoredBackground.Equals(Bounds) || pState.DoRefreshBackground)
            {
                RefreshBackground(pState,Bounds);
            }

            g.DrawImage(useBackground, Bounds);
            var PlayField = pState.PlayField;

            if (PlayField != null)
            {
                PlayField.Draw(pOwner,g, Bounds);
            }


            foreach (var activeblock in PlayField.BlockGroups)
            {
                int dl = 0;
                var GrabGhost = pState.GetGhostDrop(pOwner,activeblock, out dl, 3);
                if (GrabGhost != null)
                {
                    var BlockWidth = PlayField.GetBlockWidth(Bounds);
                    var BlockHeight = PlayField.GetBlockHeight(Bounds);

                    foreach (var iterateblock in activeblock)
                    {
                        RectangleF BlockBounds = new RectangleF(BlockWidth * (GrabGhost.X + iterateblock.X), BlockHeight * (GrabGhost.Y + iterateblock.Y - 2), PlayField.GetBlockWidth(Bounds), PlayField.GetBlockHeight(Bounds));
                        TetrisBlockDrawGDIPlusParameters tbd = new TetrisBlockDrawGDIPlusParameters(g, BlockBounds, GrabGhost,pOwner.Settings);
                        ImageAttributes Shade = new ImageAttributes();
                        Shade.SetColorMatrix(ColorMatrices.GetFader(0.5f));
                        tbd.ApplyAttributes = Shade;
                        //tbd.OverrideBrush = GhostBrush;
                        var GetHandler = RenderingProvider.Static.GetHandler(typeof(Graphics), iterateblock.Block.GetType(), typeof(TetrisBlockDrawGDIPlusParameters));
                        GetHandler.Render(pOwner,tbd.g,iterateblock.Block,tbd);
                        //iterateblock.Block.DrawBlock(tbd);
                    }
                }
            }
        }
    }
}
