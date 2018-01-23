using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris
{
    public class TetrominoTheme
    {
        public static BlockGroup GetTetromino_I()
        {
            return BlockGroup.GetTetromino_Array(new Point[][] { TetrominoData.Tetromino_I_1, TetrominoData.Tetromino_I_2, TetrominoData.Tetromino_I_3, TetrominoData.Tetromino_I_4 }, Color.DeepSkyBlue, Color.DeepSkyBlue,"I Tetromino");

        }

        public static BlockGroup GetTetromino_J()
        {
            return BlockGroup.GetTetromino_Array(new Point[][] { TetrominoData.Tetromino_J_1, TetrominoData.Tetromino_J_2, TetrominoData.Tetromino_J_3, TetrominoData.Tetromino_J_4 }, Color.Blue, Color.Blue,"J Tetromino");

        }

        public static BlockGroup GetTetromino_L()
        {
            return BlockGroup.GetTetromino_Array(new Point[][] { TetrominoData.Tetromino_L_1, TetrominoData.Tetromino_L_2, TetrominoData.Tetromino_L_3, TetrominoData.Tetromino_L_4 }, Color.DeepSkyBlue, Color.DeepSkyBlue,"L Tetromino");
        }
        public static BlockGroup GetTetromino_O()
        {
            return BlockGroup.GetTetromino_Array(new Point[][] { TetrominoData.Tetromino_O_1, TetrominoData.Tetromino_O_2, TetrominoData.Tetromino_O_3, TetrominoData.Tetromino_O_4 }, Color.DeepSkyBlue, Color.White,"O Tetromino");
        }
        public static BlockGroup GetTetromino_S()
        {
            return BlockGroup.GetTetromino_Array(new Point[][] { TetrominoData.Tetromino_S_1, TetrominoData.Tetromino_S_2, TetrominoData.Tetromino_S_3, TetrominoData.Tetromino_S_4 }, Color.Blue, Color.Blue,"S Tetromino");
        }
        public static BlockGroup GetTetromino_T()
        {
            return BlockGroup.GetTetromino_Array(new Point[][] { TetrominoData.Tetromino_T_1, TetrominoData.Tetromino_T_2, TetrominoData.Tetromino_T_3, TetrominoData.Tetromino_T_4 }, Color.Blue, Color.White,"T Tetromino");
        }

        public static BlockGroup GetTetromino_Z()
        {
            return BlockGroup.GetTetromino_Array(new Point[][] { TetrominoData.Tetromino_Z_1, TetrominoData.Tetromino_Z_2, TetrominoData.Tetromino_Z_3, TetrominoData.Tetromino_Z_4 }, Color.DeepSkyBlue, Color.DeepSkyBlue,"Z Tetromino");
        }


    }
}
