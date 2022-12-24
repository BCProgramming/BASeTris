using BASeTris.Rendering.Skia;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering
{
    /// <summary>
    /// This class is responsible for effectively taking an arrangement of Nominos and a theme and some additional control properties, and generating a Bitmap out of it.
    /// The aim/purpose of this is for visual flourishes or elements on stuff like the title screen. Eg it would have corner/borders constructed out of tetrominoes- at least, that is the idea.
    /// </summary>
    public class TetrominoCollageRenderer
    {

        private TetrisField _field = null;
        public NominoTheme Theme { get { return _field.Theme; } set { _field.Theme = value; } }
        public int ColumnCount { get; set; }
        public int RowCount { get; set; }
        public int ColumnWidth { get; set; }
        public int RowHeight { get; set; }

        public void AddNomino(Nomino Source)
        {

            _field.SetGroupToField(Source);

        }

        public TetrominoCollageRenderer(int pColumnCount, int pRowCount, int pColumnWidth, int pRowHeight,NominoTheme _theme)
        {
            //Note: We can get away with no CustomizationHandler here, as we aren't going to actually be having the Field "do" anything other than use it as a render source.
            ColumnCount = pColumnCount;
            RowCount = pRowCount;
            ColumnWidth = pColumnWidth;
            RowHeight = pRowHeight;
            _field = new TetrisField(_theme, null, RowCount, ColumnCount);
        }
        public SKBitmap Render()
        {
            Size BitmapSize = new Size((int)ColumnCount * (ColumnWidth+ 1), (int)RowHeight* (RowCount+ 1));
            SKImageInfo info = new SKImageInfo(BitmapSize.Width, BitmapSize.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            SKBitmap BuiltRepresentation = new SKBitmap(info, SKBitmapAllocFlags.ZeroPixels);

            using (SKCanvas DrawRep = new SKCanvas(BuiltRepresentation))
            {
                DrawRep.Clear(SKColors.Transparent);

                Object grabdata = RenderingProvider.Static.GetExtendedData(_field.GetType(), _field,
                    (o) =>
                        new TetrisFieldDrawSkiaParameters()
                        {
                            Bounds = new SKRect(0, 0, BitmapSize.Width, BitmapSize.Height),
                            COLCOUNT = ColumnCount,
                            ROWCOUNT = RowCount,
                            FieldBitmap = null,
                            LastFieldSave = SKRect.Empty,
                            VISIBLEROWS = RowCount
                        }) ;

                TetrisFieldDrawSkiaParameters parameters = (TetrisFieldDrawSkiaParameters)grabdata;
                parameters.LastFieldSave = parameters.Bounds;
                //if (!Source.GameHandler.AllowFieldImageCache) parameters.FieldBitmap = null;
                RenderingProvider.Static.DrawElement(null, DrawRep, _field, parameters);
            }


            return BuiltRepresentation;

        }

    }
}
