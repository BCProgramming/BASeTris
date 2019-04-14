using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Cheats;

namespace BASeTris.GameStates
{
    public class EnterCheatState : EnterTextState
    {
        GameState _PreviousState = null;

        public EnterCheatState(GameState pOriginalState, IStateOwner pOwner, int EntryLength, string PossibleChars = " _ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890") : base(pOwner, EntryLength, PossibleChars)
        {
            _PreviousState = pOriginalState;
            EntryPrompt = new string[] {"Enter Cheat Code"};
        }

        public override bool ValidateEntry(IStateOwner pOwner, string sCurrentEntry)
        {
            return true;
        }

        public override void CommitEntry(IStateOwner pOwner, string sCurrentEntry)
        {
            pOwner.CurrentState = _PreviousState;
            if (Cheat.ProcessCheat(sCurrentEntry, pOwner))
            {
                TetrisGame.Soundman.PlaySound("right", pOwner.Settings.EffectVolume);
            }
            else
            {
                TetrisGame.Soundman.PlaySound("wrong", pOwner.Settings.EffectVolume);
            }
        }
    }
}