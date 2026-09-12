using BASeTris.BackgroundDrawers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeCamp.StateLib
{
    //refactoring the gamestates AND as much menu as I can into a separate BCGameLib library, so it can be shared (instead of me rewriting it over again each time for each game...)
    //Step one is to tease out the coupling first, though.

    //Most of the coupling seems to be related to things like fonts- calls to TetrisGame to get things like the RetroFont information.
    public abstract class BaseGameState<TStateOwner, TGameKey, TGameObject>
    {
        public BaseGameState()
        {
            FriendlyNames = GetFriendlyKeyNames(); //initialize from derived class, it should know the actual gamekey type.
        }

        private List<TGameObject> GameObjects = new List<TGameObject>();

        public void AddGameObject(TGameObject Item)
        {
            lock (GameObjects)
                GameObjects.Add(Item);
        }
        public void RemoveGameObject(TGameObject Item)
        {
            lock (GameObjects)
                GameObjects.Remove(Item);
        }
        public IEnumerable<TGameObject> AllGameObjects()
        {
            return GameObjects.AsReadOnly();
        }
        private ConcurrentDictionary<String, Object> CustomStateProperties = new ConcurrentDictionary<string, object>();

        public Object GetCustomProperty(String pKey)
        {
            return CustomStateProperties?[pKey];
        }
        public bool HasCustomProperty(String pKey)
        {
            return CustomStateProperties.ContainsKey(pKey);
        }
        public void SetCustomProperty(String pKey, Object Value)
        {
            CustomStateProperties[pKey] = Value;
        }
        public virtual bool GameProcSuspended { get; set; } = false;
        /// <summary>
        /// Indicates whether this GameState allows active play. This is used to determine certain actions such as movement interpolation.
        /// </summary>
        public virtual bool GamePlayActive { get { return false; } }



        private static Dictionary<TGameKey, String> FriendlyNames = null;
        protected abstract Dictionary<TGameKey, String> GetFriendlyKeyNames();


        public static String GetGameKeyFriendlyName(TGameKey src)
        {
            return FriendlyNames[src];
        }

        /*[Obsolete("Use Rendering Providers")]
        public abstract void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds);*/
        public abstract void GameProc(TStateOwner pOwner);

        //Obbsolete("Use Rendering Providers")]
        /*public abstract void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds);*/
        public abstract void HandleGameKey(TStateOwner pOwner, TGameKey g);

        //[Obsolete("This method will be removed eventually... the rendering adapters are where this sort of behaviour should be.")]
        //public abstract void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds);

        protected IBackground _BG;
        public IBackground BG
        {
            get => _BG;
            set { _BG = value; }
        }

        /// <summary>
        /// called to validate a user-initiated key, and whether it is allowed. A good example is that when there is a replay in progress, only the pause key should be allowed by the gameplay state.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool AllowUserGameKey(TGameKey key) { return true; }

    }
}
