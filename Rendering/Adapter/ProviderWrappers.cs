using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace BASeTris.Rendering.Adapters
{
    //adapter classes needed for:
    //PointF, SKPoint
    //Color, SKColor
    //Some form of ColorMatrix


    public class BCFont

    {
        [Flags]
        public enum BCFontStyle
        {
            Regular=0,
            Bold=1,
            Italic=2,
            Underline=4,
            Strikeout=8
        }
        public String FontFace { get; set; }
        public float FontSize { get; set; }
        
        public BCFontStyle FontStyle { get; set; }
        public BCFont(String pFontFace,float pFontSize,BCFontStyle pStyle)
        {
            FontFace = pFontFace;
            FontSize = pFontSize;
            FontStyle = pStyle;
        }
        public static implicit operator Font(BCFont pSource)
        {

            Font buildResult = new Font(pSource.FontFace,pSource.FontSize,GetGDIPlusFontStyle(pSource.FontStyle));
            return buildResult;

        }
        public static implicit operator BCFont(Font pSource)
        {
            BCFont buildResult = new BCFont(pSource.FontFamily.Name, pSource.Size, GetBCStyleFromGDIPlusStyle(pSource.Style));
            return buildResult;
        }
        public static FontStyle GetGDIPlusFontStyle(BCFontStyle pSrc)
        {
            FontStyle result = System.Drawing.FontStyle.Regular;
            if (pSrc.HasFlag(BCFontStyle.Bold))
                result |= System.Drawing.FontStyle.Bold;
            if (pSrc.HasFlag(BCFontStyle.Italic))
                result |= System.Drawing.FontStyle.Italic;
            if (pSrc.HasFlag(BCFontStyle.Underline))
                result |= System.Drawing.FontStyle.Underline;
            if (pSrc.HasFlag(BCFontStyle.Strikeout))
                result |= System.Drawing.FontStyle.Strikeout;

            return result;
        }
        public static BCFontStyle GetBCStyleFromGDIPlusStyle(FontStyle pSrc)
        {
            BCFontStyle result = BCFontStyle.Regular;
            if (pSrc.HasFlag(System.Drawing.FontStyle.Bold))
                result |= BCFontStyle.Bold;
            if (pSrc.HasFlag(System.Drawing.FontStyle.Italic))
                result |= BCFontStyle.Italic;
            if (pSrc.HasFlag(System.Drawing.FontStyle.Underline))
                result |= BCFontStyle.Underline;
            if (pSrc.HasFlag(System.Drawing.FontStyle.Strikeout))
                result |= BCFontStyle.Strikeout;
            
            return result;
        }
      

    }

    public class SKFontInfo
    {
        public SKTypeface TypeFace { get; set; }
        public float FontSize { get; set; }
        public SKFontInfo(SKFontInfo Source)
        {
            TypeFace = Source.TypeFace;
            FontSize = Source.FontSize;
        }
        public SKFontInfo(SKTypeface face,float pFontSize)
        {
            TypeFace = face;
            FontSize = FontSize;
        }
        public void ApplyPaint(SKPaint Target)
        {
            Target.Typeface = this.TypeFace;
            Target.TextSize = this.FontSize;
        }
    }

    public class BCColor
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;
        public BCColor(BCColor Source)
        {
            R = Source.R;
            G = Source.G;
            B = Source.B;
            A = Source.A;
        }
        public BCColor(byte pR,byte pG,byte pB,byte pA)
        {
            R = pR;
            G = pG;
            B = pB;
            A = pA;
        }
        //BCColor->SKColor
        public static implicit operator SKColor(BCColor pSource)
        {
            return new SKColor(pSource.R,pSource.G,pSource.B,pSource.A);
        }
        //SKColor -> BCColor
        public static implicit operator BCColor(SKColor pSource)
        {
            return new BCColor(pSource.Red,pSource.Green,pSource.Blue,pSource.Alpha);
        }
        //BCColor->Color
        public static implicit operator Color(BCColor pSource)
        {
            return Color.FromArgb(pSource.A,pSource.R,pSource.G,pSource.B);
        }
        //Color->BCColor
        public static implicit operator BCColor(Color pSource)
        {
            return new BCColor(pSource.R, pSource.G, pSource.B, pSource.A);
        }
        public static BCColor FromArgb(byte pA, byte pR,byte pG,byte pB)
        {
            return new BCColor(pR,pG,pB,pA);
        }
        public static BCColor Red = Color.Red;
        public static BCColor Orange = Color.Orange;
        public static BCColor Yellow = Color.Yellow;
        public static BCColor Green = Color.Green;
        public static BCColor Blue = Color.Blue;
        public static BCColor Indigo = Color.Indigo;
        public static BCColor Violet = Color.Violet;

    }
    

    //provider wrappers are classes that wrap elementary types and provide implicit conversions to other element types.
    public class BCPointF
    {
        public float X { get; set; }
        public float Y { get; set; }

        public BCPointF(BCPointF source)
        {
            X = source.X;
            Y = source.Y;
        }
        public BCPointF(float pX,float pY)
        {
            X = pX;
            Y = pY;
        }
        public static implicit operator PointF(BCPointF Source)
        {
            return new PointF(Source.X,Source.Y); 
        }
        public static implicit operator BCPointF(PointF Source)
        {
            return new BCPointF(Source.X, Source.Y);
        }

        public static implicit operator SKPoint(BCPointF Source)
        {
            return new SKPoint(Source.X,Source.Y);
        }
        public static implicit operator BCPointF(SKPoint Source)
        {
            return new BCPointF(Source.X,Source.Y);
        }
        
    }
    

}
