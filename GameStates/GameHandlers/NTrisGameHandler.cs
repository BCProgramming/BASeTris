using BASeTris.AI;
using BASeTris.Choosers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.GameHandlers
{
    public abstract class NTrisGameHandler : StandardTetrisHandler
    {
        Dictionary<int, String> PrefixText = new Dictionary<int, string>()
        {
            {2,"Duo" },
            {3,"Tri" },
            {4,"Te" },
            {5,"Pen" },
            {6,"Hex" },
            {7,"Sep" },
            {8,"Oc" },
            {9,"Non" },
            {10,"Dec" },
            {11,"Unodec" },
            {12,"Duodec" }
        };
        private int BlockCount = 4;
        public override string Name
        {
            get
            {
                if (PrefixText.ContainsKey(BlockCount)) return PrefixText[BlockCount] + "tris";
                else return BlockCount.ToString() + "-tris";

            }
        }
        public NTrisGameHandler(int pBlockCount)
        {
            BlockCount = pBlockCount;
        }
        private Choosers.BlockGroupChooser _Chooser = null;
        private IStateOwner _Owner = null;
        public override BlockGroupChooser GetChooser(IStateOwner pOwner)
        {
            _Owner = pOwner;
            if (_Chooser == null) _Chooser = new Choosers.SingleFunctionChooser(NTrisChooserFunction);
            return _Chooser;
        }
        public Nomino NTrisChooserFunction()
        {
            var newpiece = NNominoGenerator.GetPiece(BlockCount);
            var buildNomino = NNominoGenerator.CreateNomino(newpiece);
            return buildNomino;
        }
        public override int GetFieldColumnWidth()
        {
            return 6 + BlockCount + (int)((Math.Max(0, BlockCount - 4) * 1.1));
        }
        public override int GetFieldRowHeight()
        {
            var CurrWidth = GetFieldColumnWidth();
            int DesiredHeight = (22 / 10) * CurrWidth;
            return DesiredHeight;
            //return 18 + BlockCount + (int)((Math.Max(0, BlockCount - 4) * 2));
        }
        public override int GetHiddenRowCount()
        {
            return 2;
            /*var currwidth = GetFieldColumnWidth();
            return ((22 / 10) * currwidth) - ((20 / 10) * currwidth);*/
        }
        
    }

    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    public class PentrisGameHandler:NTrisGameHandler
    {
    public PentrisGameHandler() : base(5) { }
    }
    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    public class HextrisGameHandler : NTrisGameHandler
    {
        public HextrisGameHandler() : base(6) { }
    }
    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    public class SeptrisGameHandler : NTrisGameHandler
    {
        public SeptrisGameHandler() : base(7) { }
    }
    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    public class OctrisGameHandler : NTrisGameHandler
    {
        public OctrisGameHandler() : base(8) { }
    }
    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    public class NontrisGameHandler : NTrisGameHandler
    {
        public NontrisGameHandler() : base(9) { }
    }
    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    public class DectrisGameHandler : NTrisGameHandler
    {
        public DectrisGameHandler() : base(10) { }
    }
    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    public class UnoDectrisGameHandler : NTrisGameHandler
    {
        public UnoDectrisGameHandler() : base(11) { }
    }
    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    public class DuoDectrisGameHandler : NTrisGameHandler
    {
        public DuoDectrisGameHandler() : base(12) { }
    }

    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    public class TwoDuoDectrisGameHandler : NTrisGameHandler
    {
        public TwoDuoDectrisGameHandler() : base(32) { }
    }
    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    public class CentrisGameHandler : NTrisGameHandler
    {
        public CentrisGameHandler() : base(100) { }
    }

    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    public class TriTrisGameHandler : NTrisGameHandler
    {
        public TriTrisGameHandler() : base(3) { }
    }

}
