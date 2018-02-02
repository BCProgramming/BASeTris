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
        GameState CurrentState { get; set; }
        void EnqueueAction(Action pAction);
        Rectangle GameArea { get; }
        void Feedback(float Strength,int Length);

        void AddGameObject(GameObject Source);
        void AddParticle(Particle pParticle);
    }
}
