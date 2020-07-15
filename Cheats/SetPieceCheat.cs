using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Choosers;
using BASeTris.GameStates;
using BASeTris.Tetrominoes;

namespace BASeTris.Cheats
{
    class SetPieceCheat : Cheat
    {
        public override string DisplayName { get { return "SetPieceCheat"; } }
        public override string CheatName { get { return "setpiece"; } }
        public override bool CheatAction(IStateOwner pStateOwner, string[] CheatParameters)
        {
            if (CheatParameters.Any())
            {
                String sPiece = CheatParameters[0];

                Func<Nomino> buildNominoFunc = null;
                switch(sPiece)
                {
                    case "I":
                        buildNominoFunc = ()=>new Tetromino_I();
                        break;
                    case "J":
                        buildNominoFunc = () => new Tetromino_J();
                        break;
                    case "S":
                        buildNominoFunc = () => new Tetromino_S();
                        break;
                    case "Z":
                        buildNominoFunc = () => new Tetromino_Z();
                        break;
                    case "T":
                        buildNominoFunc = () => new Tetromino_T();
                        break;
                    case "O":
                        buildNominoFunc = () => new Tetromino_O();
                        break;
                    case "BAG":
                        if (pStateOwner.CurrentState is GameplayGameState stdstate)
                        {
                            stdstate.Chooser = new BagChooser(Tetromino.StandardTetrominoFunctions);

                            stdstate.NextBlocks.Clear();
                            stdstate.RefillBlockQueue();

                            return true;

                        }
                        break;
                    default:
                    

                        return false;
                }



                
                if(pStateOwner.CurrentState is GameplayGameState mainstate)
                {
                    mainstate.Chooser = new NESChooser(new Func<Nomino>[] { buildNominoFunc });

                    mainstate.NextBlocks.Clear();
                    mainstate.RefillBlockQueue();
                    return true;

                }


            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
