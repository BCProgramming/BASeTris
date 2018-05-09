using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BaseDoku;
using BASeCamp.BASeScores;

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
          
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DebugLogger.EnableLogging = true;
            Application.Run(new BASeTris());
        }




    }
}
