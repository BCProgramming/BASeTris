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
    public class DrMarioFieldInitializer : FieldInitializer
    {
        private LineSeriesBlock.CombiningTypes[] _InitTypes;
        private int PlayLevel { get; set; }
        private IGameCustomizationHandler _Handler;

        public DrMarioFieldInitializer(IGameCustomizationHandler pHandler,LineSeriesBlock.CombiningTypes[] pInitTypes,int pPlayLevel)
        {
            _InitTypes = pInitTypes;
            PlayLevel = pPlayLevel;
            _Handler = pHandler;
        }

        public override void Initialize(TetrisField Target)
        {
            HashSet<SKPointI> usedPositions = new HashSet<SKPointI>();
            //primary count is based on our level.
            int numPrimaries = (int)((PlayLevel * 1.33f) + 4);
            for (int i = 0; i < numPrimaries; i++)
            {
                //choose a random primary type.
                var chosentype = TetrisGame.Choose(_InitTypes);
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
