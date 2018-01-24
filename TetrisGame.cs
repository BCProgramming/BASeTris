using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris
{
    public class TetrisGame : IStateOwner
    {
        private GameState CurrentGameState = null;
        private IStateOwner GameOwner = null;
        public TetrisGame(IStateOwner pOwner)
        {
            GameOwner = pOwner;
            CurrentGameState = new StandardTetrisGameState();
        }

        public GameState CurrentState { get{ return CurrentGameState; } set{ CurrentGameState = value; } }
        public void EnqueueAction(Action pAction)
        {
            GameOwner.EnqueueAction(pAction);
        }
        public void GameProc()
        {
            CurrentGameState.GameProc(GameOwner);
        }
        public void HandleGameKey(IStateOwner pOwner, GameState.GameKeys g)
        {
            CurrentGameState.HandleGameKey(pOwner,g);
        }
        public void DrawProc(Graphics g, RectangleF Bounds)
        {
            CurrentGameState.DrawProc(this,g,Bounds);
        }
    }
}
