using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BASeCamp.BASeScores;
using BASeCamp.Elementizer;

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
            TotalLines = InitializationData.LineCount;
            LevelReachedTimes = InitializationData.LevelTimes;
            TetronimoPieceCounts = InitializationData.GetPieceCounts();

        }

        public TetrisHighScoreData(XElement Source,object PersistenceData)
        {
            if(Source.HasElements)
            {
                XElement LevelTimeNode = Source.Element("LevelTimes");
                XElement PieceCountsNode = Source.Element("PieceCounts");
                TotalLines = Source.GetAttributeInt("Lines", 0);
                LevelReachedTimes = (TimeSpan[])StandardHelper.ReadArray<TimeSpan>(LevelTimeNode, PersistenceData);
                TetronimoPieceCounts = StandardHelper.ReadDictionary<String, int>(PieceCountsNode, PersistenceData);
            }
        }
        public XElement GetXmlData(string pNodeName, object PersistenceData)
        {
            XElement BuildNode = new XElement(pNodeName);
            BuildNode.Add(new XAttribute("Lines",TotalLines));
            XElement LevelTimeNode = StandardHelper.SaveArray(LevelReachedTimes, "LevelTimes", PersistenceData);
            XElement PieceCountNode = StandardHelper.SaveDictionary(TetronimoPieceCounts, "PieceCounts");
            BuildNode.Add(LevelTimeNode,PieceCountNode);
            return BuildNode;
        }
    }
    
}
