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
                new BASeTrisTK().Run();
            }
        }
    }
}