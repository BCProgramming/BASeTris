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
                Func<Nomino>[] buildNominoFuncs = null;
                switch (sPiece)
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
                            stdstate.SetChooser(new BagChooser(Tetromino.StandardTetrominoFunctions));

                            stdstate.NextBlocks.Clear();
                            stdstate.RefillBlockQueue(pStateOwner);

                            return true;

                        }
                        break;
                    
                    default:
                        if (sPiece.StartsWith("LETTER",StringComparison.OrdinalIgnoreCase))
                        {
                            String sSubstring = sPiece.Substring(6).Trim().ToUpper();


                            Func<Nomino>[] letterfuncs = (from c in sSubstring select new Func<Nomino>(() => NNominoGenerator.NominoFromLetter(c.ToString()))).ToArray();

                            buildNominoFuncs = letterfuncs;


                            


                            
                        }
                        else if (sPiece.StartsWith("P", StringComparison.OrdinalIgnoreCase))
                        {
                            String sNominoText = String.Join(" ", CheatParameters).Substring(1);
                            sNominoText = sNominoText.Replace("L", "\n");
                            buildNominoFunc = () => NNominoGenerator.CreateNomino(NNominoGenerator.FromString(sNominoText).ToList());

                        }
                        else
                        {

                            return false;
                        }
                        break;
                }



                
                if(pStateOwner.CurrentState is GameplayGameState mainstate)
                {
                    if (buildNominoFunc != null)
                    {
                        mainstate.SetChooser(new SingleFunctionChooser(buildNominoFunc));
                    }
                    else if(buildNominoFuncs !=null)
                    {
                        mainstate.SetChooser(new SequentialChooser(buildNominoFuncs));
                    }
                    mainstate.NextBlocks.Clear();
                    mainstate.RefillBlockQueue(pStateOwner);
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
