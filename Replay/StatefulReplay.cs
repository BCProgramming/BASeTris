using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BASeCamp.Elementizer;
using BASeTris.Blocks;
using BASeTris.GameStates;

namespace BASeTris.Replay
{
    /// <summary>
    /// A 'stateful' replay records the state of the game board. This records board state paired with elapsed time when changes occur.
    /// Current suggestion: Create a state for the board when a block group is "solidified", and after rows are cleared.
    /// </summary>
    public class StatefulReplay : IXmlPersistable
    {
        //Replay Data is indexed by Elapsed Time. Usually, there is only one
        //for a specific TimeSpan. However, we don't want to crash or have issues dealing with multiple recorded states on the same TimeSpan.
        //Therefore the value here is a List of ReplayStates, rather than a ReplayState itself.

        Dictionary<TimeSpan, List<StatefulReplayState>> ReplayData = new Dictionary<TimeSpan, List<StatefulReplayState>>();


        public XElement GetXmlData(String pNodeName, Object data)
        {
            XElement createNode = new XElement(pNodeName);

            return createNode;
        }

        public StatefulReplay(XElement src, Object data)
        {
        }
        public StatefulReplay()
        {

        }
        /// <summary>
        /// Creates a Replay State given a State owner and a standard game state. returns the resulting replay state.
        /// </summary>
        /// <param name="pOwner"></param>
        /// <param name="SourceState"></param>
        /// <returns></returns>
        public StatefulReplayState CreateReplayState(IStateOwner pOwner, GameplayGameState SourceState)
        {
            StatefulReplayState newState = new StatefulReplayState(pOwner, SourceState);
            if (!ReplayData.ContainsKey(newState.ElapsedGameTime))
            {
                ReplayData.Add(newState.ElapsedGameTime, new List<StatefulReplayState>());
            }
            ReplayData[newState.ElapsedGameTime].Add(newState);
            return newState;
        }
        public void WriteStateImages(String TargetDirectory)
        {
            foreach(var iterateState in ReplayData)
            {
                var LastItem = iterateState.Value.LastOrDefault();
                if(LastItem!=null)
                {
                    Image FieldBmp = LastItem.DrawFieldBitmap();
                    String sImageName = Path.Combine(TargetDirectory, LastItem.ElapsedGameTime.Ticks + ".png");
                    

                }
            }



        }

        public StatefulReplayState GetStateForTime(TimeSpan GameTime)
        {
            var GetState = (from r in ReplayData where r.Key < GameTime orderby r.Key descending select r).FirstOrDefault();
            return GetState.Value.Last();
        }
        public StatefulReplayState GetStateAtIndex(int index)
        {
            try
            {
                var useKey = ReplayData.Keys.ElementAt(index);
                return ReplayData[useKey].Last();
            }
            catch (IndexOutOfRangeException exx)
            {
                return null;
            }
        }

        public class StatefulReplayState : IXmlPersistable
        {

            //represents a single state of a stateful replay.
            //Elapsed game time at the point this Replay state was created.
            public TimeSpan ElapsedGameTime;
            public int Rows;
            public int Columns;
            public int MagicNumber = 0x4332231;
            public StatefulReplayStateBlockInformation[][] BoardState = null;

            public StatefulReplayState(XElement Source, Object pData)
            {
                int NumRows = Source.GetAttributeInt("Rows");
                int NumCols = Source.GetAttributeInt("Columns");
                long NumTicks = Source.GetAttributeLong("Ticks");
                ElapsedGameTime = TimeSpan.FromTicks(NumTicks);
                Rows = NumRows;
                Columns = NumCols;
                BoardState = new StatefulReplayStateBlockInformation[Rows][];
                var CellElements = Source.Elements("Cell").ToArray();
                int ElementIndex = 0;
                for (int r = 0; r < Rows; r++)
                {
                    BoardState[r] = new StatefulReplayStateBlockInformation[Columns];
                    for (int c = 0; c < Columns; c++)
                    {
                        var CurrCell = CellElements[ElementIndex];
                        
                        BoardState[r][c] = new StatefulReplayStateBlockInformation(CurrCell, null);
                    }
                }

            }
            public XElement GetXmlData(String pNodeName, Object Context)
            {
                XElement BuildNode = new XElement(pNodeName);

                //store the size of the Board as attributes.
                int NumRows = BoardState.Length;
                int NumColumns = BoardState[0].Length;
                BuildNode.Add(new XAttribute("Rows", NumRows), new XAttribute("Columns", NumColumns),new XAttribute("Elapsed",ElapsedGameTime.Ticks));
                for (int r = 0; r < BoardState.Length; r++)
                {
                    for (int c = 0; c < BoardState.Length; c++)
                    {
                        var currcell = BoardState[r][c];
                        if (currcell.State == StatefulReplayStateBlockInformation.BlockInformation.Block_Empty) continue;
                        XElement CellNode = new XElement("Cell", new XAttribute("Content", currcell.State.ToString()));
                        if (currcell.BlockData != null)
                        {
                            XElement DataNode = currcell.BlockData.GetXmlData("BlockData", Context);
                            CellNode.Add(DataNode);
                        }
                        BuildNode.Add(CellNode);
                    }
                }


                return BuildNode;
            }
            /// <summary>
            /// reads ReplayState from a Stream.
            /// The specified stream is left in the position after the replay data.
            /// </summary>
            /// <param name="Source"></param>
            public StatefulReplayState(Stream Source)
            {
                if (!Source.CanWrite) throw new InvalidOperationException("Cannot read from unreadable stream");
                BinaryReader br = new BinaryReader(Source);
                int readMagic = br.ReadInt32();
                long readTicks = br.ReadInt64();
                ElapsedGameTime = new TimeSpan(readTicks);
                if (readMagic != MagicNumber) throw new InvalidDataException("Stream does not contain valid Replay Information.");

            }
            public Image DrawFieldBitmap()
            {
                Bitmap BuildBitmap = new Bitmap(Columns,Rows);
                for(int x=0;x<Columns;x++)
                {
                    for(int y=0;y<Rows;y++)
                    {
                        var grabstate = BoardState[y][x];
                        Color useColor = Color.White;
                        switch(grabstate.State)
                        {
                            case StatefulReplayStateBlockInformation.BlockInformation.Block_Active:
                                useColor = Color.Blue;
                                break;
                            case StatefulReplayStateBlockInformation.BlockInformation.Block_Occupied:
                                useColor = Color.Black;
                                break;
                            default:
                                useColor = Color.White;
                                break;
                        }   
                    }
                }
                return BuildBitmap;
            }
            //writes the data in this Replay State to a Stream.
            public void WriteToStream(Stream Target)
            {
                if (!Target.CanWrite) throw new InvalidOperationException("Cannot write to unwritable stream");

                BinaryWriter bw = new BinaryWriter(Target);
                bw.Write(MagicNumber);
                bw.Write(ElapsedGameTime.Ticks);
                bw.Write(Rows);
                bw.Write(Columns);
                for (int y = 0; y < Rows; y++)
                {
                    for (int x = 0; x < Columns; x++)
                    {
                        StatefulReplayStateBlockInformation bi = BoardState[y][x];
                        bw.Write((int)bi.State);
                    }
                }

            }
            public StatefulReplayState(IStateOwner pOwner, GameplayGameState Source)
            {
                var Field = Source.PlayField;
                Rows = Field.RowCount;
                Columns = Field.ColCount;
                ElapsedGameTime = pOwner.GetElapsedTime();
                BoardState = new StatefulReplayStateBlockInformation[Field.RowCount][];
                for (int y = 0; y < Field.RowCount; y++)
                {
                    BoardState[y] = new StatefulReplayStateBlockInformation[Field.ColCount];
                    for(int x=0;x<BoardState[y].Length;x++)
                    {
                        BoardState[y][x] = new StatefulReplayStateBlockInformation(StatefulReplayStateBlockInformation.BlockInformation.Block_Empty,null);
                    }
                }

                //step one: set the Active Block positions.
                foreach (var activegroup in Source.PlayField.BlockGroups)
                {
                    int XGroup = activegroup.X;
                    int YGroup = activegroup.Y;
                    foreach (var iterateblock in activegroup)
                    {
                        int BlockX = iterateblock.X + XGroup;
                        int BlockY = iterateblock.Y + YGroup;
                        BoardState[BlockY][BlockX] = new StatefulReplayStateBlockInformation(StatefulReplayStateBlockInformation.BlockInformation.Block_Active,iterateblock.GetType().FullName);
                    }
                }
                //step two: occupied block positions.
                for (int CurrentRow = 0; CurrentRow < Source.PlayField.Contents.Length; CurrentRow++)
                {
                    for (int CurrentCol = 0; CurrentCol < Source.PlayField.Contents[CurrentRow].Length; CurrentCol++)
                    {
                        var thisBlock = Source.PlayField.Contents[CurrentRow][CurrentCol];

                        if (thisBlock != null)
                        {
                            BoardState[CurrentRow][CurrentCol].State = StatefulReplayStateBlockInformation.BlockInformation.Block_Occupied;
                            BoardState[CurrentRow][CurrentCol].BlockTypeName = thisBlock.GetType().FullName;
                            if (thisBlock is LineSeriesBlock lsb)
                            {
                                BoardState[CurrentRow][CurrentCol].BlockData = new LineSeriesBlockSpecificInfo(thisBlock as LineSeriesBlock);
                            }
                        }
                        else
                        {
                            BoardState[CurrentRow][CurrentCol].State = StatefulReplayStateBlockInformation.BlockInformation.Block_Empty;
                        }

                    }
                }
            }


        }
        public class StatefulReplayStateBlockInformation : IXmlPersistable
        {
            public enum BlockInformation
            {
                Block_Empty,
                Block_Occupied,
                Block_Active
            }
            public BlockInformation State { get; set; }

            public String BlockTypeName { get; set; }
            public BlockSpecificInfo BlockData { get; set; }
            public StatefulReplayStateBlockInformation(BlockInformation pState,String pBlockTypeName)
            {
                BlockTypeName = pBlockTypeName;
                State = pState;
            }
            public StatefulReplayStateBlockInformation(XElement pNode, Object PersistenceData)
            {
                State = (BlockInformation)pNode.GetAttributeInt("State");
                BlockTypeName = pNode.GetAttributeString("BlockTypeName", "");
                if (BlockTypeName.Equals(typeof(LineSeriesBlock).GetType().FullName))
                {
                    var BlockItem = pNode.Element("BlockData");
                    LineSeriesBlockSpecificInfo info = new LineSeriesBlockSpecificInfo(BlockItem, PersistenceData);
                    BlockData = info;
                }
            }
            public XElement GetXmlData(String pNodeName, object PersistenceData)
            {
                XElement BlockNode = BlockData.GetXmlData("BlockData", PersistenceData);
                return new XElement(pNodeName, BlockNode, new XAttribute("BlockTypeName", BlockTypeName), new XAttribute("State", State));
            }
            
        }
        public abstract class BlockSpecificInfo : IXmlPersistable
        {
            public abstract XElement GetXmlData(string pNodeName, object PersistenceData);
            
        }
        public class LineSeriesBlockSpecificInfo : BlockSpecificInfo
        {
            public int CombiningIndex { get; set; }
            public LineSeriesBlockSpecificInfo(Blocks.LineSeriesBlock nb)
            {
                CombiningIndex = (int)nb.CombiningIndex;
            }
                
            public override XElement GetXmlData(String pNodeName, object PersistenceData)
            {
                return new XElement("LineSeriesBlockData", new XAttribute("CombiningIndex", CombiningIndex));
            }
            public LineSeriesBlockSpecificInfo(XElement pNode, Object PersistenceData)
            {

                CombiningIndex = pNode.GetAttributeInt("CombiningIndex");

            }
        }
    }
}
