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

namespace BASeTris
{
    static class Program
    {
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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DebugLogger.EnableLogging = true;
            Application.Run(new BASeTris());
        }
    }
}