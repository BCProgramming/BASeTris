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

    
    public struct BCPoint
    {
        private SKPoint Main;
        public float X {  get { return Main.X; } set { Main.X = value; } }
        public float Y {  get { return Main.Y; }  set { Main.Y = value; } }
        public BCPoint(SKPoint Source)
        {
            Main = Source;
        }
        public BCPoint(float X, float Y):this(new SKPoint(X,Y))
        {

        }
        public BCPoint(PointF Source):this(Source.X,Source.Y)
        {

        }
        public static implicit operator SKPoint(BCPoint src)
        {
            return src.Main;
        }
        public static implicit operator BCPoint(SKPoint src)
        {
            return new BCPoint(src);
        }
        public static implicit operator BCPoint(PointF src)
        {
            return new BCPoint(src);
        }
        public static implicit operator PointF(BCPoint src)
        {
            return new PointF(src.X, src.Y);
        }
        public static implicit operator Point(BCPoint src)
        {
            return new Point((int)src.X, (int)src.Y);
        }
        public static BCPoint operator +(BCPoint first,BCPoint other)
        {
            return new BCPoint(first.X + other.X, first.Y + other.Y);
        }
        public static BCPoint operator -(BCPoint first,BCPoint other)
        {
            return new BCPoint(first.X - other.X, first.Y - other.Y);
        }
        public static BCPoint operator *(BCPoint first,float other)
        {
            return new BCPoint(first.X * other, first.Y * other);
        }
        public static BCPoint operator *(BCPoint first, BCPoint other)
        {
            return new BCPoint(first.X * other.X, first.Y * other.Y);
        }

    }
    public struct BCRect
    {
        private SKRect Main;
        public BCRect(SKRect Source)
        {
            Main = Source;
        }
        public BCRect(RectangleF Source):this(new SKRect(Source.Left,Source.Top,Source.Right,Source.Bottom))
        {

        }
        public BCRect(float pLeft,float pTop, float pWidth,float pHeight):this(new SKRect(pLeft,pTop,pLeft+pWidth,pTop+pHeight))
        {

        }
        public float Left {  get { return Main.Left; } set { Main.Left = value; } }
        public float Top { get { return Main.Top; } set { Main.Top = value; } }
        public float Width {  get { return Main.Width; } set { Main.Right = Main.Left+value; } }
        public float Right { get { return Main.Right; } set { Main.Right = value; } }
        public float Bottom { get { return Main.Bottom; } set { Main.Bottom = value; } }
        public float Height { get { return Main.Height; } set { Main.Bottom = Main.Top + value; } }
        public static implicit operator SKRect(BCRect src)
        {
            return src.Main;
        }
        public static implicit operator RectangleF(BCRect src)
        {
            return new RectangleF(src.Left, src.Top, src.Width, src.Height);
        }
        public static implicit operator BCRect(SKRect src)
        {
            return new BCRect(src);
        }
        public static implicit operator BCRect(RectangleF src)
        {
            return new BCRect(src);
        }
    }


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
            FontSize = pFontSize;
        }
        public void ApplyPaint(SKPaint Target)
        {
            Target.Typeface = this.TypeFace;
            Target.TextSize = this.FontSize;
        }
    }

    public struct BCColor
    {
        private SKColor Main;
        public byte R { get { return Main.Red; } }
        public byte G { get { return Main.Green; } }
        public byte B { get { return Main.Blue; } }
        public byte A { get { return Main.Alpha; } }
        public BCColor(SKColor src)
        {
            Main = src;
        }
        public BCColor(byte pR, byte pG, byte pB, byte pA) : this(new SKColor(pR, pG, pB, pA))
        {

        }
        public BCColor(byte pR, byte pG, byte pB) : this(pR, pG, pB, 255)
        {

        }
        public static implicit operator Color(BCColor src)
        {
            return Color.FromArgb(src.A, src.R, src.G, src.B);
        }
        public static implicit operator BCColor(Color src)
        {
            return new BCColor(src.R, src.G, src.B, src.A);
        }
        public static implicit operator SKColor(BCColor src)
        {
            return src.Main;
        }
        public static implicit operator BCColor(SKColor src)
        {
            return new BCColor(src);
        }
        public static BCColor Red = new BCColor(SKColors.Red);
        public static BCColor Orange = new BCColor(SKColors.Orange);
        public static BCColor Yellow = new BCColor(SKColors.Yellow);
        public static BCColor Green = new BCColor(SKColors.Green);
        public static BCColor Blue = new BCColor(SKColors.Blue);
        public static BCColor Indigo= new BCColor(SKColors.Indigo);
        public static BCColor Violet= new BCColor(SKColors.Violet);
        public static BCColor White = new BCColor(SKColors.White);
        public static BCColor Gray = new BCColor(SKColors.Gray);
        public static BCColor Black = new BCColor(SKColors.Black);
        public static BCColor DarkSlateGray = new BCColor(SKColors.DarkSlateGray);
        public static BCColor Transparent = new BCColor(SKColors.Transparent);
        public override string ToString()
        {
            return Main.ToString();
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
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

    public class BCImage
    {
        private Image GDIImage = null;
        private SKImage SkiaImage = null;

        public BCImage(Image src)
        {
            GDIImage = src;
            SkiaImage = SkiaSharp.Views.Desktop.Extensions.ToSKImage(new Bitmap(GDIImage));
        }
        public BCImage (SKImage src)
        {
            SkiaImage = src;
            GDIImage = SkiaSharp.Views.Desktop.Extensions.ToBitmap(SkiaImage);
        }
        public static implicit operator SKImage(BCImage src)
        {
            return src.SkiaImage;
        }
        public static implicit operator BCImage(SKImage src)
        {
            return new BCImage(src);
        }
        public static implicit operator Image(BCImage src)
        {
            return src.GDIImage;
        }
        public static implicit operator BCImage(Image src)
        {
            return new BCImage(src);
        }
    }
    

}
