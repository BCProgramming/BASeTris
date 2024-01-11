using BASeTris.AssetManager;
using BASeTris.BackgroundDrawers;
using BASeTris.Rendering.Adapters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates
{
    public class TextScrollState : GameState
    {
        public override DisplayMode SupportedDisplayMode => DisplayMode.Full;
        public ScrollData sd = null;
        public String[][] CreditText = new string[][]{

            new []{

            "Producer",
            "","",
            "Michael Burgwin",

        },
                        new []{

            "Co-Producer",
            "","",
            "Michael Burgwin",

        },
        new []{

            "Assistant Co-Producer",
            "","",
            "Michael Burgwin",

        },
        new []{

            "Associate Producer",
            "","",
            "Michael Burgwin",

        },
        new []{

            "Executive Story Editor",
            "","",
            "Michael Burgwin",

        },
            new []{

            "Lead Programmer",
            "","",
            "Michael Burgwin",
        },
            new []{

            "Assistant Programmer",
            "","",
            "BC_Programming",
        },
                new []{

            "Assistant Assistant Programmer",
            "","",
            "Michael Burgwin",
        },
                       new []{

            "Graphics Designer",
            "","",

            "Michael Burgwin",
        },
        new []{

            "Music Takerizer",
            "","",
            "Michael Burgwin",
            "","","",
            "Sound Kajiggerizer","","",
            "Michael Burgwin"
        },
                       new []{

            "Supervisor Programmer",
            "",
            "Michael Burgwin",
        },
          new []{

            "Assistant Music Person",
            "","",
            "Michael Burgwin",
            "","","",
            "Sound Person",
            "","",
            "Michael Burgwin"
        },
        new []{

            "Donut Guy",
            "","",
            "Michael Burgwin",
            "","",
            "Rice Making Assistant",
            "","",
            "Michael Burgwin","","",
            "Bread Maker","",
            "Michael Burgwin"
        },
        new []{

            "Director",
            "",
            "Michael Burgwin",
            "",
            "Executive Director",
            "",
            "Michael Burgwin","","",
            "Assistant Directors",
            "","",
            "Michael Burgwin","",
            "-AND-","",
            "Michael Burgwin"
        },
        new []{

            "Production Designer",
            "",
            "Michael Burgwin",
            "",
            "Nomino Grip",
            "",
            "Michael Burgwin",
            "",
            "",
            "Nomino Grip Assistant",
            "",
            "Michael Burgwin",
        },
                new []{

            "Unit Production Manager",
            "",
            "Michael Burgwin",
            "",
            "First Assistant Director",
            "",

            "Michael Burgwin",
            "",
            "",
            "Second Assistant Director",
            "",
            "Michael Burgwin"
        },
            new []{

            "Nomino Theme Designer",
            "",
            "Michael Burgwin",
            "",
            "Set Decorator",
            "",
            "Michael Burgwin",
            "",
            ""

        },
        new []{

            "Art Director",
            "",
            "Michael Burgwin",
            "",
            "Scenic Art Director",
            "",
            "Michael Burgwin",
            "",
            "",
            "Rock Placement Engineer",
            "",
            "",
            "Michael Burgwin"

        },
                                new []{

            "Galley Chef",
            "",
            "Michael Burgwin",
            "",
            "Assistant Galley Chef",
            "",
            "Michael Burgwin","","",
            "Potato Peeler","","",
            "Michael Burgwin"
                                },

            new []
            {

            "Senior Illustrator",
            "",
            "Michael Burgwin","",
            "",
            "Set Designer",
            "",
            "Michael Burgwin",
            "",
            "",
            "Property Master",
               "",
            "Michael Burgwin",
            "",
            "",
            "Construction Coordinator",
             "",
            "Michael Burgwin",
            "",
            "",
            "Scenic Artist",
             "",
            "Michael Burgwin"
                                },
                        new []
            {

            "Script Supervisor",
            "",
            "Michael Burgwin","",
            "",
            "Special Effects",
            "",
            "Michael Burgwin",
            "",
            "",
            "Assistant Director",
               "",
            "Michael Burgwin",
            "",
            "",
            "Visual Effects",
             "",
            "Michael Burgwin",
            "",
            "",
            "Audio Effects",
             "",
            "Michael Burgwin"
            }
,
                         new []
            {

            "Stunt Coordinator",
            "",
            "Michael Burgwin","",
            "",
            "Sound Mixing Person",
            "",
            "Michael Burgwin",
            "",
            "",
            "Director of Towels",
               "",
            "Michael Burgwin",
            "",
            "",
            "Chief Chef Assistant",
             "",
            "Michael Burgwin",
            "",
            "",
            "Music Editor",
             "",
            "Michael Burgwin"
            },
                          new []
            {

            "Sound Editors",
            "",
            "Michael Burgwin","","Michael Burgwin","","Michael Burgwin","","Michael Burgwin","","Michael Burgwin","","Michael Burgwin","","",
            "",
            "Network Administrator",
            "",
            "Michael Burgwin",
            "",
            "",
            "Toilet Scrubber",
               "",
            "Michael Burgwin"
           
            },
            new []
            {

            "Executive Producer",
            "",
            "Michael Burgwin",

            }

        };
        public ScrollEntry CurrentItem = null;
        public GameState ReversionState { get; set; }

        //public IBackground BG;
        public TextScrollState(GameState pReversionState,IBackground pBG = null)
        {
            if (pBG == null) pBG = new StarfieldBackgroundSkia(new StarfieldBackgroundSkiaCapsule() { StarCount=300,WarpFactor=0.33f});
            BG = pBG;
            int offset = 2000;
            int count = 0;
            List<TextScrollEntry> scroller = new List<TextScrollEntry>();
            foreach (var iterate in CreditText)
            {
                TextScrollEntry tse = new TextScrollEntry((from s in iterate select s.ToUpper()).ToArray(), offset + (offset * count)) { TickLifeTime = 2000 };
                scroller.Add(tse);
                count++;
            }
            sd = new ScrollData(scroller);
            ReversionState = pReversionState;
        }

        private Stopwatch ElapsedWatcher = null;
       
        iActiveSoundObject CreditSong = null;
        public override void GameProc(IStateOwner pOwner)
        {
            if (BG != null) BG.FrameProc(pOwner);
            if (ElapsedWatcher == null)
            {
                ElapsedWatcher = new Stopwatch();
                ElapsedWatcher.Start();
            }
            if (CreditSong == null)
            {
                var playSong = TetrisGame.Soundman.GetSound("credit");
                CreditSong = playSong.Play(false, 4.0f);
                

            }

            if (CreditSong.Finished)
            {
                pOwner.CurrentState = ReversionState;
            }
            else
            {
                int PeekAmount = sd.PeekNextTick();
                if (PeekAmount == -1)
                {
                    sd.Reset();
                    ElapsedWatcher.Restart();
                }
                else if (PeekAmount > 0)
                    if (ElapsedWatcher.Elapsed.TotalMilliseconds > PeekAmount)
                    {
                        var NextEntry = sd.DrawNext();
                        CurrentItem = NextEntry;
                    }

            }

        }
        public BCPoint DirectionAdd = new BCPoint(0, 0);
        public double WarpFactor = 1;
        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            //throw new NotImplementedException();
            if (g == GameKeys.GameKey_Drop)
            {
                DirectionAdd = new BCPoint(DirectionAdd.X, DirectionAdd.Y-3);
            }
            else if (g == GameKeys.GameKey_Left)
            {
                DirectionAdd = new BCPoint(DirectionAdd.X - 3, DirectionAdd.Y);
            }
            else if (g == GameKeys.GameKey_Right)
            {
                DirectionAdd = new BCPoint(DirectionAdd.X + 3, DirectionAdd.Y);
            }
            else if (g == GameKeys.GameKey_Down)
            {
                DirectionAdd = new BCPoint(DirectionAdd.X, DirectionAdd.Y + 3);
            }
            else if (g == GameKeys.GameKey_RotateCW)
            {
                WarpFactor = WarpFactor + 0.25f;
            }
            else if (g == GameKeys.GameKey_RotateCCW)
            {
                WarpFactor = Math.Max(0,WarpFactor - 0.25f);
            }
        }
    }


    public class ScrollData
    {


        public Queue<ScrollEntry> Entries = null;
        public List<ScrollEntry> OriginalEntries = null;

        public void Reset()
        {
            Entries = new Queue<ScrollEntry>(Entries.OrderBy((w) => w.AppearanceTick));
        }
        public ScrollData(IEnumerable<ScrollEntry> pEntries)
        {
            OriginalEntries = new List<ScrollEntry>(pEntries);
            Entries = new Queue<ScrollEntry>(pEntries.OrderBy((w) => w.AppearanceTick));
        }
        public int PeekNextTick()
        {
            if (Entries == null) return 0;
            if (Entries.Count > 0)
            {
                return Entries.Peek().AppearanceTick;
            }
            return -1;
        }
        public ScrollEntry DrawNext()
        {
            if (Entries == null) return null;
            if (Entries.Count == 0) return null;
            return Entries.Dequeue();
        }

    }
    public class ScrollEntry
    {
        public int FirstTickAlive { get; set; }
        public int TickLifeTime { get; set; }
        public int AppearanceTick { get; set; }
        public ScrollEntry(int pAppearanceTick)
        {
            AppearanceTick = pAppearanceTick;
        }
    }
    public class TextScrollEntry : ScrollEntry
    {

        public String[] Text { get; set; }

        public TextScrollEntry(String[] pText, int pAppearanceTick) : base(pAppearanceTick)
        {
            Text = pText;
        }
    }
}
