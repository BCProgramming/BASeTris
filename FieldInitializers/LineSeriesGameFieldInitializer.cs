using BASeTris.Blocks;
using BASeTris.GameStates.GameHandlers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.FieldInitializers
{
    public class LineSeriesGameFieldInitializerParameters
    {
        public int GameLevel { get; set; }

        public LineSeriesBlock.CombiningTypes[] CombiningTypes { get; set; }
        public LineSeriesGameFieldInitializerParameters(int pGameLevel,LineSeriesBlock.CombiningTypes[] pCombineTypes)
        {
            CombiningTypes = pCombineTypes;
            GameLevel = pGameLevel;
        }
    }
    public class LineSeriesGameFieldInitializer : FieldInitializer
    {
        private LineSeriesGameFieldInitializerParameters _params;

        private IGameCustomizationHandler _Handler;

        public LineSeriesGameFieldInitializer(IGameCustomizationHandler pHandler,LineSeriesGameFieldInitializerParameters parameters)
        {
              _params = parameters;
            _Handler = pHandler;
        }

        public override void Initialize(TetrisField Target)
        {
            HashSet<SKPointI> usedPositions = new HashSet<SKPointI>();
            //primary count is based on our level.
            int numPrimaries = (int)((_params.GameLevel * 2f) + 4);
            numPrimaries = Math.Max(66, numPrimaries);
            for (int i = 0; i < numPrimaries; i++)
            {
                //choose a random primary type.
                var chosentype = TetrisGame.Choose(_params.CombiningTypes);
                LineSeriesPrimaryBlock lsmb = new LineSeriesPrimaryBlock() { CombiningIndex = chosentype };
                var Dummino = new Nomino() { };
                Dummino.AddBlock(new Point[] { new Point(0, 0) }, lsmb);
                Target.Theme.ApplyTheme(Dummino, _Handler, Target, NominoTheme.ThemeApplicationReason.Normal);
                lsmb.CriticalMass = 4; //TODO: should this be changed?

                int RandomXPos = TetrisGame.rgen.Next(Target.ColCount);
                int RandomYPos = Target.RowCount - 1 - TetrisGame.rgen.Next(Target.RowCount / 2);
                SKPointI randomPos = new SKPointI(RandomXPos, RandomYPos);
                while (usedPositions.Contains(randomPos))
                {
                    int rndXPos = TetrisGame.rgen.Next(Target.ColCount);
                    int rndYPos = Target.RowCount - 1 - TetrisGame.rgen.Next(Target.RowCount / 2);
                    randomPos = new SKPointI(rndXPos, rndYPos);
                }
                Target.Contents[RandomYPos][RandomXPos] = lsmb;
                



            }
        }
    }
}
