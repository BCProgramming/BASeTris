using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using BASeCamp.BASeScores;
using BASeCamp.Logging;
using BASeTris.AI;
using OpenTK;
using OpenTK.Input;

namespace BASeTris
{
    static class Program
    {

        private const int SPI_GETWORKAREA = 48;
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfoA")]
        private static extern int SystemParametersInfo(int uAction, IntPtr uParam, ref RECT lpvParam, int fuWinIni);
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            internal int Left;
            internal int Top;
            internal int Right;
            internal int Bottom;
        }



        public enum StartMode
        {
            Mode_WinForms,
            Mode_OpenTK
        }
        public static StartMode RunMode = StartMode.Mode_OpenTK;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

           
            /* foreach(var iterate in NominoBuilder.BuildNominoes(5))
             {
                 var buildstring = NominoBuilder.NominoToString(iterate);


             }*/
            //DebugLogger.EnableLogging = true;
            //var testresult = SimpleAIEvolver.RunSimulation(new StoredBoardState.AIScoringRuleData());
            //SimpleAIEvolver.RunSimulations();
            //return;
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DebugLogger.EnableLogging = true;
            if (RunMode == StartMode.Mode_WinForms)
            {
             
                Application.Run(new BASeTris());
            }
            else
            {
                /*var testresult = SNESTetrominoTheme.GetUnsetImage(0, 'T');
                SkiaSharp.Views.Desktop.Extensions.ToBitmap(testresult).Save("T:\\T_SNES_UNSET.PNG");
                var unsetTest = SNESTetrominoTheme.GetUnsetImage(0, 'Z');
                SkiaSharp.Views.Desktop.Extensions.ToBitmap(unsetTest).Save("T:\\Z_SNES_UNSET.PNG");
                var unsetTestS = SNESTetrominoTheme.GetUnsetImage(0, 'S');
                SkiaSharp.Views.Desktop.Extensions.ToBitmap(unsetTestS).Save("T:\\S_SNES_UNSET.PNG");
                var testresult2 = SNESTetrominoTheme.GetUnsetImage(6, 'I');
                SkiaSharp.Views.Desktop.Extensions.ToBitmap(testresult2).Save("T:\\I_UNSET_SNES.PNG");*/
                //return;
                RECT returnedvalue = new RECT();
                SystemParametersInfo(SPI_GETWORKAREA, IntPtr.Zero, ref returnedvalue, 0);

                var DesiredHeight = (returnedvalue.Bottom - returnedvalue.Top)- 64;
                var DesiredWidth = (int)((float)DesiredHeight * .95f);



                new BASeTrisTK(DesiredWidth,DesiredHeight).Run();
            }
        }
    }
}