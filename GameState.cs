using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris
{
    public abstract class GameState
    {
        public enum GameKeys
        {
            GameKey_RotateCW,
            GameKey_RotateCCW,
            GameKey_Drop,
            GameKey_Left,
            GameKey_Right,
            GameKey_Down
        }
        public abstract void GameProc(IStateOwner pOwner);
        public abstract void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds);
        public abstract void HandleGameKey(IStateOwner pOwner, GameKeys g);
    }

    public class StandardTetrisGameState : GameState
    {



        private TetrisField PlayField = null;
        private DateTime lastHorizontalMove = DateTime.MinValue;
        public StandardTetrisGameState(int GarbageRows = 0)
        {
            PlayField = new TetrisField(GarbageRows);
        }
        public override void GameProc(IStateOwner pOwner)
        {
            PlayField.AnimateFrame();
            foreach (var iterate in PlayField.BlockGroups)
            {
                if ((DateTime.Now - iterate.LastFall).TotalMilliseconds > iterate.FallSpeed)
                {

                    if (MoveGroupDown(iterate))
                    {
                        pOwner.EnqueueAction(() =>
                        {
                            PlayField.ProcessLines();
                        });
                    }
                    iterate.LastFall = DateTime.Now;
                }
            }

            if (PlayField.BlockGroups.Count == 0)
            {
                SpawnNewTetromino();
            }
        }
        static Random rgen = new Random();
        public static T Choose<T>(IEnumerable<T> ChooseArray)
        {
            if (rgen == null) rgen = new Random();
            SortedList<double, T> sorttest = new SortedList<double, T>();
            foreach (T loopvalue in ChooseArray)
            {
                double rgg;
                do
                {
                    rgg = rgen.NextDouble();
                }
                while (sorttest.ContainsKey(rgg));
                sorttest.Add(rgg, loopvalue);

            }

            //return the first item.
            return sorttest.First().Value;
        }
        private void SpawnNewTetromino()
        {
            Func<BlockGroup> GetTetrominoFunction;
            Func<BlockGroup>[] GeneratorFunctions = new Func<BlockGroup>[]
            {
                BlockGroup.GetTetromino_Z,
                BlockGroup.GetTetromino_I,
                BlockGroup.GetTetromino_J,
                BlockGroup.GetTetromino_L,
                BlockGroup.GetTetromino_O,
                BlockGroup.GetTetromino_S,
                BlockGroup.GetTetromino_T
            };
            GetTetrominoFunction = Choose(GeneratorFunctions);
            //GetTetrominoFunction = BlockGroup.GetTetromino_T;
            BlockGroup newTetromino = GetTetrominoFunction();
            newTetromino.X = 5 - newTetromino.GroupExtents.Width / 2;
            newTetromino.Y = 0;
            PlayField.AddBlockGroup(newTetromino);

        }
        public override void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            if(PlayField!=null)
            {
                PlayField.Draw(g,Bounds);
            }
        }
        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            if (g == GameKeys.GameKey_RotateCW)
            {
                foreach (var activeitem in PlayField.BlockGroups)
                {
                    if (PlayField.CanRotate(activeitem, false))
                    {
                        activeitem.Rotate(false);
                        activeitem.Clamp(PlayField.RowCount, PlayField.ColCount);
                    }
                }
            }
            else if (g == GameKeys.GameKey_Down)
            {
                foreach (var activeitem in PlayField.BlockGroups)
                {
                    if (MoveGroupDown(activeitem))
                    {

                        PlayField.ProcessLines();

                    }
                }
            }
            else if(g==GameKeys.GameKey_Right || g==GameKeys.GameKey_Left)
            {
                int XMove = g == GameKeys.GameKey_Right ? 1 : -1;
                foreach (var ActiveItem in PlayField.BlockGroups)
                {
                    if (PlayField.CanFit(ActiveItem, ActiveItem.X + XMove, ActiveItem.Y))
                    {
                        lastHorizontalMove = DateTime.Now;
                        ActiveItem.X += XMove;
                    }
                }
            }
        }
        private bool MoveGroupDown(BlockGroup activeItem)
        {

            if (PlayField.CanFit(activeItem, activeItem.X, activeItem.Y + 1))
            {
                activeItem.Y++;
            }
            else
            {
                if ((DateTime.Now - lastHorizontalMove).TotalMilliseconds > 250)
                {
                    PlayField.SetGroupToField(activeItem);
                    return true;
                }
            }
            return false;
        }
    }
}
