using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris
{
    /// <summary>
    /// Game Presenter implements the base logic that is above TetrisGame itself, but "below" the ultimate "UI" layer involved in handling keys or rendering.
    /// The overall idea is that whatever layer is being used will pass key events to this class, which will handle the Game Logic involved in those keys, and
    /// then the drawing can be dealt with appropriately based on the UI layer, using the Rendering Provider/Handling design for whatever target "canvas" type is being used.
    /// </summary>
    public class GamePresenter
    {
        
        private StandardSettings _GameSettings = null;
        private List<GameObject> _GameObjects = new List<GameObject>();
        private TetrisGame _Game;
        public StandardSettings GameSettings {  get { return _GameSettings; } set { _GameSettings = value; } }
        public List<GameObject> GameObjects {  get { return _GameObjects; } set { _GameObjects = value; } }
        public TetrisGame Game {  get { return _Game; } set { _Game = value; } }
    }
}
