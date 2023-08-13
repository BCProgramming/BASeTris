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
        public bool DoShinyBlocks { get; set; } = false;
        public Func<LineSeriesBlock, int> GetCriticalMassFunc = GetCriticalMass;
        protected static int GetCriticalMass(LineSeriesBlock lsb)
        {
            return 4;
        }
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

        private IBlockGameCustomizationHandler _Handler;

        public LineSeriesGameFieldInitializer(IBlockGameCustomizationHandler pHandler,LineSeriesGameFieldInitializerParameters parameters)
        {
              _params = parameters;
            _Handler = pHandler;
        }

        public override void Initialize(TetrisField Target)
        {
            HashSet<SKPointI> usedPositions = new HashSet<SKPointI>();
            Dictionary<LineSeriesBlock.CombiningTypes, List<LineSeriesPrimaryBlock>> AddedBlocks = new Dictionary<LineSeriesBlock.CombiningTypes, List<LineSeriesPrimaryBlock>>();
            int currTypeIndex = 0;
            //primary count is based on our level.
            int numPrimaries = (int)((_params.GameLevel * 2f) + 4);
            numPrimaries = Math.Max(10, numPrimaries);
            for (int i = 0; i < numPrimaries; i++)
            {
                bool NearBottom = false;
                //choose a random primary type.
                var chosentype = _params.CombiningTypes[currTypeIndex];
                LineSeriesPrimaryBlock lsmb = null;
                if (!AddedBlocks.ContainsKey(chosentype) && _params.DoShinyBlocks)
                {
                    lsmb = new LineSeriesPrimaryShinyBlock() { CombiningIndex = chosentype };
                    AddedBlocks.Add(chosentype, new List<LineSeriesPrimaryBlock>());
                    NearBottom = true;
                }
                else
                {
                    lsmb = new LineSeriesPrimaryBlock() { CombiningIndex = chosentype };
                }
                if (_params.DoShinyBlocks) AddedBlocks[chosentype].Add(lsmb);

                var Dummino = new Nomino() { };
                Dummino.AddBlock(new Point[] { new Point(0, 0) }, lsmb);
                Target.Theme.ApplyTheme(Dummino, _Handler, Target, NominoTheme.ThemeApplicationReason.Normal);
                lsmb.CriticalMass = _params.GetCriticalMassFunc(lsmb);

                int RandomXPos = TetrisGame.rgen.Next(Target.ColCount);
                int RandomYPos = Target.RowCount - 1 - (TetrisGame.rgen.Next(Target.RowCount / (NearBottom?8:2)));
                SKPointI randomPos = new SKPointI(RandomXPos, RandomYPos);
                while (usedPositions.Contains(randomPos))
                {
                    int rndXPos = TetrisGame.rgen.Next(Target.ColCount);
                    int rndYPos = Target.RowCount - 1 - TetrisGame.rgen.Next(Target.RowCount / (NearBottom ? 8 : 2));
                    randomPos = new SKPointI(rndXPos, rndYPos);
                }
                Target.Contents[RandomYPos][RandomXPos] = lsmb;

                currTypeIndex = (currTypeIndex + 1) % _params.CombiningTypes.Length;


            }
        }
    }
}
