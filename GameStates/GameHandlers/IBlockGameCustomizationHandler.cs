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

    


    ///TODO Game types that will need new handlers:
    ///1. Columns. Nominoes are three line series blocks. Further, the line series will check diagonals. I think the rotation behaviour
    /// could dbe handled by the nomino itself through the rotation modulo, instead of having rotated positions it could just have the three rotation points be the three separate positions, rotating each piece through the nomino for each of those three states.
    /// 
    /// 2. Bombtris/Blasttris. Tetris with bomb blocks added to some nominoes, and lines don't clear- instead, lines will cause bombs on that line to explode, which create rectangular explosions that will chain to other bomb blocks. Then blocks that were left floating by the explosions fall.
    /// 
    /// <summary>
    /// interface for game customization, for different Nomino-based games. (eg. Tetris being standard, but could be Tetris 2 or Dr Mario and stuff)
    /// </summary>
    public interface IBlockGameCustomizationHandler 
    {
        String Name { get; }
        FieldChangeResult ProcessFieldChange(GameplayGameState state, IStateOwner pOwner, Nomino Trigger);
        IHighScoreList GetHighScores();
        Choosers.BlockGroupChooser GetChooser(IStateOwner pOwner);
        IBlockGameCustomizationHandler NewInstance();
        NominoTheme DefaultTheme { get; }
        void PrepareField(GameplayGameState state, IStateOwner pOwner); //prepare field for a new game. (or level or whatever- basically depends on the type of game)
        BaseStatistics Statistics { get; set; }
        Nomino[] GetNominos();
        GameOptions GameOptions { get; }

        GamePreparerOptions PrepInstance { get; set; }
        

        GameOverStatistics GetGameOverStatistics(GameplayGameState state, IStateOwner pOwner);

        
        IGameCustomizationStatAreaRenderer<TRenderTarget, GameplayGameState, TDataElement, IStateOwner> GetStatAreaRenderer<TRenderTarget, TDataElement>();

        FieldCustomizationInfo GetFieldInfo();
        //IGameCustomizationStatAreaRenderer<TRenderTarget,TRenderSource,TDataElement,TOwnerType>

    }
    public class ExtendedCustomizationHandlerResult
    {
        public static ExtendedCustomizationHandlerResult Default = new ExtendedCustomizationHandlerResult(true);
        public bool ContinueDefault { get; init; }
        public ExtendedCustomizationHandlerResult(bool pDoDefault)
        {
            ContinueDefault = pDoDefault;
        }
    }
    public interface IExtendedGameCustomizationHandler
    {
        ExtendedCustomizationHandlerResult GameProc(GameplayGameState state, IStateOwner pOwner);

        //Obbsolete("Use Rendering Providers")]
        /*public abstract void DrawProc(IStateOwner pOwner, Graphics g, RectangleF Bounds);*/
        ExtendedCustomizationHandlerResult HandleGameKey(GameplayGameState state,IStateOwner pOwner, GameState.GameKeys g);
    }
    public class FieldCustomizationInfo
    {
        public int FieldRows { get; init; }
        public int TopHiddenFieldRows { get; init; }
        public int BottomHiddenFieldRows { get; init; }
        public int FieldColumns { get; init; }
        
    }
    public class HandlerMenuCategoryAttribute : Attribute
    {
        public String Category { get; set; }
        public HandlerMenuCategoryAttribute(String pCategory)
        {
            Category = pCategory;
        }
    }
    public class HandlerTipTextAttribute : Attribute
    {
        public String TipText { get; set; }
        public HandlerTipTextAttribute(String pTipText)
        {
            TipText = pTipText;
        }
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
        public int BlocksAffected { get; set; }
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

    public static class HandlerHelperExtensions
    {
        private static Dictionary<IStateOwner,IBlockGameCustomizationHandler> LastHandlers = new Dictionary<IStateOwner, IBlockGameCustomizationHandler>();
        /// <summary>
        /// Helper extension on IStateOwner. This attempts to get the IGameCustomizationHandler that is currently  "active" for the handler.
        /// The customization handler is a part of the GameplayGameState, so this relies on either the current state being the gameplay game state, a Composite state of the gameplay gamestate. If neither of those
        /// is the case, it will return the last handler that was previously retrieved through this method from that Owner, if this is the first call, it will return null.
        /// </summary>
        /// <param name="pOwner"></param>
        /// <returns></returns>
        public static IBlockGameCustomizationHandler GetHandler(this IStateOwner pOwner)
        {
            IBlockGameCustomizationHandler result = null;
            if(pOwner.CurrentState is GameplayGameState ggs)
            {
                result = ggs.GameHandler;
            }
            else if(pOwner.CurrentState is ICompositeState<GameplayGameState> css)
            {
                result = css.GetComposite().GameHandler;
            }
            else
            {
                if (LastHandlers.ContainsKey(pOwner)) return LastHandlers[pOwner];
            }
            LastHandlers[pOwner] = result;

            return result;
        }

    }


}
