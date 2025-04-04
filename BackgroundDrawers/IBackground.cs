﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.BackgroundDrawers;
using SkiaSharp;

namespace BASeTris.BackgroundDrawers
{


    //base class for all Background Draw Data.
    //note that this is part of the Background interface definition. This 
    //is intended to be used for storing any Graphics API specific information. if needed, with subclasses for the appropriate
    //types being implemented as needed for each possible renderer.
    public abstract class BackgroundDrawData
    {

    }
    public class SkiaBackgroundDrawData :BackgroundDrawData
    {
        public SKRect Bounds;
        public SkiaBackgroundDrawData(SKRect pBounds)
        {
            Bounds = pBounds;
        }
    }
    public class GDIBackgroundDrawData : BackgroundDrawData
    {
        public RectangleF Bounds;
        public GDIBackgroundDrawData(RectangleF pBounds)
        {
            Bounds = pBounds;
        }
    }
    public class NullBackgroundDrawData : BackgroundDrawData
    {

    }

    public abstract class Background<T> : Background, IBackground<T> where T:BackgroundDrawData,new()
    {
        //public abstract void DrawProc(Graphics g, RectangleF Bounds);

        public new Background<T> Underlayer { get { return (Background<T>)base.Underlayer; } set { base.Underlayer = value; } }

        public new Background<T> Overlayer { get { return (Background<T>)base.Overlayer; } set { base.Overlayer = value; } }

        //public override Background<T> Underlayer { get; set; } = null;
        //public override Background<T> Overlayer { get; set; } = null; //a composite background: we will draw this one right after drawing this one.
        public T Data { get; set; }
    }
    public abstract class Background : IBackground
    {
        public Background Underlayer { get; set; }

        public Background Overlayer { get; set; }
        IBackground IBackground.Underlayer { get => this.Underlayer; set => this.Underlayer= (Background)value; }
        IBackground IBackground.Overlayer { get => this.Overlayer; set => this.Overlayer = (Background)value; }

        public abstract void FrameProc(IStateOwner pState);

       
    }
    public interface IBackground<T> : IBackground where T:BackgroundDrawData
    {

        T Data { get; set; }
        /// <summary>
        /// Routine called to perform the drawing task. 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        //void DrawProc(Graphics g, RectangleF Bounds);
        //drawing is handled by rendering provider now...

        /// <summary>
        /// Called each Game "tick" to allow the background implementation to perform any necessary state changes.
        /// </summary>


    }
    public interface IBackground
    {
        IBackground Underlayer { get; set; }
        IBackground Overlayer { get; set; }
        void FrameProc(IStateOwner pState);
    }



    public class BackgroundInformationAttribute : Attribute
    {
        public Type CanvasType { get; set; }
        public String StyleName { get; set; }
        public BackgroundInformationAttribute(Type pCanvasType,String pStyleName)
        {
            CanvasType = pCanvasType;
            StyleName = pStyleName;
        }
        public static String Style_Standard = "STANDARD";
    }
}