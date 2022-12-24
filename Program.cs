using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Reflection.BASeCamp;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using BASeCamp.BASeScores;
using BASeCamp.Logging;
using BASeTris.AI;
using BASeTris.GameStates.GameHandlers;
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

        public static System.Reflection.BASeCamp.MultiTypeManager DITypes = null;
        public static Type[] LoadTypes = new Type[] { typeof(NominoTheme), typeof(IGameCustomizationHandler) };

        public enum StartMode
        {
            Mode_WinForms,
            Mode_OpenTK
        }
        public static StartMode RunMode = StartMode.Mode_OpenTK;

        private static Dictionary<Type, Type[]> CacheHandlerTheme = new Dictionary<Type, Type[]>();
        public static IEnumerable<Type> GetHandlerThemes(Type HandlerType)
        {
            if(CacheHandlerTheme.ContainsKey(HandlerType))
            {
                return CacheHandlerTheme[HandlerType];
            }

            var TheTypes = DITypes[typeof(NominoTheme)].GetManagedTypes();
            List<Type> ConstructType = new List<Type>();
            foreach(var iteratetype in TheTypes)
            {
                var attrib = (HandlerThemeAttribute)iteratetype.GetCustomAttribute(typeof(HandlerThemeAttribute));
                if(attrib!=null)
                {
                    if(attrib.HandlerType.Contains(HandlerType))
                    {
                        ConstructType.Add(iteratetype);
                    }
                }
            }
            CacheHandlerTheme.Add(HandlerType, ConstructType.ToArray());
            return CacheHandlerTheme[HandlerType];

        }
        public static IEnumerable<Type> GetGameHandlers()
        {
            return DITypes[typeof(IGameCustomizationHandler)].ManagedTypes;
        }
        const double Phi = 1.618033988749895;
        const double phi = -1 /Phi;
        public static double Fibonacci(int n)
        {
            return Math.Round((Math.Pow(Phi, n) - Math.Pow(phi, n)) / Math.Sqrt(5),2);
        }
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
            DebugLogger.EnableLogging = true;
            Debug.Print("Started");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DebugLogger.EnableLogging = true;
            iManagerCallback cback = new LoggingCallback();
            
            DITypes = new System.Reflection.BASeCamp.MultiTypeManager(
                new Assembly[] { Assembly.GetExecutingAssembly() }.AsEnumerable(),
                LoadTypes,
                cback,
                null,
                null,
                null);


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

                var DesiredHeight = (int)(((returnedvalue.Bottom - returnedvalue.Top)- 64)*.80f);
                var DesiredWidth = (int)((float)DesiredHeight * .95f);


                Debug.Print($"Starting BTTK: {DesiredWidth},{DesiredHeight}");
                new BASeTrisTK(DesiredWidth,DesiredHeight).Run();
            }
        }
    }
}