using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris
{
    public class GameComposite
    {
        //class composites several elements for running a game.
        //Might make sense for this to be integrated into TetrisGame instead.
        private StandardSettings _GameSettings = null;
        private List<GameObject> _GameObjects = new List<GameObject>();
        private TetrisGame _Game;

        public StandardSettings GameSettings { get { return _GameSettings; } set { _GameSettings = value; } }
        public List<GameObject> GameObjects {  get { return _GameObjects; } set { _GameObjects = value; } }
        public TetrisGame Game {  get { return _Game; } set { _Game = value; } }
    }
}
