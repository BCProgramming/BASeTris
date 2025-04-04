using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Reflection.BASeCamp;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BASeCamp.BASeScores;
using BASeCamp.Logging;
using BASeTris.AI;
using BASeTris.Blocks;
using BASeTris.Choosers;
using BASeTris.GameStates.GameHandlers;
using BASeTris.Tetrominoes;
using Microsoft.VisualBasic;
using OpenTK;
using OpenTK.Input;
using static BASeTris.NNominoGenerator;

namespace BASeTris
{
    static partial class Program
    {

        private const int SPI_GETWORKAREA = 48;
        [LibraryImport("user32.dll", EntryPoint = "SystemParametersInfoA")]
        private static partial int SystemParametersInfo(int uAction, IntPtr uParam, ref RECT lpvParam, int fuWinIni);
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            internal int Left;
            internal int Top;
            internal int Right;
            internal int Bottom;
        }

        public static System.Reflection.BASeCamp.MultiTypeManager DITypes = null;
        public static Type[] LoadTypes = new Type[] { typeof(NominoTheme), typeof(IBlockGameCustomizationHandler), typeof(BlockGroupChooser) };

        public enum StartMode
        {
            Mode_WinForms,
            Mode_OpenTK
        }
        public static StartMode RunMode = StartMode.Mode_OpenTK;

        private static Dictionary<Type, Type[]> CacheHandlerTheme = new Dictionary<Type, Type[]>();
        public static IEnumerable<Type> GetHandlerThemes(Type HandlerType)
        {
            if (CacheHandlerTheme.ContainsKey(HandlerType))
            {
                return CacheHandlerTheme[HandlerType];
            }

            var TheTypes = DITypes[typeof(NominoTheme)].GetManagedTypes();
            List<Type> ConstructType = new List<Type>();
            foreach (var iteratetype in TheTypes)
            {
                var attrib = (HandlerThemeAttribute)iteratetype.GetCustomAttribute(typeof(HandlerThemeAttribute));
                if (attrib != null)
                {
                    if (!attrib.Flags.HasFlag(HandlerThemeAttribute.HandlerThemeFlags.ThemeFlags_NonBrowsable) && attrib.HandlerType.Any((t) => t.IsAssignableFrom(HandlerType)))

                    //if(attrib.HandlerType.Contains(HandlerType))
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
            return DITypes[typeof(IBlockGameCustomizationHandler)].ManagedTypes;
        }
        const double Phi = 1.618033988749895;
        const double phi = -1 / Phi;
        public static double Fibonacci(int n)
        {
            return Math.Round((Math.Pow(Phi, n) - Math.Pow(phi, n)) / Math.Sqrt(5), 2);
        }
        
        private static void MultiMinoTest()
        {
           
                var result =  NNominoGenerator.FilterPieces(NNominoGenerator.GetPieces(8,NominoPieceGenerationFlags.Flag_Randomize,  null)).ToList();
            //var result = NNominoGenerator.FilterPieces(firstresult).ToList();
            StringBuilder sbreport = new StringBuilder();
            int CurrentCount = 1;
           
                using (StreamWriter sw = new StreamWriter(new FileStream("T:\\NominoOut.txt", FileMode.Create)))
                {
                foreach (var iterate in result)
                    {
                        

                        String str = NNominoGenerator.GetDirectionString(iterate);
                        String strrep = NNominoGenerator.StringRepresentation(iterate);
                           sw.WriteLine("N-omino " + CurrentCount);
                           sw.WriteLine("");
                           sw.WriteLine(strrep);
                    if (strrep.StartsWith(HolePiece))
                    {
                        ;
                    }
                           var isHole = NNominoGenerator.IsHolePiece(iterate);
                            sw.WriteLine("isHole:" + isHole);
                    if (isHole)
                    {
                        ;
                    }
                        CurrentCount++;

                    }
                }
            
            //String strresult = sbreport.ToString();
            //System.IO.File.WriteAllText("T:\\NominoOut.txt", strresult);
        }
        private static String HolePiece = @"###
# #
###";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //TestAI();
            //return;
            //Nomino MakeTester = new Tetromino_Y((a) => new LineSeriesBlock());

            //var testresult = MakeTester.GetContiguousSets();
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
                //MultiMinoTest();
                //return;
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

                var DesiredHeight = (int)(((returnedvalue.Bottom - returnedvalue.Top) - 64) * .80f);
                var DesiredWidth = (int)((float)DesiredHeight * .95f);
                //DesiredHeight = 1557;
                //DesiredWidth = 1672;
                //1557,1672
                Debug.Print($"Starting BTTK: {DesiredWidth},{DesiredHeight}");
                new BASeTrisTK((int)(DesiredWidth*1.5f), (int)(DesiredHeight*1.5f)).Run();
            }
        }

        private static NominoBlock bb()
        {
            return new StandardColouredBlock();
        }
        static void TestAI()
        {
            //TODO: this sort of testing stuff should be part of some sort of unit tests, I'd say, certain layouts have to score worse, after all, and we want to verify that remains true even after changing the AI Scoring rules.
            NominoBlock[][] TState = new NominoBlock[][] {
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,bb(),null,null,null,null },
                new NominoBlock[]{null,null,null,bb(),bb(),bb(),bb(),null,null,null },
                new NominoBlock[]{null,null,null,bb(),bb(),bb(),bb(),null,bb(),bb() },
                new NominoBlock[]{null,null,null,bb(),bb(),bb(),bb(),bb(),bb(),bb() },
                new NominoBlock[]{bb(),null,null,bb(),bb(),bb(),bb(),null,bb(),bb() },
                new NominoBlock[]{bb(),bb(),null,bb(), bb(),bb(),bb(),bb(),bb(),bb() },
                new NominoBlock[]{bb(),bb(),null,bb(),bb(),bb(),bb(),null,bb(),bb() }


            };



            //use StandardNominoAI to evaluate a known board state.
            NominoBlock[][] WellState = new NominoBlock[][] {
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,bb(),null,null,bb(),null,null,null,null },
                new NominoBlock[]{null,null,bb(),bb(),bb(),bb(),bb(),null,null,null },
                new NominoBlock[]{null,null,bb(),bb(),bb(),bb(),bb(),null,bb(),bb() },
                new NominoBlock[]{null,null,bb(),bb(),bb(),bb(),bb(),null,bb(),bb() },
                new NominoBlock[]{bb(),null,bb(),bb(),bb(),bb(),bb(),null,bb(),bb() },
                new NominoBlock[]{bb(),bb(),bb(),bb(), bb(),bb(),bb(),null,bb(),bb() },
                new NominoBlock[]{bb(),bb(),null,bb(),bb(),bb(),bb(),null,bb(),bb() }


            };

            NominoBlock[][] BadIPlacement = new NominoBlock[][] {
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,bb() },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,bb() },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,bb() },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,bb() },
                new NominoBlock[]{null,null,bb(),null,null,null,null,null,null,bb() },
                new NominoBlock[]{null,null,bb(),null,null,null,null,null,null,bb() },
                new NominoBlock[]{null,bb(),bb(),null,bb(),null,null,null,null,bb() },
                new NominoBlock[]{null,bb(),bb(),null,bb(),null,null,null,null,bb() },
                new NominoBlock[]{bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb(),null },
                new NominoBlock[]{bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb(),null,bb() },
                new NominoBlock[]{null,bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb() },
                new NominoBlock[]{null,bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb() },
                new NominoBlock[]{null,bb(),bb(),null,bb(),bb(),bb(),bb(),bb(),bb() },
                new NominoBlock[]{null,bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb() },
                new NominoBlock[]{null,bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb() }


            };


            NominoBlock[][] BetterIPlacement = new NominoBlock[][] {
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{bb(),null,bb(),null,null,null,null,null,null,bb() },
                new NominoBlock[]{bb(),null,bb(),null,null,null,null,null,null,bb() },
                new NominoBlock[]{bb(),bb(),bb(),null,bb(),null,null,null,null,bb() },
                new NominoBlock[]{bb(),bb(),bb(),null,bb(),null,null,null,null,bb() },
                new NominoBlock[]{bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb(),null },
                new NominoBlock[]{bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb(),null,bb() },
                new NominoBlock[]{null,bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb() },
                new NominoBlock[]{null,bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb() },
                new NominoBlock[]{null,bb(),bb(),null,bb(),bb(),bb(),bb(),bb(),bb() },
                new NominoBlock[]{null,bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb() },
                new NominoBlock[]{null,bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb() }


            };




            /*
         #
         #
         #
         #
  #      #
  #      #
 ##  #   #
 ##  #   #
######### 
######## #
 #########
 #########
 ## ######
 #########
 #########
             
             */



            NominoBlock[][] BadPlacement2 = new NominoBlock[][] {
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,bb() },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,bb() },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,bb() },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,bb() },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,bb() },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,bb() },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,bb() },
                new NominoBlock[]{bb(),bb(),bb(),null,null,bb(),null,null,null,bb() },
                new NominoBlock[]{null,bb(),bb(),bb(),bb(),bb(),null,bb(),bb(),bb() },
                new NominoBlock[]{bb(),bb(),bb(),bb(),bb(),bb(),null,bb(),null,bb() },
                new NominoBlock[]{bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb(), bb(),null }


            };

             NominoBlock[][] BetterPlacement2 = new NominoBlock[][] {
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,null },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,bb() },
                new NominoBlock[]{null,null,null,null,null,null,null,null,null,bb() },
                new NominoBlock[]{null,null,null,null,null,null,bb(),null,null,bb() },
                new NominoBlock[]{bb(),bb(),bb(),null,null,bb(),bb(),null,null,bb() },
                new NominoBlock[]{null,bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb() },
                new NominoBlock[]{bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb(),null,bb() },
                new NominoBlock[]{bb(),bb(),bb(),bb(),bb(),bb(),bb(),bb(), bb(),null }


            };

            /* Bad Placement 2
         #
         #
         #
         #
         #
         #
         #
         #
###  #   #
 ##### ###
###### # #
######### 
             
             
             */

            /* Better Placement 2
         
         
         
         
         #
         #
         #
      #  #
###  ##  #
 #########
######## #
#########              
             
             */




            var IMino = new Tetromino_I();
            var TMino = new Tetromino_T();
            var TMino2 = new Tetromino_T();
            var LMino = new Tetromino_L();
            StoredBoardState.TetrisScoringRuleData scorer = new StoredBoardState.TetrisScoringRuleData();

            //BetterIPlacement should give a better score.

            var BadPlacement = new StoredBoardState(BadPlacement2, null, 0, 0);
            var BetterPlacement = new StoredBoardState(BetterPlacement2, null, 0, 0);


            var BadScore = BadPlacement.GetScore(typeof(StandardTetrisHandler), scorer);
            var BetterScore = BetterPlacement.GetScore(typeof(StandardTetrisHandler), scorer);
            //TODO: instead of using this separate routine, we should have some sort of special testing gamestate available for it. Then we can do stuff like visually "watch" the training process or something.
            var DepthData = new DepthSearchInfo(null, BetterPlacement, new Nomino[] { TMino, LMino, IMino,TMino2 },(y) => y.GetScore(typeof(StandardTetrisHandler),scorer),true);
            while (DepthSearchInfo.WorkersActive)
            {
                Thread.Sleep(50);
            }
            ;

            //done, get the leaf nodes.

            var EvaluatedBranch = DepthData.GetLeafNodes().OrderByDescending((d) => d.GetScore()).FirstOrDefault();

            var BadScoreDepth = StandardNominoAI.GetDepthScoreResultConsideringNextQueue(BadPlacement.State, TMino, new[] { LMino }, (y) => y.GetScore(typeof(StandardTetrisHandler), scorer));
            var BetterScoreDepth = StandardNominoAI.GetDepthScoreResultConsideringNextQueue(BetterPlacement.State, TMino, new[] { LMino }, (y) => y.GetScore(typeof(StandardTetrisHandler), scorer));


            var BadIPlacementTest1 = new StoredBoardState(StringToBlocks(QuestionableIPlacement1), null, 0, 0);
            var BetterIPlacementTest1 = new StoredBoardState(StringToBlocks(BetterIPlacement1), null, 0, 0);


            var BadIScore = BadIPlacementTest1.GetScore(typeof(StandardTetrisHandler), scorer);
            var BetterIScore = BetterIPlacementTest1.GetScore(typeof(StandardTetrisHandler), scorer);



            return;
            var bestfutureresults = StandardNominoAI.GetDepthScoreResultConsideringNextQueue(TState, TMino, new Nomino[] { TMino, IMino }, (y) => y.GetScore(typeof(StandardTetrisHandler), scorer));

            int PositionDisplay = 0;
            foreach (var iterate in bestfutureresults.OrderByDescending((w)=>w.Item1))
            {
                PositionDisplay++;
                DebugLogger.Log.WriteLine("Result #" + PositionDisplay + "Immediate Score:" + iterate.Item2.GetScore(typeof(StandardTetrisHandler), scorer) + " Future Score:" + iterate.Item1);
                DebugLogger.Log.WriteLine(iterate.Item2.GetBoardString());
            }

            /*

            //Get test results for I Mino. Best place should be in the "well" of empty space!
            var PossibleBoardResultsI = StandardNominoAI.GetPossibleResults(TestState, IMino);
            //these are all the possible results for the board. Now, rank by the score.
            var RankedResultsI = PossibleBoardResultsI.OrderByDescending((w) => w.GetScore(typeof(StandardTetrisHandler), scorer));
            var TopResultPieceI = RankedResultsI.First();
            int DisplayPos = 1;

            //var clearlines = StandardNominoAI.ClearFilledRows(TopResultPieceI.State);


            DebugLogger.Log.WriteLine("I Piece #1 result:");
            DebugLogger.Log.WriteLine(TopResultPieceI.GetBoardString());
            DebugLogger.Log.WriteLine("With cleared Rows:");
            //DebugLogger.Log.WriteLine(new StoredBoardState(clearlines, null, 0, 0).GetBoardString());
            */
            /*
            foreach (var iterate in RankedResultsI)
            {
                if (!iterate.InvalidState)
                {
                    DebugLogger.Log.WriteLine("I Piece ranked result #" + DisplayPos + " With Score:" + iterate.GetScore(typeof(StandardTetrisHandler), scorer));
                    DebugLogger.Log.WriteLine("X Offset:" + iterate.XOffset + " Rotation:" + iterate.RotationCount);
                    DebugLogger.Log.WriteLine(iterate.GetBoardString());
                }

            }*/
            
            //DebugLogger.Log.WriteLine("I Piece placement result:\n" + TopResultPieceI.GetBoardString());


            //var TMino = new Tetromino_T();

            //var PossibleBoardResultsT = StandardNominoAI.GetPossibleResults(TestState, TMino, scorer);

            //var RankedResultsT = PossibleBoardResultsT.OrderByDescending((w) => w.GetScore(typeof(StandardTetrisHandler), scorer));

            //var TopResultPieceT = RankedResultsT.First();

            //Added: Do test of T Tetromino, it should get slotted to the left.


            //known board state.




        }
        private static NominoBlock?[][] StringToBlocks(String input)
        {
            String[] Lines = input.Split('\n');
            NominoBlock?[][] Result = new NominoBlock[Lines.Length][];
            for (int i = 0; i < Lines.Length; i++)
            {
                Result[i] = Lines[i].Replace("\r","").Select((c) => c == '#' ? new StandardColouredBlock() : null).ToArray();
            }

            return Result;




        }

        private static String QuestionableIPlacement1 =

@"          
          
          
         #
         #
         #
         #
         #
###  #  ##
 #########
######### 
##### ## #
 #########
 #########
 #########
##### ####
## #######
## #######
####### ##
### ######
## ###### 
######## #";

        private static String BetterIPlacement1 =

@"          
          
          
          
          
          
          
####     #
###  #  ##
 #########
######### 
##### ## #
 #########
 #########
 #########
##### ####
## #######
## #######
####### ##
### ######
## ###### 
######## #";


    }
}