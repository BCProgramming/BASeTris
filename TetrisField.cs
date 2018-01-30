using BASeTris.TetrisBlocks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BASeTris
{
    public class TetrisField
    {
        //standard tetris field is 10 blocks wide, and 22 blocks tall- (the top 2 rows aren't drawn usually).
        //The "tetris field" here just represents the "permanent" blocks. eg, blocks that have been dropped, and "set" in place.
        //these get evaluated to check for lines when a new block is placed, for example.
        //blocks that are still "controlled" and falling are handled separately, and are placed here when they are dropped.
        public static String StandardMusic = "tetris_theme_A";
        public event EventHandler<LevelChangeEventArgs> LevelChanged;
        public event EventHandler<BlockGroupSetEventArgs> BlockGroupSet;
        private TetrisBlock[][] FieldContents;
        public long LineCount = 0;
        public TetrominoTheme Theme = new NESTetrominoTheme();
        private List<BlockGroup> ActiveBlockGroups = new List<BlockGroup>();
        public int Level {  get { return (int)LineCount / 10; } }
        const int ROWCOUNT = 22;
        const int COLCOUNT = 10;
        const int VISIBLEROWS = 20;
        public int HIDDENROWS {  get { return ROWCOUNT - VISIBLEROWS; } }
        //const int ROWCOUNT = 44;
        //const int COLCOUNT = 20;
        Random rg = new Random();
        public int RowCount {  get { return ROWCOUNT; } }
        public int ColCount {  get { return COLCOUNT; } }
        public IList<BlockGroup> BlockGroups { get { return new List<BlockGroup>(ActiveBlockGroups); } }

        
        public TetrisBlock[][] Contents {  get { return FieldContents; } }
        public void ClearContents()
        {
            ActiveBlockGroups.Clear();
            foreach(var row in FieldContents)
            {
                for(int i=0;i<COLCOUNT;i++)
                {
                    row[i] = null;
                }
            }
          
        }
        public void AddBlockGroup(BlockGroup newGroup)
        {
            Debug.Print("Added:" + newGroup.ToString());
            ActiveBlockGroups.Add(newGroup);
            
        }
        public void RemoveBlockGroup(BlockGroup oldGroup)
        {
            ActiveBlockGroups.Remove(oldGroup);
        }
        public TetrisField(int GarbageRows=0)
        {
            FieldContents = new TetrisBlock[ROWCOUNT][];
            for(int row=0;row<ROWCOUNT;row++)
            {
                FieldContents[row] = new TetrisBlock[COLCOUNT];
            }
            if(GarbageRows > 0)
            {
                for(int i=0;i<GarbageRows;i++)
                {
                    var FillRow = FieldContents[ROWCOUNT - i-1];
                    for(int fillcol=0;fillcol<COLCOUNT;fillcol++)
                    {
                        if(rg.NextDouble()>0.5)
                        {
                            var standardfilled = new StandardColouredBlock();
                            standardfilled.BlockColor = Color.FromArgb(rg.Next(255), rg.Next(255), rg.Next(255));
                            FillRow[fillcol] = standardfilled;
                            
                        }
                    }


                }
            }
        }
        public void AnimateFrame()
        {
            for (int drawRow = HIDDENROWS; drawRow < ROWCOUNT; drawRow++)
            {
                var currRow = FieldContents[drawRow];
                //for each Tetris Row...
                for (int drawCol = 0; drawCol < COLCOUNT; drawCol++)
                {

                    var TetBlock = currRow[drawCol];
                    if (TetBlock != null)
                    {

                        if (TetBlock.IsAnimated)
                        {
                            TetBlock.AnimateFrame();
                        }

                    }

                }
            }
            foreach(var bg in ActiveBlockGroups)
            {
                foreach(var iterateblock in bg)
                {
                    if(iterateblock.Block!=null && iterateblock.Block.IsAnimated)
                    {
                        iterateblock.Block.AnimateFrame();
                    }
                }
            }
        }
        DateTime LastSetGroup = DateTime.MinValue;
        public void SetGroupToField(BlockGroup bg)
        {
            lock (ActiveBlockGroups)
            {
                LastSetGroup = DateTime.Now;
                

                Debug.Print("Setting BlockGroup to Field:" + bg.ToString());
                foreach (var groupblock in bg)
                {

                    int RowPos = groupblock.Y + bg.Y;
                    int ColPos = groupblock.X + bg.X;
                    if (FieldContents[RowPos][ColPos] == null)
                        FieldContents[RowPos][ColPos] = groupblock.Block;
                }
                BlockGroupSet?.Invoke(this, new BlockGroupSetEventArgs(bg));
                if (ActiveBlockGroups.Contains(bg)) ActiveBlockGroups.Remove(bg);
            }

        }
        public bool CanFit(BlockGroup bg,int X,int Y)
        {
            foreach(var checkblock in bg)
            {
                int CheckRow = Y+checkblock.Y;
                int CheckCol = X+checkblock.X;
                if (CheckRow < 0 || CheckCol < 0) return false;
                if (CheckRow >= ROWCOUNT || CheckCol >= COLCOUNT) return false;
                var grabpos = FieldContents[CheckRow][CheckCol];
                if(grabpos!=null)
                {
                    return false;
                }

            }
            return true;
        }
        public bool CanRotate(BlockGroup bg,bool ccw)
        {

            BlockGroup duped = new BlockGroup(bg);
            duped.Rotate(ccw);
            duped.Clamp(ROWCOUNT, COLCOUNT);
            return CanFit(duped, bg.X, bg.Y);


        }
        public float GetBlockWidth(RectangleF ForBounds)
        {
            return ForBounds.Width / COLCOUNT;
        }
        public float GetBlockHeight(RectangleF ForBounds)
        {
            return ForBounds.Height / (VISIBLEROWS);
        }
        Pen LinePen = new Pen(Color.Black,1){DashPattern= new float[]{4,1,3,1,2,1,3,1}};
        public void Draw(Graphics g, RectangleF Bounds)
        {
            //first how big is each block?
            float BlockWidth = Bounds.Width / COLCOUNT;
            float BlockHeight = Bounds.Height / (VISIBLEROWS); //remember, we don't draw the top two rows- we start the drawing at row index 2, skipping 0 and 1 when drawing.
            for (int drawCol = 0; drawCol < COLCOUNT; drawCol++)
            {
                float XPos = drawCol * BlockWidth;
                g.DrawLine(LinePen,XPos,0,XPos,Bounds.Height);
            }
            for(int drawRow =HIDDENROWS;drawRow<ROWCOUNT;drawRow++)
            {
                float YPos = (drawRow - HIDDENROWS) * BlockHeight;
                g.DrawLine(LinePen,0,YPos,Bounds.Width,YPos);
            }
            for (int drawRow = HIDDENROWS;drawRow<ROWCOUNT;drawRow++)
            {
                float YPos = (drawRow-HIDDENROWS) * BlockHeight;
                var currRow = FieldContents[drawRow];
                //for each Tetris Row...
                for (int drawCol=0;drawCol<COLCOUNT;drawCol++)
                {
                    float XPos = drawCol * BlockWidth;
                    var TetBlock = currRow[drawCol];
                    if(TetBlock!=null)
                    {
                        RectangleF BlockBounds = new RectangleF(XPos,YPos,BlockWidth,BlockHeight);
                        TetrisBlockDrawParameters tbd = new TetrisBlockDrawParameters(g, BlockBounds,null);
                        TetBlock.DrawBlock(tbd);
                    }

                }
            }

         


            lock (ActiveBlockGroups)
            {
                foreach (BlockGroup bg in ActiveBlockGroups)
                {
                    int BaseXPos = bg.X;
                    int BaseYPos = bg.Y;
                    foreach (BlockGroupEntry bge in bg)
                    {
                        int DrawX = BaseXPos + bge.X;
                        int DrawY = BaseYPos + bge.Y - HIDDENROWS;
                        if (DrawX >= 0 && DrawY >= 0 && DrawX < COLCOUNT && DrawY < ROWCOUNT)
                        {
                            float DrawXPx = DrawX * BlockWidth;
                            float DrawYPx = DrawY * BlockHeight;
                            RectangleF BlockBounds = new RectangleF(DrawXPx, DrawYPx, BlockWidth, BlockHeight);
                            TetrisBlockDrawParameters tbd = new TetrisBlockDrawParameters(g, BlockBounds, bg);
                            bge.Block.DrawBlock(tbd);
                        }
                    }
                }
            }



        }
        private void SetFieldColors()
        {
            foreach(var iteraterow in FieldContents)
            {
                foreach(var iteratecell in iteraterow)
                {
                    if (iteratecell!=null)
                    Theme.ApplyTheme(iteratecell.Owner,this);
                }
            }
        }
        public int ProcessLines()
        {
            int rowsfound = 0;
            //checks the field contents for lines. If there are lines found, they are removed, and all rows above it are shifted down.
            for(int r = 0;r<ROWCOUNT;r++)
            {
                if(FieldContents[r].All((d)=>d!=null))
                {
                    Debug.Print("Found completed row at row " + r);
                    rowsfound++;
                    for(int g=r;g>0;g--)
                    {
                        Debug.Print("Moving row " + (g-1).ToString() + " to row " + g);

                        for(int i=0;i<COLCOUNT;i++)
                        {
                            FieldContents[g][i] = FieldContents[g - 1][i];
                        }
                    }
                }
            }
            long PreviousLineCount = LineCount;
            LineCount += rowsfound;
            if((PreviousLineCount%10)>(LineCount%10))
            {
                LevelChanged?.Invoke(this,new LevelChangeEventArgs((int)LineCount/10));
                TetrisGame.Soundman.PlaySound("level_up");
                SetFieldColors();
            }
            if(rowsfound > 0 && rowsfound < 4)
            {
                TetrisGame.Soundman.PlaySound("line_clear",2.0f);
            }
            else if(rowsfound == 4)
            {
                TetrisGame.Soundman.PlaySound("line_tetris",2.0f);
            }
            int topmost = ROWCOUNT;
            //find the topmost row with any blocks.
            for(int i=0;i<ROWCOUNT;i++)
            {
                if(FieldContents[i].Any((w)=>w!=null))
                {
                    topmost = i;
                    break;
                }
            }
            if(topmost < 9)
            {
                if (currenttempo == 1)
                {
                    currenttempo = 68;
                    TetrisGame.Soundman.PlayMusic(StandardMusic,0.75f,true);
                    var grabbed = TetrisGame.Soundman.GetPlayingMusic_Active();
                    if (grabbed != null)
                    {


                        TetrisGame.Soundman.GetPlayingMusic_Active().Tempo = 90f;
                    }
                }
            }
            else
            {
                if (currenttempo != 1)
                {
                    currenttempo = 1;
                    TetrisGame.Soundman.PlayMusic(StandardMusic,0.75f,true);
                    var grabbed = TetrisGame.Soundman.GetPlayingMusic_Active();
                    if (grabbed != null) grabbed.Tempo = 1f;
                }
            }

            return rowsfound;
        }
        private int currenttempo = 1;
    }
    
    public class LevelChangeEventArgs:EventArgs
    {
        private int LevelNumber = 0;
        public LevelChangeEventArgs(int newLevel=0)
        {

        }
    }
    public class BlockGroupSetEventArgs:EventArgs
    {
        public BlockGroup _group = null;
        public BlockGroupSetEventArgs(BlockGroup bg)
        {
            _group = bg;
        }
    }
}
