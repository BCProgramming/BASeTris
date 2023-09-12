using BASeTris.AI;
using BASeTris.Choosers;
using BASeTris.Rendering.Skia.GameStates;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BASeTris.GameStates.GameHandlers
{
    [GamePreparer(typeof(NTrisGamePreparer))]
    [HandlerTipText("Standard Tetris, Customizable Nomino sizes.")]
    public class NTrisGameHandler : StandardTetrisHandler,IPreparableGame
    {

        protected int MaxAddedBlockCount = 0;
        Dictionary<int, String> PrefixText = new Dictionary<int, string>()
        {
            {2,"Duo" },
            {3,"Tri" },
            {4,"Te" },
            {5,"Pen" },
            {6,"Hex" },
            {7,"Hep" },
            {8,"Oc" },
            {9,"Enne" },
            {10,"Dec" },
            {11,"Hendec" },
            {12,"Dodec" },
            {13,"Decatria" },
            {14,"Decatettara" }
        };
        //private int BlockCount = 4;
        public override string Name
        {
            get
            {
                if (_NTrisPreparer == null) return "N-Tris";
                if (_NTrisPreparer.MinimumNominoSize == _NTrisPreparer.MaximumNominoSize)
                {
                    if (PrefixText.ContainsKey(_NTrisPreparer.MinimumNominoSize)) return PrefixText[_NTrisPreparer.MinimumNominoSize] + "tris";
                    else return _NTrisPreparer.MinimumNominoSize.ToString() + "-tris";
                }
                else
                {
                    return $"{_NTrisPreparer.MinimumNominoSize}-{_NTrisPreparer.MaximumNominoSize}";
                }

            }
        }
        public override void SetPrepData(GamePreparerOptions gpo)
        {
            base.SetPrepData(gpo);
            _NTrisPreparer = gpo as NTrisGamePreparer;
        }
        private NTrisGamePreparer _NTrisPreparer = null;
        public NTrisGameHandler()
        {
        }
        private NTrisGameHandler(int pBlockCount)
        {
            _NTrisPreparer = new NTrisGamePreparer(typeof(NTrisGameHandler)) { MaximumNominoSize = pBlockCount, MinimumNominoSize = pBlockCount };
            
        }
        private Choosers.BlockGroupChooser _Chooser = null;
        private IStateOwner _Owner = null;
        public override BlockGroupChooser GetChooser(IStateOwner pOwner)
        {
            _Owner = pOwner;
            if (_Chooser == null)
            {
                if (MaxAddedBlockCount == 0)
                {
                    _Chooser = new Choosers.SingleFunctionChooser(NTrisChooserFunction);
                }
                else
                {

                }
            }
            return _Chooser;
        }
        public Nomino NTrisChooserFunction()
        {
            int chooseSize = TetrisGame.rgen.Next(_NTrisPreparer.MinimumNominoSize, _NTrisPreparer.MaximumNominoSize + 1);
            var newpiece = NNominoGenerator.GetPiece(chooseSize);
            var buildNomino = NNominoGenerator.CreateNomino(newpiece);
            return buildNomino;
        }
        public override FieldCustomizationInfo GetFieldInfo()
        {
            return new FieldCustomizationInfo()
            {
                FieldColumns = GetFieldColumnWidth(),
                FieldRows = GetFieldRowHeight(),
                TopHiddenFieldRows = 2,
                BottomHiddenFieldRows = 0
            };
        }
        private int GetFieldColumnWidth()
        {
            return (int)_NTrisPreparer.ColumnCount;
            //return 6 + BlockCount + (int)((Math.Max(0, BlockCount - 4) * 1.1));
        }
        private int GetFieldRowHeight()
        {
            return (int)_NTrisPreparer.RowCount;
            
            /*
             * var CurrWidth = GetFieldColumnWidth();
            int DesiredHeight = (22 / 10) * CurrWidth;
            return DesiredHeight;
            */
            //return 18 + BlockCount + (int)((Math.Max(0, BlockCount - 4) * 2));
        }
        public int GetHiddenRowCount()
        {
            return 2;
            /*var currwidth = GetFieldColumnWidth();
            return ((22 / 10) * currwidth) - ((20 / 10) * currwidth);*/
        }
        public override IGameCustomizationStatAreaRenderer<TRenderTarget, GameplayGameState, TDataElement, IStateOwner> GetStatAreaRenderer<TRenderTarget, TDataElement>()
        {

            if (typeof(TRenderTarget) == typeof(SKCanvas))
            {
                if (StatRenderer == null)
                    StatRenderer = new StandardTetrisSkiaStatAreaRenderer() { AlwaysDrawDefaultTetrominoes = false };
                return (IGameCustomizationStatAreaRenderer<TRenderTarget, GameplayGameState, TDataElement, IStateOwner>)StatRenderer;
            };
            
        
                return null;
            

        }


    }

    /*
    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    [HandlerTipText("Pentris - mino's have 5 blocks instead of 4.")]
public class PentrisGameHandler:NTrisGameHandler
    {
    public PentrisGameHandler() : base(5) { }
}
    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    [HandlerTipText("Hextris - mino's have 6 blocks.")]
    public class SextrisGameHandler : NTrisGameHandler
    {
        public SextrisGameHandler() : base(6) { }
    }
    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    [HandlerTipText("Heptatris - mino's have 7 blocks. There seems to be a pattern here.")]
    public class HeptaTrisGameHandler : NTrisGameHandler
    {
        public HeptaTrisGameHandler() : base(7) { }
    }
    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    [HandlerTipText("Octris. mino's have 8 blocks. Yep, definitely seeing a pattern.")]
    public class OctrisGameHandler : NTrisGameHandler
    {
        public OctrisGameHandler() : base(8) { }
    }
    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    [HandlerTipText("Enneatris. Mino's have 9 blocks.")]
    public class EnneatrisGameHandler : NTrisGameHandler
    {
        public EnneatrisGameHandler() : base(9) { }
    }
    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    [HandlerTipText("Dectris. Minos have 10 blocks. OK, this is starting to get silly")]
    public class DectrisGameHandler : NTrisGameHandler
    {
        public DectrisGameHandler() : base(10) { }
    }
    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    [HandlerTipText("HenDectris. Minos have 11 blocks.")]
    public class HenDectrisGameHandler : NTrisGameHandler
    {
        public HenDectrisGameHandler() : base(11) { }
    }
    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    [HandlerTipText("DoDectris. Minos have 12 blocks.")]
    public class DoDectrisGameHandler : NTrisGameHandler
    {
        public DoDectrisGameHandler() : base(12) { }
    }

    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    [HandlerTipText("Tettaracontatris. Minos have 32 blocks. Who can forget this classic.")]
    public class tettaracontatrisGameHandler : NTrisGameHandler
    {
        public tettaracontatrisGameHandler() : base(32) { }
    }
    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    [HandlerTipText("Alright well now you are just getting silly. This doesn't even work well. I can't handle it. It's too many.")]
    public class HecatotrisGameHandler : NTrisGameHandler
    {
        public HecatotrisGameHandler() : base(100) { }
    }

    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    [HandlerTipText("Tritris. Minos have 3 blocks. ")]
    public class TriTrisGameHandler : NTrisGameHandler
    {
        public TriTrisGameHandler() : base(3) { }
    }
    */
}
