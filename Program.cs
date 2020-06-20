using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Forms;
using BASeCamp.BASeScores;
using BASeCamp.Logging;
using BASeTris.AI;
using OpenTK;

namespace BASeTris
{
    static class Program
    {
        public enum StartMode
        {
            Mode_WinForms,
            Mode_OpenTK
        }
        public static StartMode RunMode = StartMode.Mode_WinForms;
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
                var testresult = SNESTetrominoTheme.GetUnsetImage(0, 'O');
                SkiaSharp.Views.Desktop.Extensions.ToBitmap(testresult).Save("T:\\O_SNES_UNSET.PNG");
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
                return;
                new BASeTrisTK().Run();
            }
        }
    }
}