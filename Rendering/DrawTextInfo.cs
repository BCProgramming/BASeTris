using BASeTris.Rendering.Adapters;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering
{
    public abstract class DrawTextInformation
    {
        public String Text;
        public float ScalePercentage = 1;
        public DrawTextInformation PreDrawDataBase = null; //delegate to this one first if present.
        public DrawTextInformation PostDrawDataBase = null; //delegate to this one after if present.
    }
    public class DrawTextInformationGDI : DrawTextInformation
    {

        public Font DrawFont;

        public PointF Position;
        public Brush ForegroundBrush;
        public Brush BackgroundBrush;
        public Brush ShadowBrush;
        public PointF ShadowOffset = new PointF(5, 5);
        public StringFormat Format;
        public DrawCharacterHandlerGDI CharacterHandler = new DrawCharacterHandlerGDI();

        public DrawTextInformationGDI PreDrawData { get { return PreDrawDataBase as DrawTextInformationGDI; } set { PreDrawDataBase = value; } }

        public DrawTextInformationGDI PostDrawData { get { return PostDrawDataBase as DrawTextInformationGDI; } set { PostDrawDataBase = value; } }

        //Graphics g,Font UseFont,
        //String sText, Brush ForegroundBrush,
        //Brush ShadowBrush, float XPosition, float YPosition,
        //float ShadowXOffset=5,float ShadowYOffset=5,StringFormat sf = null
    }
    public class DrawTextInformationSkia : DrawTextInformation
    {
        public DrawTextInformationSkia PreDrawData { get { return PreDrawDataBase as DrawTextInformationSkia; } set { PreDrawDataBase = value; } }

        public DrawTextInformationSkia PostDrawData { get { return PostDrawDataBase as DrawTextInformationSkia; } set { PostDrawDataBase = value; } }
        public SKPaint ForegroundPaint;
        public SKPaint BackgroundPaint;
        public SKPaint ShadowPaint;
        public SKPoint Position;
        public SKPoint ShadowOffset = new SKPoint(5, 5);
        public SKFontInfo DrawFont;
        public DrawCharacterHandlerSkia CharacterHandler = new DrawCharacterHandlerSkia();

    }

    public abstract class DrawCharacterHandler<PositionCalcType, CanvasType, InfoType, PosType, SizeType> where PositionCalcType : DrawCharacterPositionCalculator<CanvasType, PosType, SizeType, InfoType>, new()
    {
        protected IList<PositionCalcType> _Extensions = new List<PositionCalcType>() { new PositionCalcType() };
        public void SetPositionCalculator(PositionCalcType calc)
        {
            _Extensions = new List<PositionCalcType>() { calc };
        }
        public abstract void DrawCharacter(CanvasType g, char character, InfoType DrawData, PosType Position, SizeType CharacterSize, int CharacterNumber, int TotalCharacters, int Pass);
    }
    public class DrawCharacterHandlerGDI : DrawCharacterHandler<DrawCharacterPositionCalculatorGDI, Graphics, DrawTextInformationGDI, PointF, SizeF>
    {

        public override void DrawCharacter(Graphics g, char character, DrawTextInformationGDI DrawData, PointF Position, SizeF CharacterSize, int CharacterNumber, int TotalCharacters, int Pass)
        {
            if (DrawData.PreDrawData != null)
            {
                DrawCharacter(g, character, DrawData.PreDrawData, Position, CharacterSize, CharacterNumber, TotalCharacters, Pass);
            }
            //adjust positioning by calling each extension.
            foreach (var iterate in _Extensions)
            {
                iterate.AdjustPositioning(ref Position, CharacterSize, DrawData, CharacterNumber, TotalCharacters, Pass);
            }
            PointF AddedOffset = Pass == 1 ? PointF.Empty : DrawData.ShadowOffset;
            Brush DrawBrush = Pass == 1 ? DrawData.ShadowBrush : DrawData.ForegroundBrush;
            //call beforedraw...
            foreach (var iterate in _Extensions)
            {
                iterate.BeforeDraw(g, character, DrawData, Position, CharacterSize, CharacterNumber, TotalCharacters, Pass);
            }
            Font UseFont = DrawData.DrawFont;
            PointF DrawPosition = new PointF(Position.X + AddedOffset.X, Position.Y + AddedOffset.Y);
            if (DrawData.ScalePercentage != 1)
            {
                UseFont = new Font(DrawData.DrawFont.FontFamily, DrawData.DrawFont.Size * DrawData.ScalePercentage, DrawData.DrawFont.Style);
                float NewWidth = CharacterSize.Width * DrawData.ScalePercentage;
                float NewHeight = CharacterSize.Height * DrawData.ScalePercentage;
                AddedOffset.X -= ((NewWidth - CharacterSize.Width)) / 2;
                AddedOffset.Y -= ((NewHeight - CharacterSize.Height)) / 2;
            }


            g.DrawString(character.ToString(), UseFont, DrawBrush, DrawPosition.X, DrawPosition.Y, DrawData.Format);
            foreach (var iterate in _Extensions.Reverse())
            {
                iterate.AfterDraw(g, character, DrawData, Position, CharacterSize, CharacterNumber, TotalCharacters, Pass);
            }
            if (DrawData.PostDrawData != null)
            {
                DrawCharacter(g, character, DrawData.PostDrawData, Position, CharacterSize, CharacterNumber, TotalCharacters, Pass);
            }
        }
    }
    public class DrawCharacterHandlerSkia : DrawCharacterHandler<DrawCharacterPositionCalculatorSkia, SKCanvas, DrawTextInformationSkia, SKPoint, SKPoint>
    {

        public override void DrawCharacter(SKCanvas g, char character, DrawTextInformationSkia DrawData, SKPoint Position, SKPoint CharacterSize, int CharacterNumber, int TotalCharacters, int Pass)
        {
            if (DrawData.PreDrawData != null)
            {
                DrawCharacter(g, character, DrawData.PreDrawData, Position, CharacterSize, CharacterNumber, TotalCharacters, Pass);
            }
            //adjust positioning by calling each extension.
            foreach (var iterate in _Extensions)
            {
                iterate.AdjustPositioning(ref Position, CharacterSize, DrawData, CharacterNumber, TotalCharacters, Pass);
            }
            SKPoint AddedOffset = Pass == 1 ? SKPoint.Empty : DrawData.ShadowOffset;
            SKPaint DrawBrush = Pass == 1 ? DrawData.ShadowPaint: DrawData.ForegroundPaint;
            //call beforedraw...
            foreach (var iterate in _Extensions)
            {
                iterate.BeforeDraw(g, character, DrawData, Position, CharacterSize, CharacterNumber, TotalCharacters, Pass);
            }
            SKFontInfo UseFont = new SKFontInfo(DrawData.DrawFont);
            
            SKPoint DrawPosition = new SKPoint(Position.X + AddedOffset.X, Position.Y + AddedOffset.Y);
            if (DrawData.ScalePercentage != 1)
            {
                UseFont.FontSize = UseFont.FontSize * DrawData.ScalePercentage;
                float NewWidth = CharacterSize.X * DrawData.ScalePercentage;
                float NewHeight = CharacterSize.Y * DrawData.ScalePercentage;
                AddedOffset.X -= ((NewWidth - CharacterSize.X)) / 2;
                AddedOffset.Y -= ((NewHeight - CharacterSize.Y)) / 2;
            }
            g.DrawText(character.ToString(), DrawPosition, DrawBrush);

            foreach (var iterate in _Extensions.Reverse())
            {
                iterate.AfterDraw(g, character, DrawData, Position, CharacterSize, CharacterNumber, TotalCharacters, Pass);
            }
            if (DrawData.PostDrawData != null)
            {
                DrawCharacter(g, character, DrawData.PostDrawData, Position, CharacterSize, CharacterNumber, TotalCharacters, Pass);
            }
        }
    }


    public abstract class DrawCharacterPositionCalculator<CanvasType, PosType, SizeType, InfoType>
    {
        public abstract void AdjustPositioning(ref PosType Position, SizeType size, InfoType DrawData, int pCharacterNumber, int TotalCharacters, int Pass);
        public abstract void BeforeDraw(CanvasType g, char Character, InfoType DrawData, PosType Position, SizeType CharacterSize, int CharacterNumber, int TotalCharacters, int Pass);
        public abstract void AfterDraw(CanvasType g, char character, InfoType DrawData, PosType Position, SizeType CharacterSize, int CharacterNumber, int TotalCharacters, int Pass);
    }
    public class DrawCharacterPositionCalculatorGDI : DrawCharacterPositionCalculator<Graphics, PointF, SizeF, DrawTextInformationGDI>
    {
        public override void AdjustPositioning(ref PointF Position, SizeF size, DrawTextInformationGDI DrawData, int pCharacterNumber, int TotalCharacters, int Pass)
        {
            //default makes no changes.
        }
        public override void BeforeDraw(Graphics g, char character, DrawTextInformationGDI DrawData, PointF Position, SizeF CharacterSize, int CharacterNumber, int TotalCharacters, int Pass)
        {

        }
        public override void AfterDraw(Graphics g, char character, DrawTextInformationGDI DrawData, PointF Position, SizeF CharacterSize, int CharacterNumber, int TotalCharacters, int Pass)
        {

        }

    }
    public class DrawCharacterPositionCalculatorSkia: DrawCharacterPositionCalculator<SKCanvas, SKPoint, SKPoint, DrawTextInformationSkia>
    {
        public override void AdjustPositioning(ref SKPoint Position, SKPoint size, DrawTextInformationSkia DrawData, int pCharacterNumber, int TotalCharacters, int Pass)
        {
            //default makes no changes.
        }
        /*public void AdjustPositioning(ref SKPoint Position,SKPoint size,DrawTextInformationSkia DrawData,int pCharacterNumber,int TotalCharacters,int Pass)
        {
            PointF usePosition = SkiaSharp.Views.Desktop.Extensions.ToDrawingPoint(Position);
            PointF tempSize = SkiaSharp.Views.Desktop.Extensions.ToDrawingPoint(size);
            SizeF sendsize = new SizeF(tempSize.Width, tempSize.Height);
            AdjustPositioning(ref usePosition, sendsize, DrawData, pCharacterNumber, TotalCharacters, Pass);
        }*/
        public override void BeforeDraw(SKCanvas g, char character, DrawTextInformationSkia DrawData, SKPoint Position, SKPoint CharacterSize, int CharacterNumber, int TotalCharacters, int Pass)
        {

        }
        public override void AfterDraw(SKCanvas g, char character, DrawTextInformationSkia DrawData, SKPoint Position, SKPoint CharacterSize, int CharacterNumber, int TotalCharacters, int Pass)
        {

        }

    }
    public class NullCharacterPositionCalculatorGDI: DrawCharacterPositionCalculatorGDI
    {

    }
    public class NullCharacterPositionCalculatorSkia: DrawCharacterPositionCalculatorSkia
    {

    }
    public class RotatingPositionCharacterPositionCalculatorGDI : DrawCharacterPositionCalculatorGDI
    {
        private float Radius = 10;
        private float CharacterNumberModifier = 0.5f;
        public override sealed void AdjustPositioning(ref PointF Position, SizeF size, DrawTextInformationGDI DrawData, int pCharacterNumber, int TotalCharacters, int Pass)
        {
            //rotate once every 3/4's of a second.
            float XPos = Position.X, YPos = Position.Y;
            StandardPositionCalculators.RotatingPositionCalculator(ref XPos, ref YPos, size.Width, size.Height, pCharacterNumber, TotalCharacters, Pass, CharacterNumberModifier, Radius);
            Position.X = XPos;
            Position.Y = YPos;
        }
    }
    public static class StandardPositionCalculators
    {
        public static void RotatingPositionCalculator(ref float XPos, ref float YPos, float Width, float Height, int pCharacterNumber, int TotalCharacters, int Pass, float CharacterNumberModifier = 0.5f, float Radius = 10)
        {
            var rotationpercentage = (DateTime.Now.TimeOfDay.TotalMilliseconds % 750) / 750;
            var addedpercentage = (float)pCharacterNumber / (float)TotalCharacters;
            double Angle = rotationpercentage * 2 * Math.PI + (addedpercentage * CharacterNumberModifier * Math.PI);
            float NewXPos = (float)Math.Cos(Angle) * Radius;
            float NewYPos = (float)Math.Sin(Angle) * Radius;
            XPos = XPos + NewXPos;
            YPos = YPos + NewYPos;

        }
    }
}
