﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BASeCamp.BASeScores;
using BASeCamp.Elementizer;

namespace BASeTris
{
    public abstract class BaseHighScoreData : IHighScoreEntryCustomData
    {
        public virtual GameplayRecord ReplayData { get; set; }


        public BaseHighScoreData(GameplayRecord pReplayData)
        {
            ReplayData = pReplayData;
        }
        public BaseHighScoreData(XElement Source, object PersistenceData)
        {
            var FindElement = Source.Element("ReplayData");
            if (FindElement != null)
            {
                ReplayData = new GameplayRecord(FindElement, PersistenceData);
            }
            else
            {
                //no replay data...
            }
        }
        public abstract XElement GetHighScoreXMLData(String pNodeName, object pPersistenceData);
        
        public XElement GetXmlData(string pNodeName, object PersistenceData)
        {

            var result = ReplayData==null?null:ReplayData.GetXmlData(pNodeName, PersistenceData);
            if (ReplayData != null)
            {
                result.Add("ReplayData", ReplayData.GetXmlData(pNodeName, PersistenceData));
            }
            return result;

        }
    }
    public class DrMarioHighScoreData : BaseHighScoreData
    {

        public Dictionary<int, int> BlocksDestroyedCounts = new Dictionary<int, int>();
        public Dictionary<int, int> VirusDestroyedCounts = new Dictionary<int, int>();
        public DrMarioHighScoreData(XElement Source, object PersistenceData) : base(Source, PersistenceData)
        {
            //
        }
        public override XElement GetHighScoreXMLData(string pNodeName, object PersistenceData)
        {
            return new XElement("Dr. Mario");
            //throw new NotImplementedException();
        }
    }
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
    public class TetrisHighScoreData : BaseHighScoreData
    {
        public TimeSpan[] LevelReachedTimes = new TimeSpan[] {TimeSpan.Zero};
        public int TotalLines { get; set; }
        public Dictionary<String, int> TetronimoPieceCounts = new Dictionary<string, int>(); //this is indexed by the GetType().Name value of the tetronimo instance(s) in question.
        public Dictionary<String, int> TetronimoLineCounts = new Dictionary<string, int>();

        /// <summary>
        /// Initializes this High Score information from the information in a Statistics class.
        /// The statistics class is used while playing through a game, and this will represent the "final" state of a game when it is over, so we can use it for that purpose here.
        /// 
        /// </summary>
        /// <param name="InitializationData"></param>
        /// 
        //todo: this needs to bring in the actual replay data...
        public TetrisHighScoreData(TetrisStatistics InitializationData,GameplayRecord InputRecord):base(InputRecord)
        {
            if (InitializationData == null)
            {
                LevelReachedTimes = new TimeSpan[] { };
                TetronimoPieceCounts = new Dictionary<string, int>();
                TotalLines = 0;
            }
            else
            {
                TotalLines = InitializationData.LineCount;
                LevelReachedTimes = InitializationData.LevelTimes;
                TetronimoPieceCounts = InitializationData.GetPieceCounts();
                TetronimoLineCounts = InitializationData.GetLineCounts();
            }
        }

        public TetrisHighScoreData(XElement Source, object PersistenceData):base(Source,PersistenceData)
        {
            if (Source.HasElements)
            {
                XElement LevelTimeNode = Source.Element("LevelTimes");
                XElement PieceCountsNode = Source.Element("PieceCounts");
                TotalLines = Source.GetAttributeInt("Lines", 0);
                if (LevelTimeNode != null)
                {
                    Dictionary<int, TimeSpan> ConstructListing = new Dictionary<int, TimeSpan>();
                    foreach (XElement loopelement in LevelTimeNode.Elements("LevelTime"))
                    {
                        int Level = loopelement.GetAttributeInt("Level");
                        long ticks = loopelement.GetAttributeLong("Time");
                        if (!ConstructListing.ContainsKey(Level))
                        {
                            ConstructListing.Add(Level, new TimeSpan(ticks));
                        }
                    }

                    LevelReachedTimes = (from s in ConstructListing orderby s.Key select s.Value).ToArray();
                }
                else
                    LevelReachedTimes = new TimeSpan[] { };

                if (PieceCountsNode != null)
                {
                    TetronimoPieceCounts = new Dictionary<string, int>();
                    TetronimoLineCounts = new Dictionary<string, int>();
                    foreach (var NodePiece in PieceCountsNode.Elements("PieceCount"))
                    {
                        String sPiece = NodePiece.GetAttributeString("Piece");
                        int iCount = NodePiece.GetAttributeInt("Count");
                        int pieceLines = NodePiece.GetAttributeInt("Lines");
                        TetronimoPieceCounts.Add(sPiece, iCount);
                        TetronimoLineCounts.Add(sPiece, pieceLines);
                    }
                }
            }
        }

        public override XElement GetHighScoreXMLData(string pNodeName, object PersistenceData)
        {
            XElement BuildNode = new XElement(pNodeName);
            BuildNode.Add(new XAttribute("Lines", TotalLines));
            if (LevelReachedTimes.Length > 0)
            {
                XElement LevelTimesNode = new XElement("LevelTimes");
                for (int leveltime = 0; leveltime < LevelReachedTimes.Length; leveltime++)
                {
                    XElement LevelTimeNode = new XElement("LevelTime", new XAttribute("Level", leveltime), new XAttribute("Time", LevelReachedTimes[leveltime].Ticks));
                    LevelTimesNode.Add(LevelTimeNode);
                }

                //XElement LevelTimeNode = StandardHelper.SaveArray(LevelReachedTimes, "LevelTimes", PersistenceData);
                BuildNode.Add(LevelTimesNode);
            }

            XElement PieceCountsNode = new XElement("PieceCounts");
            foreach (var kvp in TetronimoPieceCounts)
            {
                XElement PieceCountNode = new XElement("PieceCount", new XAttribute("Piece", kvp.Key), new XAttribute("Count", kvp.Value), new XAttribute("Lines", TetronimoLineCounts[kvp.Key]));
                PieceCountsNode.Add(PieceCountNode);
            }

            //XElement PieceCountNode = StandardHelper.SaveDictionary(TetronimoPieceCounts, "PieceCounts");
            BuildNode.Add(PieceCountsNode);
            return BuildNode;
        }
    }
}