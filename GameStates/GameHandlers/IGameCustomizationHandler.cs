using BASeCamp.BASeScores;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Tetrominoes;
using BASeTris.Choosers;
using BASeTris.Rendering.Adapters;
using SkiaSharp;

namespace BASeTris.GameStates.GameHandlers
{
    /// <summary>
    /// interface for game customization, for different Nomino-based games. (eg. Tetris being standard, but could be Tetris 2 or Dr Mario and stuff)
    /// </summary>
    public interface IGameCustomizationHandler
    {
        String Name { get; }
        FieldChangeResult ProcessFieldChange(GameplayGameState state, IStateOwner pOwner, Nomino Trigger);
        IHighScoreList<TetrisHighScoreData> GetHighScores();
        Choosers.BlockGroupChooser Chooser { get;  }
        IGameCustomizationHandler NewInstance();
        TetrominoTheme DefaultTheme { get; }
        void PrepareField(GameplayGameState state, IStateOwner pOwner); //prepare field for a new game. (or level or whatever- basically depends on the type of game)
        BaseStatistics Statistics { get; }
        Nomino[] GetNominos();
        GameOptions GameOptions { get; }

        GameOverStatistics GetGameOverStatistics(GameplayGameState state, IStateOwner pOwner);

        IGameCustomizationStatAreaRenderer<TRenderTarget, GameplayGameState, TDataElement, IStateOwner> GetStatAreaRenderer<TRenderTarget, TDataElement>();

        //IGameCustomizationStatAreaRenderer<TRenderTarget,TRenderSource,TDataElement,TOwnerType>

    }
    public class GameOverStatistics
    {
        public List<GameOverStatistic> Statistics = null;
        public GameOverStatistics(IEnumerable<GameOverStatistic> pStatistics)
        {
            Statistics = pStatistics.ToList();
        }
        public GameOverStatistics(params GameOverStatistic[] pStatistics)
        {
            Statistics = pStatistics.ToList();
        }
    }
    public class GameOverStatistic
    {
        public List<GameOverStatisticColumnData> ColumnData = null;
        public GameOverStatistic(IEnumerable<GameOverStatisticColumnData> StatisticData)
        {
            ColumnData = StatisticData.ToList();
        }
        public GameOverStatistic(params GameOverStatisticColumnData[] StatisticData)
        {
            ColumnData = StatisticData.ToList();
        }
    }
    public class GameOverStatisticColumnData
    {
        public enum CellType
        {
            Image,
            Text
        }
        public enum HorizontalAlignment
        {
            Left,
            Middle,
            Right
        }
        public enum VerticalAlignment
        {
            Top,
            Middle,
            Bottom
        }
        public static SKRect AlignRect(SKRect Container,SKPoint DesiredSize,HorizontalAlignment halign,VerticalAlignment valign)
        {

            //handle horizontal.
            float XPosition= Container.Left, YPosition = Container.Top;
            if(halign==HorizontalAlignment.Middle)
            {
                XPosition = Container.Left + Container.Width / 2 - DesiredSize.X / 2;
            }
            else if(halign == HorizontalAlignment.Right)
            {
                XPosition = Container.Right - DesiredSize.X;
            }

            if(valign == VerticalAlignment.Middle)
            {
                YPosition = Container.Top + Container.Height / 2 - DesiredSize.Y / 2;
            }
            else if(valign == VerticalAlignment.Bottom)
            {
                YPosition = Container.Bottom - DesiredSize.Y;
            }

            return new SKRect(XPosition, YPosition, XPosition + DesiredSize.X, YPosition + DesiredSize.Y);


        }
        public GameOverStatisticColumnData(String pText, BCColor pColor, BCColor pShadowColor,HorizontalAlignment hAlign = HorizontalAlignment.Left,VerticalAlignment vAlign = VerticalAlignment.Top)
        {
            this.InfoType = CellType.Text;
            Color = pColor;
            CellText = pText;
            ContentAlignmentHorizontal = hAlign;
            ContentAlignmentVertical = vAlign;
            ShadowColor = pShadowColor;
        }
        public GameOverStatisticColumnData(BCImage CellImage, HorizontalAlignment hAlign = HorizontalAlignment.Left, VerticalAlignment vAlign = VerticalAlignment.Top)
        {
            this.InfoType = CellType.Image;
            this.CellImage = CellImage;
        }
        public CellType InfoType { get; set; }
        public BCColor Color { get; set; }
        public BCColor ShadowColor { get; set; }
        public BCImage CellImage { get; set; }
        public String CellText { get; set; }
        public float SizeScale { get; set; } = 1;
        public HorizontalAlignment ContentAlignmentHorizontal { get; set; }
        public VerticalAlignment ContentAlignmentVertical { get; set; }
    }
    public class FieldChangeResult
    {
        public int ScoreResult { get; set; }
        public IList<Action> AfterClearActions { get; set; }
    }
    public interface IGameCustomizationStatAreaRenderer<TRenderTarget,TRenderSource,TDataElement,TOwnerType> : BASeCamp.Rendering.Interfaces.IRenderingHandler<TRenderTarget,TRenderSource,TDataElement,TOwnerType>
         where TRenderSource : class
    {

    }

    public class StatisticsInfoLineInfo
    {
        List<StatisticInfoColumnInfo> Columns { get; set; }
        public StatisticsInfoLineInfo(params StatisticInfoColumnInfo[] input)
        {
            Columns = input.ToList();
        }
    }
    public abstract class StatisticInfoColumnInfo
    {
        public enum HorizontalStatisticAlignment
        {
            Left,
            Middle,
            Right
        }
        public enum VerticalStatisticAlignment
        {
            Left,
            Middle,
            Right
        }
        public HorizontalStatisticAlignment HAlign { get; set; }
        public VerticalStatisticAlignment VAlign { get; set; }
        protected StatisticInfoColumnInfo(HorizontalStatisticAlignment pH, VerticalStatisticAlignment pV)
        {
            HAlign = pH;
            VAlign = pV;
        }
    }
    public class StatisticInfoColumnInfoText : StatisticInfoColumnInfo
    {
        public String Text { get; set; }
        public StatisticInfoColumnInfoText(String pText, HorizontalStatisticAlignment pH, VerticalStatisticAlignment pV) : base(pH, pV)
        {
            Text = pText;
        }
    }
    public class StatisticInfoColumnInfoImage : StatisticInfoColumnInfo
    {
        public BCImage CellImage { get; set; }
        public StatisticInfoColumnInfoImage(BCImage pImage, HorizontalStatisticAlignment pH, VerticalStatisticAlignment pV) : base(pH, pV)
        {
            CellImage = pImage;
        }
    }

    public class StatisticInfoColumnInfoProgress : StatisticInfoColumnInfo
    {
        public float Percentage { get; set; }
        public StatisticInfoColumnInfoProgress(float pPercentage, HorizontalStatisticAlignment pH, VerticalStatisticAlignment pV) : base(pH, pV)
        {
            Percentage = pPercentage;
        }
    }




}
