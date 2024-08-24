using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BASeCamp.Elementizer;
using BASeTris.GameStates;
using BASeTris.GameStates.GameHandlers;
using BASeTris.Rendering.Adapters;
using BASeTris.Tetrominoes;

namespace BASeTris
{
    public abstract class BaseStatistics:IXmlPersistable
    {
        public virtual Dictionary<String, String> GetDisplayStatistics(IStateOwner pOwner,GameplayGameState Source)
        {

            //stats:  Time, Score, Top Score


            var TopScore = Source.GetLocalScores() == null ? 0 : Source.GetLocalScores().GetScores().First().Score;
            int MaxScoreLength = Math.Max(TopScore.ToString().Length, Score.ToString().Length);

            String CurrentScoreStr = Score.ToString().PadLeft(MaxScoreLength);
            String TopScoreStr = TopScore.ToString().PadLeft(MaxScoreLength);

           


            return new Dictionary<string, string>()
            {
                {"Time",FormatGameTime(pOwner) },
                { "Score",CurrentScoreStr },
                { "Top",TopScoreStr }
            };

            



        }
        protected String FormatGameTime(IStateOwner stateowner)
        {
            TimeSpan useCalc = stateowner.GetElapsedTime();
            return useCalc.ToString(@"hh\:mm\:ss\:ff");
        }
        public int Score = 0;
        private TimeSpan[] LevelReachTimes = new TimeSpan[] { TimeSpan.Zero };

        public TimeSpan[] LevelTimes
        {
            get { return new List<TimeSpan>(LevelReachTimes).ToArray(); }
        }
        public TimeSpan TotalGameTime = TimeSpan.MinValue;
        public void SetLevelTime(TimeSpan ReachedTime)
        {
            LevelReachTimes = LevelReachTimes.Concat(new TimeSpan[] { ReachedTime }).ToArray();
        }
        public void AddScore(int AddScore)
        {
            this.Score += AddScore;
        }
        public BaseStatistics()
        {
        }
        public BaseStatistics(XElement pSourceData, Object pContextData)
        {
            Score = pSourceData.GetAttributeInt("Score");
            TotalGameTime = TimeSpan.FromTicks(pSourceData.GetAttributeLong("TotalGameTime"));
            var ReadReachTimes = ((long[])pSourceData.ReadArray<long>("LevelTimes", new long[] { }))   .Select((L) => TimeSpan.FromTicks(L));
            LevelReachTimes = ReadReachTimes.ToArray();
        }
        public virtual XElement GetXmlData(String pNodeName, Object pContextData)
        {
            XElement ResultNode = new XElement(pNodeName, new XAttribute("Score", Score), new XAttribute("TotalGameTime", TotalGameTime.Ticks));
            XElement LevelReachTimesElement = StandardHelper.SaveArray(LevelTimes.Select((L)=>L.Ticks).ToArray(), "LevelTimes", pContextData);
            ResultNode.Add(LevelReachTimesElement);
            return ResultNode;
        }


    }

    






    public class DrMarioStatistics : BaseStatistics
    {
        public override Dictionary<string, string> GetDisplayStatistics(IStateOwner pOwner,GameplayGameState Source)
        {
            var stats = base.GetDisplayStatistics(pOwner, Source);
            stats.Add("Level", (Source.GameHandler as DrMarioHandler).Level.ToString());
            stats.Add("Virus", (Source.GameHandler as DrMarioHandler).PrimaryBlockCount.ToString());
            //we want to show a VIRUS: count too.
            //implement that into the handler first for us to access here!.
            return stats;
        }
        public DrMarioStatistics(XElement src, Object pContext) : base(src, pContext)
        {
        }
        public DrMarioStatistics()
        {
        }
    }
    public class TetrisAttackStatistics : BaseStatistics
    {
        public TetrisAttackStatistics(XElement src, Object pContext) : base(src, pContext)
        {
        }
        public TetrisAttackStatistics()
        {
        }
    }
    public class ColumnsStatistics : BaseStatistics
    {
        public override Dictionary<string, string> GetDisplayStatistics(IStateOwner pOwner, GameplayGameState Source)
        {
            var stats = base.GetDisplayStatistics(pOwner, Source);
            //stats.Add("Level", (Source.GameHandler as DrMarioHandler).Level.ToString());
            //stats.Add("Virus", (Source.GameHandler as DrMarioHandler).PrimaryBlockCount.ToString());
            //we want to show a VIRUS: count too.
            //implement that into the handler first for us to access here!.
            return stats;
        }
        public ColumnsStatistics(XElement src, Object pContext) : base(src, pContext)
        {
        }
        public ColumnsStatistics()
        {
        }
    }
    public class Tetris2Statistics :BaseStatistics
    {
        public Tetris2Statistics(XElement src, Object pContext) : base(src, pContext)
        {
        }
        public Tetris2Statistics()
        {
        }
    }
    public class TetrisStatistics:BaseStatistics
    {

        public class TetrisStatusRenderLine
        {
            public Object ElementSource { get; set; }
            public int PieceCount { get; set; }
            public int LineCount { get; set; }


        }

        public TetrisStatistics()
        {
        }
        public TetrisStatistics(XElement pSource, Object pContext)
        {

        }
        public XElement GetXmlData(String pNodeName, Object pContext)
        {
            XElement result = base.GetXmlData(pNodeName, pContext);
            
            XElement PieceCountElement = StandardHelper.SaveDictionary<Type,int>(PieceCounts, "PieceCounts",pContext);
            XElement PieceCountExtendedElement = StandardHelper.SaveDictionary<String, int>(PieceCountsExtended, "PieceCountsExtended", pContext);
            XElement LineCountElement = StandardHelper.SaveDictionary<Type, int>(LineCounts, "LineCounts", pContext);
            XElement LineCountElementExtended = StandardHelper.SaveDictionary<String, int>(LineCountsExtended, "LineCountExtended", pContext);

            result.Add(PieceCountElement, PieceCountExtendedElement, LineCountElement, LineCountElementExtended);

            return result;
        }
        private String GetExKey(String StrRep, Dictionary<String, int> SourceDictionary)
        {
            if (SourceDictionary.ContainsKey(StrRep)) return StrRep;
            var nPoints = NNominoGenerator.FromString(StrRep).ToList();
            String[] Rots = NNominoGenerator.GetOtherRotationStrings(nPoints);
            String foundMatch = Rots.FirstOrDefault((r) => SourceDictionary.ContainsKey(r));
            return foundMatch ?? StrRep;


        }
        
        private String GetExKey(Nomino src, Dictionary<String, int> SourceDictionary)
        {
            //first check for the type.
            var checktype = src.GetType();
            


            var ppt = NNominoGenerator.GetNominoPoints(src);
            String sPieceString = NNominoGenerator.StringRepresentation(ppt);
            //are we in the piece text lookup?
            if (SourceDictionary.ContainsKey(sPieceString))
                return sPieceString;
            else
            {
                String[] Rots = NNominoGenerator.GetOtherRotationStrings(src);
                String foundMatch = Rots.FirstOrDefault((r) => SourceDictionary.ContainsKey(r));
                if (foundMatch == null)
                {
                    return sPieceString;

                }
                else
                {
                    return foundMatch;
                }


            }
        }
        public override Dictionary<string, string> GetDisplayStatistics(IStateOwner pOwner,GameplayGameState Source)
        {
            var result = base.GetDisplayStatistics(pOwner, Source);
            int LineCount = Source.GameStats is TetrisStatistics ? (Source.GameStats as TetrisStatistics).LineCount : 0;
            result.Add("Lines", LineCount.ToString());
            return result;
        }
        [Flags]
        public enum ElementStatFlags
        {
            Flags_None = 0,
            Flags_DefaultTetrominoes_Always = 1
        }
        public List<TetrisStatusRenderLine> GetElementStats(ElementStatFlags pFlags = ElementStatFlags.Flags_DefaultTetrominoes_Always)
        {
            List<TetrisStatusRenderLine> result = new List<TetrisStatusRenderLine>();
            if(pFlags.HasFlag(ElementStatFlags.Flags_DefaultTetrominoes_Always))
                foreach (var kvp in PieceCounts)
                {
                    var LineCount = GetLineCount(kvp.Key);
                    result.Add(new TetrisStatusRenderLine() { ElementSource = kvp.Key, PieceCount = kvp.Value, LineCount = LineCount });

                }
            if (PieceCountsExtended.Any()) //note: can't have lines for a piece without having piece counts- realistically, that is.
            {
                foreach (var kvp in PieceCountsExtended)
                {
                    var LineCount = GetLineCount(kvp.Key);
                    result.Add(new TetrisStatusRenderLine() { ElementSource = kvp.Key, PieceCount = kvp.Value, LineCount = LineCount });
                }
            }
            return result;
        }

        public Dictionary<String, int> GetPieceCounts()
        {
            Dictionary<String, int> BuildResult = new Dictionary<string, int>();
            foreach (var kvp in PieceCounts)
            {
                BuildResult.Add(kvp.Key.Name, kvp.Value);
            }
            if (PieceCountsExtended.Any())
            {
                foreach (var kvp in PieceCountsExtended)
                {
                    BuildResult.Add(kvp.Key, kvp.Value);
                }
            }
            return BuildResult;
        }

        public Dictionary<String, int> GetLineCounts()
        {
            Dictionary<String, int> BuildResult = new Dictionary<string, int>();
            foreach (var kvp in LineCounts)
            {
                BuildResult.Add(kvp.Key.Name, kvp.Value);
            }
            if (LineCountsExtended.Any())
            {
                foreach (var kvp in LineCountsExtended)
                {
                    BuildResult.Add(kvp.Key, kvp.Value);
                }
            }
            return BuildResult;
        }

        public int LineCount
        {
            get { return LineCounts.Sum((w) => w.Value); }
        }
        public int Level
        {
            get
            {
                return (int)(LineCount / 10);
            }
        }
        public int I_Piece_Count
        {
            get => PieceCounts[typeof(Tetromino_I)];
            set => PieceCounts[typeof(Tetromino_I)] = value;
        }

        public int J_Piece_Count
        {
            get => PieceCounts[typeof(Tetromino_J)];
            set => PieceCounts[typeof(Tetromino_J)] = value;
        }

        public int L_Piece_Count
        {
            get => PieceCounts[typeof(Tetromino_L)];
            set => PieceCounts[typeof(Tetromino_L)] = value;
        }

        public int O_Piece_Count
        {
            get => PieceCounts[typeof(Tetromino_O)];
            set => PieceCounts[typeof(Tetromino_O)] = value;
        }

        public int S_Piece_Count
        {
            get => PieceCounts[typeof(Tetromino_S)];
            set => PieceCounts[typeof(Tetromino_S)] = value;
        }

        public int T_Piece_Count
        {
            get => PieceCounts[typeof(Tetromino_T)];
            set => PieceCounts[typeof(Tetromino_T)] = value;
        }

        public int Z_Piece_Count
        {
            get => PieceCounts[typeof(Tetromino_Z)];
            set => PieceCounts[typeof(Tetromino_Z)] = value;
        }
        public int GetPieceCount(String strRep)
        {
            String sKey = GetExKey(strRep, PieceCountsExtended);
            return PieceCountsExtended.ContainsKey(sKey) ? PieceCountsExtended[sKey] : 0;
        }
        public int GetPieceCount(Nomino src)
        {
            if (src is Tetromino) return PieceCounts[src.GetType()];
            String sKey = GetExKey(src, PieceCountsExtended);
            return PieceCountsExtended.ContainsKey(sKey) ? PieceCountsExtended[sKey] : 0;
        }
        public void IncrementPieceCount(Nomino src)
        {
            SetPieceCount(src, GetPieceCount(src) + 1);
        }
        public void IncrementPieceCount(String strRep)
        {
            SetPieceCount(strRep, GetPieceCount(strRep) + 1);
        }
        public void SetPieceCount(String strRep, int pValue)
        {
            string sKey = GetExKey(strRep, PieceCountsExtended);
            PieceCountsExtended[sKey] = pValue;
        }
        public void SetPieceCount(Nomino src, int pValue)
        {
            if (src is Tetromino)
            {
                PieceCounts[src.GetType()] = pValue;
                return;
            }
            String sKey = GetExKey(src, PieceCountsExtended);
            PieceCountsExtended[sKey] = pValue;
        }
        public int GetPieceCount(Type TetronimoType)
        {
            return PieceCounts[TetronimoType];
        }

        public void SetPieceCount(Type TetronimoType, int pValue)
        {
            PieceCounts[TetronimoType] = pValue;
        }

        private Dictionary<String, int> LineCountsExtended = new Dictionary<string, int>();
        private Dictionary<String, int> PieceCountsExtended = new Dictionary<string, int>();
        

        private Dictionary<Type, int> PieceCounts =
            new Dictionary<Type, int>()
            {
                {typeof(Tetromino_I), 0},
                {typeof(Tetromino_O), 0},
                {typeof(Tetromino_J), 0},
                {typeof(Tetromino_T), 0},
                {typeof(Tetromino_L), 0},
                {typeof(Tetromino_S), 0},
                {typeof(Tetromino_Z), 0}
            };


        public int GetLineCount(String strRep)
        {
            String sKey = GetExKey(strRep, LineCountsExtended);
            return LineCountsExtended.ContainsKey(sKey) ? LineCountsExtended[sKey] : 0;
        }
        public int GetLineCount(Nomino src)
        {
            String sKey = GetExKey(src, LineCountsExtended);
            return LineCountsExtended.ContainsKey(sKey)?LineCountsExtended[sKey]:0;
        }
        public void IncrementLineCount(String strRep, int amount)
        {
            SetPieceCount(strRep, GetPieceCount(strRep) + amount);
        }
        public void IncrementLineCount(Nomino src,int amount)
        {
            SetPieceCount(src, GetPieceCount(src) + amount);
        }
        public void SetLineCount(String strRep, int pValue)
        {
            String sKey = GetExKey(strRep, LineCountsExtended);
            LineCountsExtended[sKey] = pValue;
        }
                public void SetLineCount(Nomino src, int pValue)
        {
            String sKey = GetExKey(src, LineCountsExtended);
            LineCountsExtended[sKey] = pValue;
        }

        public int GetLineCount(Type TetrominoType)
        {
            return LineCounts[TetrominoType];
        }

        public void SetLineCount(Type TetrominoType, int pValue)
        {
            LineCounts[TetrominoType] = pValue;
        }

     

        public void AddLineCount(Type TetrominoType, int AddLines)
        {
            if (!LineCounts.ContainsKey(TetrominoType)) LineCounts.Add(TetrominoType, 0);
            LineCounts[TetrominoType] += AddLines;
        }

        private Dictionary<Type, int> LineCounts =
            new Dictionary<Type, int>()
            {
                {typeof(Tetromino_I), 0},
                {typeof(Tetromino_O), 0},
                {typeof(Tetromino_J), 0},
                {typeof(Tetromino_T), 0},
                {typeof(Tetromino_L), 0},
                {typeof(Tetromino_S), 0},
                {typeof(Tetromino_Z), 0}
            };


        public int I_Line_Count
        {
            get => LineCounts[typeof(Tetromino_I)];
            set => LineCounts[typeof(Tetromino_I)] = value;
        }

        public int J_Line_Count
        {
            get => LineCounts[typeof(Tetromino_J)];
            set => LineCounts[typeof(Tetromino_J)] = value;
        }

        public int L_Line_Count
        {
            get => LineCounts[typeof(Tetromino_L)];
            set => LineCounts[typeof(Tetromino_L)] = value;
        }

        public int O_Line_Count
        {
            get => LineCounts[typeof(Tetromino_O)];
            set => LineCounts[typeof(Tetromino_O)] = value;
        }

        public int S_Line_Count
        {
            get => LineCounts[typeof(Tetromino_S)];
            set => LineCounts[typeof(Tetromino_S)] = value;
        }

        public int T_Line_Count
        {
            get => LineCounts[typeof(Tetromino_T)];
            set => LineCounts[typeof(Tetromino_T)] = value;
        }

        public int Z_Line_Count
        {
            get => LineCounts[typeof(Tetromino_Z)];
            set => LineCounts[typeof(Tetromino_Z)] = value;
        }


        //        Type[] useTypes = new Type[] { typeof(Tetromino_I), typeof(Tetromino_O), typeof(Tetromino_J), typeof(Tetromino_T), typeof(Tetromino_L), typeof(Tetromino_S), typeof(Tetromino_Z) };
        //       int[] PieceCounts = new int[] { useStats.I_Piece_Count, useStats.O_Piece_Count, useStats.J_Piece_Count, useStats.T_Piece_Count, useStats.L_Piece_Count, useStats.S_Piece_Count, useStats.Z_Piece_Count };
    }
}