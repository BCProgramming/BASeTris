using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris
{
    public static class GraphicsPathExtensions
    {
        public static void AddString(this GraphicsPath gp, String addme, Font usefont, PointF origin, StringFormat sformat)
        {
            gp.AddString(addme, usefont.FontFamily, (int)usefont.Style, usefont.Size, origin, sformat);


        }
        public static void AddString(this GraphicsPath gp, String addme, Font usefont, Point origin, StringFormat sformat)
        {
            gp.AddString(addme, usefont.FontFamily, (int)usefont.Style, usefont.Size, origin, sformat);


        }

    }
    public static class RandomExtensions
    {
        public static double NextDouble(this Random r, double Minimum, double Maximum)
        {
            return (r.NextDouble() * (Maximum - Minimum)) + Minimum;



        }


    }
    public static class ListExtensions
    {

        public static Queue<T> Clone<T>(this Queue<T> QueueToClone) where T : ICloneable
        {
            return new Queue<T>(QueueToClone.Select(item => (T)item.Clone()));



        }
        public static List<T> Clone<T>(this IEnumerable<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
        public static List<T> ShallowClone<T>(this IEnumerable<T> listtoclone)
        {
            lock (listtoclone)
            {
                return new List<T>(listtoclone);
            }

        }
        public static LinkedList<T> Clone<T>(this LinkedList<T> listtoClone) where T : ICloneable
        {
            return new LinkedList<T>(listtoClone.Select(item => (T)item.Clone()));
        }
        public static LinkedList<T> ShallowClone<T>(this LinkedList<T> listtoClone)
        {
            return new LinkedList<T>(listtoClone);

        }

    }
    public static class ImageExtensions
    {

        public static Image ClipImage(this Image ourimage, Point TopLeft, Size ClipSize)
        {
            Bitmap newimage = new Bitmap(ClipSize.Width, ClipSize.Height);

            using (Graphics drawcanvas = Graphics.FromImage(newimage))
            {

                drawcanvas.DrawImage(ourimage, 0, 0, new Rectangle(TopLeft, ClipSize), GraphicsUnit.Pixel);


            }


            return newimage;




        }


    }
}
