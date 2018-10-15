using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris
{
    public interface IStateOwner
    {
        void SetDisplayMode(GameState.DisplayMode pMode);
        GameState CurrentState { get; set; }
        void EnqueueAction(Action pAction);
        Rectangle GameArea { get; }
        void Feedback(float Strength, int Length);

        void AddGameObject(GameObject Source);
        double ScaleFactor { get; }
        void SetScale(double pScale);
    }
}