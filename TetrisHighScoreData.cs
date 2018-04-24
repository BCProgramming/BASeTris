using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BASeCamp.BASeScores;

namespace BASeTris
{
    /// <summary>
    /// holds extra score information for the High score info.
    /// </summary>
    ///
    
    //Information tracked:
    //The number of lines
    //level reached
    //The number of tetronimoes that were used
    //the number of lines cleared by tetronimoes
    //the elapsed time that each level was achieved

    //(Other possibility: What if we track each dropped block, such that we could "replay" a game in some capacity- we could put that here...)
    //That might benefit from other architectural changes involving random seeds, too...
    public class TetrisHighScoreData: IHighScoreEntryCustomData
    {
        private TimeSpan[] LevelReachedTimes = new TimeSpan[]{TimeSpan.Zero};
        public int TotalLines { get; set; }
        private Dictionary<String, int> TetronimoPieceCounts = new Dictionary<string, int>(); //this is indexed by the GetType().Name value of the tetronimo instance(s) in question.
        /// <summary>
        /// Initializes this High Score information from the information in a Statistics class.
        /// The statistics class is used while playing through a game, and this will represent the "final" state of a game when it is over, so we can use it for that purpose here.
        /// 
        /// </summary>
        /// <param name="InitializationData"></param>
        public TetrisHighScoreData(Statistics InitializationData)
        {

        }

        public TetrisHighScoreData(XElement Source,object PersistenceData)
        {

        }
        public XElement GetXmlData(string pNodeName, object PersistenceData)
        {
            throw new NotImplementedException();
        }
    }
    
}
