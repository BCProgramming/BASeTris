using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace BASeTris
{
    public class TetrominoData
    {
        private static T[,] rotate<T>(T[,] matrix)
        {
            int n = matrix.GetUpperBound(0);


            T[,] rotated = new T[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    rotated[i, j] = matrix[n - j - 1, i];
                }
            }

            return rotated;
        }


        //first block of I tetromino and it's positions.

        #region I Tetromino Point Data

        public static Point[] Tetromino_I_1 = new Point[]
        {
            new Point(0, 1), new Point(2, 0), new Point(3, 2), new Point(1, 3)
        };

        public static Point[] Tetromino_I_2 = new Point[]
        {
            new Point(1, 1), new Point(2, 1), new Point(2, 2), new Point(1, 2)
        };

        public static Point[] Tetromino_I_3 = new Point[]
        {
            new Point(2, 1), new Point(2, 2), new Point(1, 2), new Point(1, 1)
        };

        public static Point[] Tetromino_I_4 = new Point[]
        {
            new Point(3, 1), new Point(2, 3), new Point(0, 2), new Point(1, 0)
        };

        #endregion

        #region  J Tetromino Point Data

        public static Point[] Tetromino_J_1 = new Point[]
        {
            new Point(0, 0), new Point(2, 0), new Point(2, 2), new Point(0, 2)
        };

        public static Point[] Tetromino_J_2 = new Point[]
        {
            new Point(0, 1), new Point(1, 0), new Point(2, 1), new Point(1, 2)
        };

        public static Point[] Tetromino_J_3 = new Point[]
        {
            new Point(1, 1)
        };

        public static Point[] Tetromino_J_4 = new Point[]
        {
            new Point(2, 1), new Point(1, 2), new Point(0, 1), new Point(1, 0)
        };

        #endregion

        #region L Tetromino Data

        public static Point[] Tetromino_L_1 = new Point[]
        {
            new Point(0, 1), new Point(1, 0), new Point(2, 1), new Point(1, 2)
        };

        public static Point[] Tetromino_L_2 = new Point[]
        {
            new Point(1, 1)
        };

        public static Point[] Tetromino_L_3 = new Point[]
        {
            new Point(2, 1), new Point(1, 2), new Point(0, 1), new Point(1, 0)
        };

        public static Point[] Tetromino_L_4 = new Point[]
        {
            new Point(2, 0), new Point(2, 2), new Point(0, 2), new Point(0, 0)
        };

        #endregion

        #region O Tetromino Data

        public static Point[] Tetromino_O_1 = new Point[]
        {
            new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(0, 1)
        };

        public static Point[] Tetromino_O_2 = new Point[]
        {
            new Point(0, 1), new Point(0, 0), new Point(1, 0), new Point(1, 1)
        };

        public static Point[] Tetromino_O_3 = new Point[]
        {
            new Point(1, 1), new Point(0, 1), new Point(0, 0), new Point(1, 0)
        };

        public static Point[] Tetromino_O_4 = new Point[]
        {
            new Point(1, 0), new Point(1, 1), new Point(0, 1), new Point(0, 0)
        };

        #endregion

        #region S tetromino Data

        public static Point[] Tetromino_S_1 = new Point[]
        {
            new Point(0, 1), new Point(1, 0), new Point(2, 1), new Point(1, 2)
        };

        public static Point[] Tetromino_S_2 = new Point[]
        {
            new Point(1, 1), new Point(1, 1), new Point(1, 1), new Point(1, 1)
        };

        public static Point[] Tetromino_S_3 = new Point[]
        {
            new Point(1, 0), new Point(2, 1), new Point(1, 2), new Point(0, 1)
        };

        public static Point[] Tetromino_S_4 = new Point[]
        {
            new Point(2, 0), new Point(2, 2), new Point(0, 2), new Point(0, 0)
        };

        #endregion

        #region T tetromino Data

        public static Point[] Tetromino_T_1 = new Point[]
        {
            new Point(1, 1), new Point(1, 1), new Point(1, 1), new Point(1, 1)
        };

        public static Point[] Tetromino_T_2 = new Point[]
        {
            new Point(0, 1), new Point(1, 0), new Point(2, 1), new Point(1, 2)
        };

        public static Point[] Tetromino_T_3 = new Point[]
        {
            new Point(1, 0), new Point(2, 1), new Point(1, 2), new Point(0, 1)
        };

        public static Point[] Tetromino_T_4 = new Point[]
        {
            new Point(2, 1), new Point(1, 2), new Point(0, 1), new Point(1, 0)
        };

        #endregion

        #region Z Tetromino Data

        public static Point[] Tetromino_Z_1 = new Point[]
        {
            new Point(0, 0), new Point(2, 0), new Point(2, 2), new Point(0, 2)
        };

        public static Point[] Tetromino_Z_2 = new Point[]
        {
            new Point(1, 0), new Point(2, 1), new Point(1, 2), new Point(0, 1)
        };

        public static Point[] Tetromino_Z_3 = new Point[]
        {
            new Point(1, 1), new Point(1, 1), new Point(1, 1), new Point(1, 1)
        };

        public static Point[] Tetromino_Z_4 = new Point[]
        {
            new Point(2, 1), new Point(1, 2), new Point(0, 1), new Point(1, 0)
        };

        #endregion

        //added tetrominoes for Tetris 2. y and g (better named as j, but that is already used...)
        #region Y Tetromino Data

        public static Point[] Tetromino_Y_1 = new Point[]
        {
            new Point(0, 1), new Point(1, 0), new Point(2, 1), new Point(1, 2)
        };

        public static Point[] Tetromino_Y_2 = new Point[]
        {
            new Point(1, 1), new Point(1, 1), new Point(1, 1), new Point(1, 1)
        };

        public static Point[] Tetromino_Y_3 = new Point[]
        {
            new Point(2, 0), new Point(2, 2), new Point(0, 2), new Point(0, 0)
        };

        public static Point[] Tetromino_Y_4 = new Point[]
        {
            new Point(2, 2), new Point(0, 2), new Point(0, 0), new Point(2, 0)
        };

        #endregion


        #region g Tetromino Data

        public static Point[] Tetromino_G_1 = new Point[]
        {
            new Point(0, 1), new Point(1, 0), new Point(3, 1), new Point(2, 3)
        };

        public static Point[] Tetromino_G_2 = new Point[]
        {
            new Point(1, 1), new Point(1, 1), new Point(2, 1), new Point(2, 2)
        };

        public static Point[] Tetromino_G_3 = new Point[]
        {
            new Point(2, 1), new Point(1, 2), new Point(1, 1), new Point(2, 1)
        };

        public static Point[] Tetromino_G_4 = new Point[]
        {
            new Point(3, 0), new Point(2, 3), new Point(0, 2), new Point(1, 0)
        };

        #endregion

        #region f Tetromino Data

        public static Point[] Tetromino_F_1 = new Point[]
        {
            new Point(0, 1), new Point(2, 0), new Point(3, 2), new Point(1, 3)
        };

        public static Point[] Tetromino_F_2 = new Point[]
        {
            new Point(1, 1), new Point(2, 1), new Point(2, 2), new Point(1, 2)
        };

        public static Point[] Tetromino_F_3 = new Point[]
        {
            new Point(2, 1), new Point(2, 2), new Point(1, 2), new Point(1, 1)
        };

        public static Point[] Tetromino_F_4 = new Point[]
        {
            new Point(3, 2), new Point(1, 3), new Point(0, 1), new Point(2, 0)
        };

        #endregion




    }
}