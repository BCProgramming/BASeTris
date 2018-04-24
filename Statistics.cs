using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Tetrominoes;
namespace BASeTris
{
 
    public class Statistics
    {
        private TimeSpan[] LevelReachTimes = new TimeSpan[] { TimeSpan.Zero };
        public TimeSpan TotalGameTime = TimeSpan.MinValue;
        public void SetLevelTime(TimeSpan ReachedTime)
        {
            LevelReachTimes = LevelReachTimes.Concat(new TimeSpan[] { ReachedTime }).ToArray();
        }
        public int LineCount {  get { return LineCounts.Sum((w) => w.Value); } }
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
        public int Score = 0;
        
        public int GetPieceCount(Type TetronimoType)
        {
            return PieceCounts[TetronimoType];
        }
        public void SetPieceCount(Type TetronimoType,int pValue)
        {
            PieceCounts[TetronimoType] = pValue;
        }
        private Dictionary<Type, int> PieceCounts =
            new Dictionary<Type, int>()
            {
                { typeof(Tetromino_I),0 },
                { typeof(Tetromino_O),0 },
                { typeof(Tetromino_J),0 },
                { typeof(Tetromino_T),0 },
                { typeof(Tetromino_L),0 },
                { typeof(Tetromino_S),0 },
                { typeof(Tetromino_Z),0 }

            };


        public int GetLineCount(Type TetrominoType)
        {
            return LineCounts[TetrominoType];
        }
        public void SetLineCount(Type TetrominoType,int pValue)
        {
            LineCounts[TetrominoType] = pValue;
        }
        public void AddScore(int AddScore)
        {
            this.Score += AddScore;
        }
        public void AddLineCount(Type TetronimoType,int AddLines)
        {
            LineCounts[TetronimoType] += AddLines;
        }
        private Dictionary<Type, int> LineCounts =
            new Dictionary<Type, int>()
            {
                { typeof(Tetromino_I),0 },
                { typeof(Tetromino_O),0 },
                { typeof(Tetromino_J),0 },
                { typeof(Tetromino_T),0 },
                { typeof(Tetromino_L),0 },
                { typeof(Tetromino_S),0 },
                { typeof(Tetromino_Z),0 }

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
