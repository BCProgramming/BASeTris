using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.BASeCamp;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BASeTris.Cheats
{
    public abstract class Cheat
    {
        public static LoadedTypeManager CheatManager = null;

        private static Dictionary<String, Cheat> CheatDictionary = new Dictionary<String, Cheat>(StringComparer.OrdinalIgnoreCase);
        static Cheat()
        {
            CheatManager = new LoadedTypeManager(new Assembly[] { Assembly.GetExecutingAssembly() },typeof(Cheat),null);
            foreach(var iterate in CheatManager.ManagedTypes)
            {
                ConstructorInfo findconstructor = iterate.GetConstructor(new Type[] { });
                if(findconstructor!=null)
                {
                    Cheat CheatInstance = (Cheat)findconstructor.Invoke(new object[] { });
                    CheatDictionary.Add(CheatInstance.CheatName,CheatInstance);
                }
            }
        }
        public static bool ProcessCheat(String[] cheattext, IStateOwner pStateOwner)
        {
            bool AllTrue = true;
            foreach(String sCheat in cheattext)
            {
                AllTrue &= ProcessCheat(sCheat, pStateOwner);
            }
            return AllTrue;
        }
        public static Cheat GetCheat(String pCheatName)
        {
            if (CheatDictionary.ContainsKey(pCheatName))
                return CheatDictionary[pCheatName];

            return null;
        }
        public static bool ProcessCheat(String cheattext,IStateOwner pStateOwner)
        {
            //same recursive definition for semicolons as well.
            if (cheattext.Contains(";"))
            {
                return ProcessCheat(cheattext.Split(';'),pStateOwner);

            }
            String[] splitcheat = cheattext.Split(' ');
            Cheat acquirecheat = Cheat.GetCheat(splitcheat[0]);
            if (acquirecheat == null) return false;
            //remove the first element from the string array.
            splitcheat = new List<String>(splitcheat.Skip(1)).ToArray();
            //call the cheat we acquired.
            return acquirecheat.CheatAction(pStateOwner, splitcheat);
        }

        public abstract String DisplayName { get; }
        public abstract String CheatName { get; }


        public abstract bool CheatAction(IStateOwner pStateOwner,String[] CheatParameters);
        

    }
    
    public class NullCheat :Cheat
    {
        public override string DisplayName { get { return " Null Cheat. Performs no action"; } }
        public override string CheatName { get{ return "NULL"; } }
        public override bool CheatAction(IStateOwner pStateOwner, string[] CheatParameters)
        {
            return true;
        }
    }
}
