using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris
{
    //a "Hotline" indicates a row that, when a line is cleared within it, gives a bonus multiplier.
    public class HotLine
    {

        private Brush _LineBrush;
        private double _Multiplier;
        private Color _LineColor;
        private int _Row;
        public Brush LineBrush{ get { return _LineBrush; } }
        public Color Color {  get { return _LineColor; } set { _LineColor = value; } }
        public double Multiplier { get { return _Multiplier; } set { _Multiplier = value; }}
        public int Row {  get { return _Row; } set { _Row = value; } }
        public HotLine(Brush pLineBrush,double pMultiplier,int pRow)
        {
            _LineBrush = pLineBrush;
            Multiplier = pMultiplier;
            Row = pRow;
        }
        public HotLine(Color pLineColor,double pMultiplier,int pRow)
        {
            _LineColor = pLineColor;
            Multiplier = pMultiplier;
            Row = pRow;
        }
        
    }
}
