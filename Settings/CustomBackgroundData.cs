using BASeTris.GameStates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace BASeTris.Settings
{
    public class CustomBackgroundData
    {
        public static String sCustomBackgroundFolder = Path.Combine(TetrisGame.AppDataFolder, "UserBackgrounds");
        //basically, we store 10 Backgrounds.
        static DesignBackgroundState[] GetCustomBackgrounds()
        {
            
            DesignBackgroundState[] Result = new DesignBackgroundState[10];
            for (int i = 1; i < 10; i++)
            {
                
                try
                {
                    var buildresult = LoadCustomBackground(i);
                    Result[i] = buildresult;
                }
                catch (Exception exr)
                {
                    ;
                }



            }
            return Result;


        }
        public static DesignBackgroundState LoadCustomBackground(int slot)
        {
            String sFindFile = Path.Combine(sCustomBackgroundFolder, slot.ToString() + ".dat");
            try
            {
                if (File.Exists(sFindFile))
                {
                    XDocument xdoc = XDocument.Load(sFindFile);
                    var buildresult = new DesignBackgroundState(xdoc.Root, null);
                    return buildresult;
                }
            }
            catch (Exception exr)
            {
                ;
            }
            return null;
        }
        public static void SaveCustomBackground(DesignBackgroundState source,int slot)
        {
            XDocument doc = new XDocument(source.GetXmlData("Background", null));
            String sFindFile = Path.Combine(sCustomBackgroundFolder, slot.ToString() + ".dat");
            String sPath = Path.GetDirectoryName(sFindFile);
            if (!Directory.Exists(sPath)) Directory.CreateDirectory(sPath);
            doc.Save(sFindFile);
        }
        public static DateTime? GetCustomBackgroundTouched(int i)
        {
            String sFindFile = Path.Combine(sCustomBackgroundFolder, i.ToString() + ".dat");

            if (File.Exists(sFindFile)) return File.GetLastWriteTime(sFindFile);

            return null;



        }

    }
}
