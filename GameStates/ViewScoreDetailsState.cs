using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeCamp.BASeScores;
using BASeTris.AssetManager;
using BASeTris.BackgroundDrawers;

namespace BASeTris.GameStates
{
    public class ViewScoreDetailsState : GameState
    {
        public override DisplayMode SupportedDisplayMode { get{ return DisplayMode.Full; } }
        private ShowHighScoresState _Owner = null;
        private IHighScoreEntry ShowEntry = null;
        public enum ViewScoreDetailsType
        {
            Details_Tetrominoes,
            Details_LevelTimes
        }
        private IBackgroundDraw _BG;
        private ViewScoreDetailsType CurrentView = ViewScoreDetailsType.Details_Tetrominoes;
        private int _Position;
        public ViewScoreDetailsState(ShowHighScoresState pOwner,IHighScoreEntry pShowEntry,IBackgroundDraw useBG,int DetailPosition)
        {
            _Position = DetailPosition;
            _BG = useBG; //so it is the same as the "main" show score state and looks "seamless".
            _Owner = pOwner;
            ShowEntry = pShowEntry;
        }
        public override void DrawStats(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            //throw new NotImplementedException();
        }
    
        public override void GameProc(IStateOwner pOwner)
        {
            _BG.FrameProc();
            //For flair we'll have some gubbins or whatever in the background.
            //throw new NotImplementedException();
        }
        private String _DetailHeader = "---SCORE DETAILS---";
        private Font HeaderFont = new Font(TetrisGame.RetroFont,24,FontStyle.Regular);
        private Font PlacementFont = new Font(TetrisGame.RetroFont,18,FontStyle.Regular);
        private Font DetailFont = new Font(TetrisGame.RetroFont,12,FontStyle.Regular);
        private Pen Separator = new Pen(Color.Black,3);
        public override void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            _BG.DrawProc(g,Bounds);

            //One thing we draw in every case is the "--SCORE DETAILS--" header text. this is positioned at 5% from the top, centered in the middle of our bounds.
            float Millipercent = (float)DateTime.Now.Ticks / 5000f; //(float)DateTime.Now.Millisecond / 1000;

            var MeasuredHeader = g.MeasureString(_DetailHeader, HeaderFont);
            int RotateAmount = (int)(Millipercent * 240);
            Color UseColor1 = HSLColor.RotateHue(Color.Red, RotateAmount);
            Color UseColor2 = HSLColor.RotateHue(Color.LightPink, RotateAmount);
            PointF ScorePosition = new PointF((Bounds.Width / 2) - (MeasuredHeader.Width / 2), Bounds.Height * 0.05f);
            using (LinearGradientBrush lgb = new LinearGradientBrush(new Rectangle(0, 0, (int)MeasuredHeader.Width, (int)MeasuredHeader.Height), UseColor1, UseColor2, LinearGradientMode.Vertical))
            {
                

                using (GraphicsPath gp = new GraphicsPath())
                {
                    gp.AddString(_DetailHeader, HeaderFont, new Point((int)ScorePosition.X, (int)ScorePosition.Y), StringFormat.GenericDefault);
                    g.FillPath(lgb,gp);
                    g.DrawPath(Pens.White,gp);
                }
            }

            //we also show Xth Place - <NAME> centered below the header using the placementfont.
            String sPlacement = TetrisGame.FancyNumber(_Position) + " - " + ShowEntry.Name + " - " + ShowEntry.Score.ToString();

            var measureit = g.MeasureString(sPlacement, PlacementFont);

            PointF DrawPlacement = new PointF(Bounds.Width/2-measureit.Width/2,(float)(ScorePosition.Y + MeasuredHeader.Height*1.1f));

            g.DrawString(sPlacement,PlacementFont,Brushes.Black,DrawPlacement.X+3,DrawPlacement.Y+3);
            g.DrawString(sPlacement, PlacementFont, Brushes.White, DrawPlacement.X, DrawPlacement.Y);

            g.DrawLine(Separator,(float)(Bounds.Width*0.05f),(float)(DrawPlacement.Y+measureit.Height+5),(float)(Bounds.Width*0.95), (float)(DrawPlacement.Y + measureit.Height + 5));
           

            switch (CurrentView)
            {
                case ViewScoreDetailsType.Details_Tetrominoes:
                    DrawTetronimoDetails(g,Bounds);
                    break;
                case ViewScoreDetailsType.Details_LevelTimes:
                    DrawLevelTimesDetails(g,Bounds);
                    break;
            }

        }
        private void DrawTetronimoDetails(Graphics g, RectangleF Bounds)
        {
            
            //draws the tetronimo pictures, the tetronimo stats, and the numberof lines down the screen.
        }
        private void DrawLevelTimesDetails(Graphics g, RectangleF Bounds)
        {
            //draw the times each level was achieved.
            //(Possible feature: support paging if  we can't fit them on one screen?)
        }
        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            switch(g)
            {
                case GameKeys.GameKey_Left:
                    break;
                case GameKeys.GameKey_Right:
                    break;
                case GameKeys.GameKey_RotateCCW:
                    pOwner.CurrentState = _Owner;
                    break;
            }
        }

        public override void DrawForegroundEffect(IStateOwner pOwner, Graphics g, RectangleF Bounds)
        {
            
        }
    }
}
