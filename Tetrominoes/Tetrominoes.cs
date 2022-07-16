using BASeTris.Blocks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Tetrominoes
{
    public class Tetromino_I : Tetromino
    {
        public Tetromino_I(Func<int, NominoBlock> BuildBlock = null)
        {
            base.BlockData = GetNominoEntries(new Point[] {TetrominoData.Tetromino_I_1[0], TetrominoData.Tetromino_I_2[0], TetrominoData.Tetromino_I_3[0], TetrominoData.Tetromino_I_4[0]}, new Size(3, 3),BuildBlock).ToList();
            this.SpecialName = "I Tetromino";
            base.SetBlockOwner();
            RecalcExtents();
        }
    }

    public class Tetromino_J : Tetromino
    {
        public Tetromino_J(Func<int, NominoBlock> BuildBlock = null)
        {
            base.BlockData = GetNominoEntries(new Point[] {TetrominoData.Tetromino_J_1[0], TetrominoData.Tetromino_J_2[0], TetrominoData.Tetromino_J_3[0], TetrominoData.Tetromino_J_4[0]}, new Size(2, 2),BuildBlock).ToList();
            this.SpecialName = "J Tetromino";
            base.SetBlockOwner();
            RecalcExtents();
        }
    }

    public class Tetromino_L : Tetromino
    {
        public Tetromino_L(Func<int, NominoBlock> BuildBlock = null)
        {
            base.BlockData = GetNominoEntries(new Point[] {TetrominoData.Tetromino_L_1[0], TetrominoData.Tetromino_L_2[0], TetrominoData.Tetromino_L_3[0], TetrominoData.Tetromino_L_4[0]}, new Size(2, 2),BuildBlock).ToList();
            this.SpecialName = "L Tetromino";
            base.SetBlockOwner();
            RecalcExtents();
        }
    }

    public class Tetromino_O : Tetromino
    {
        public Tetromino_O(Func<int, NominoBlock> BuildBlock = null)
        {
            base.BlockData = GetNominoEntries(new Point[] {TetrominoData.Tetromino_O_1[0], TetrominoData.Tetromino_O_2[0], TetrominoData.Tetromino_O_3[0], TetrominoData.Tetromino_O_4[0]}, new Size(1, 1),BuildBlock).ToList();
            this.SpecialName = "O Tetromino";
            base.SetBlockOwner();
            RecalcExtents();
        }
    }

    public class Tetromino_S : Tetromino
    {
        public Tetromino_S(Func<int, NominoBlock> BuildBlock = null)
        {
            base.BlockData = GetNominoEntries(new Point[] {TetrominoData.Tetromino_S_1[0], TetrominoData.Tetromino_S_2[0], TetrominoData.Tetromino_S_3[0], TetrominoData.Tetromino_S_4[0]}, new Size(2, 2),BuildBlock).ToList();
            this.SpecialName = "S Tetromino";
            base.SetBlockOwner();
            RecalcExtents();
        }
    }

    public class Tetromino_T : Tetromino
    {
        public Tetromino_T(Func<int, NominoBlock> BuildBlock = null)
        {
            base.BlockData = GetNominoEntries(new Point[] {TetrominoData.Tetromino_T_1[0], TetrominoData.Tetromino_T_2[0], TetrominoData.Tetromino_T_3[0], TetrominoData.Tetromino_T_4[0]}, new Size(2, 2),BuildBlock).ToList();

            this.SpecialName = "T Tetromino";
            base.SetBlockOwner();
            RecalcExtents();
        }
    }

    public class Tetromino_Z : Tetromino
    {
        public Tetromino_Z(Func<int, NominoBlock> BuildBlock = null)
        {
            base.BlockData = GetNominoEntries(new Point[] {TetrominoData.Tetromino_Z_1[0], TetrominoData.Tetromino_Z_2[0], TetrominoData.Tetromino_Z_3[0], TetrominoData.Tetromino_Z_4[0]}, new Size(2, 2),BuildBlock).ToList();
            this.SpecialName = "Z Tetromino";
            base.SetBlockOwner();
            RecalcExtents();
        }
    }

    public class Tetromino_Y : Tetromino
    {
        public Tetromino_Y(Func<int, NominoBlock> BuildBlock = null)
        {
            base.BlockData = GetNominoEntries(new Point[] { TetrominoData.Tetromino_Y_1[0], TetrominoData.Tetromino_Y_2[0], TetrominoData.Tetromino_Y_3[0], TetrominoData.Tetromino_Y_4[0] }, new Size(2, 2),BuildBlock).ToList();
            this.SpecialName = "Y Tetromino";
            base.SetBlockOwner();
            RecalcExtents();
        }
    }


    public class Tetromino_G : Tetromino
    {
        public Tetromino_G(Func<int, NominoBlock> BuildBlock = null)
        {
            base.BlockData = GetNominoEntries(new Point[] { TetrominoData.Tetromino_G_1[0], TetrominoData.Tetromino_G_2[0], TetrominoData.Tetromino_G_3[0], TetrominoData.Tetromino_G_4[0] }, new Size(3, 3),BuildBlock).ToList();
            this.SpecialName = "G Tetromino";
            base.SetBlockOwner();
            RecalcExtents();
        }
    }

    public class Tetromino_F : Tetromino
    {
        public Tetromino_F(Func<int, NominoBlock> BuildBlock = null)
        {
            base.BlockData = GetNominoEntries(new Point[] { TetrominoData.Tetromino_F_1[0], TetrominoData.Tetromino_F_2[0], TetrominoData.Tetromino_F_3[0], TetrominoData.Tetromino_F_4[0] }, new Size(3, 3),BuildBlock).ToList();
            this.SpecialName = "F Tetromino";
            base.SetBlockOwner();
            RecalcExtents();
        }
    }


}