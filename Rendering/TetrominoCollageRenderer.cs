using BASeCamp.BASeScores;
using BASeCamp.Elementizer;
using BASeTris.Blocks;
using BASeTris.Choosers;
using BASeTris.GameStates;
using BASeTris.GameStates.GameHandlers;
using BASeTris.Rendering.Skia;
using BASeTris.Tetrominoes;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using XInput.Wrapper;

namespace BASeTris.Rendering
{

    public class FakeRendererHandler : IBlockGameCustomizationHandler
    {
        public int ColumnWidth { get; set; }
        public int ColumnHeight { get; set; }

        TetrisStatistics _Stats = null;
        public FakeRendererHandler(int pWidth, int pHeight, int pLevel)
        {
            _Stats = new TetrisStatistics() {I_Line_Count=pLevel*10 };
            
        }
        public GamePreparerOptions PrepInstance { get; set; } = null;
        public string Name => "Fake Collage Hander";

        public NominoTheme DefaultTheme => null;

        public BaseStatistics Statistics { get { return _Stats; }set { } }

        public GameOptions GameOptions => null;

        public BlockGroupChooser GetChooser(IStateOwner pOwner)
        {
            throw new NotImplementedException();
        }
        public virtual FieldCustomizationInfo GetFieldInfo()
        {
            return new FieldCustomizationInfo()
            {
                FieldRows = ColumnHeight,
                BottomHiddenFieldRows = 0,
                TopHiddenFieldRows = 0,
                FieldColumns = ColumnWidth
            };
        }


      

        public GameOverStatistics GetGameOverStatistics(GameplayGameState state, IStateOwner pOwner)
        {
            return null;
        }

   
        public IHighScoreList GetHighScores()
        {
            return null;
        }

        public Nomino[] GetNominos()
        {
            return null;
        }

        public IGameCustomizationStatAreaRenderer<TRenderTarget, GameplayGameState, TDataElement, IStateOwner> GetStatAreaRenderer<TRenderTarget, TDataElement>()
        {
            return null;
        }

        public IBlockGameCustomizationHandler NewInstance()
        {
            return null;
        }

        public void PrepareField(GameplayGameState state, IStateOwner pOwner)
        {
            
        }

        public FieldChangeResult ProcessFieldChange(GameplayGameState state, IStateOwner pOwner, Nomino Trigger)
        {
            return null;
        }
    }

    /// <summary>
    /// This class is responsible for effectively taking an arrangement of Nominos and a theme and some additional control properties, and generating a Bitmap out of it.
    /// The aim/purpose of this is for visual flourishes or elements on stuff like the title screen. Eg it would have corner/borders constructed out of tetrominoes- at least, that is the idea.
    /// </summary>
    public class TetrominoCollageRenderer
    {
        public static SKBitmap GetNominoBitmap(NominoTheme _theme)
        {
            //similar to "Collage", but we only want to use One Nomino, at random. (one of the tetrominoes, for the moment.)
            int generatedlevel = TetrisGame.StatelessRandomizer.Next(0, 21);
            //first, choose a random Tetromino Type.
            var buildNominoFunc = TetrisGame.Choose(Tetrominoes.Tetromino.StandardTetrominoFunctions);
            Nomino GenerateResult = buildNominoFunc();

            //with the generated Nomino in hand, construct a CollageRenderer instance the size of this Nomino.
            GenerateResult.X = 0;
            GenerateResult.Y = 0;
            GenerateResult.RecalcExtents();

            TetrominoCollageRenderer tcr = new TetrominoCollageRenderer( GenerateResult.GroupExtents.Right+1, GenerateResult.GroupExtents.Bottom+1, 500, 500, generatedlevel, _theme,SKColors.Transparent);
            _theme.ApplyTheme(GenerateResult, null, tcr._field, NominoTheme.ThemeApplicationReason.NewNomino);
            tcr.AddNomino(GenerateResult);

            SKBitmap CreateBitmap = tcr.Render();
            
            return CreateBitmap;
        }
        public static IEnumerable<Nomino> LoadTetrominoCollageFromXMLString(String sString,out XElement TetrominoCollage)
        {
            var xdoc = XDocFromString(sString);
            TetrominoCollage = xdoc.Root;
            return LoadTetrominoCollageFromXML(xdoc.Root);
        }
        private static XDocument XDocFromString(String sSource)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                StreamWriter sw = new StreamWriter(ms);
                sw.Write(sSource);
                sw.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                XDocument xdoc = XDocument.Load(ms);
                return xdoc;
            }
        }
        public static IEnumerable<Nomino> LoadTetrominoCollageFromXML(String sFileName,out XElement TetrominoCollage)
        {
            XDocument xdoc = XDocument.Load(sFileName);
            TetrominoCollage = xdoc.Root;
            return LoadTetrominoCollageFromXML(xdoc.Root);

        }
        private static readonly Dictionary<String, Type> TetrominoTypeLookup = new Dictionary<string, Type>()
        {
            {"I",typeof(Tetromino_I) },
            {"L",typeof(Tetromino_L) },
            {"J",typeof(Tetromino_J) },
            {"Z",typeof(Tetromino_Z) },
            {"S",typeof(Tetromino_S) },
            {"T",typeof(Tetromino_T) },
            {"O",typeof(Tetromino_O) },

        };
        public static IEnumerable<Nomino> LoadTetrominoCollageFromXML(XElement TetCollageNode)
        {
            
            
            foreach (XElement TetrominoElements in TetCollageNode.Elements("Tetromino"))
            {
                

                //<Tetromino Type=[I,L,J,Z,S,T,O] Rotation=[0,1,2,3] X=XPosition Y=YPosition>

                String sTetrominoType = TetrominoElements.GetAttributeString("Type");
                int sRotation = TetrominoElements.GetAttributeInt("Rotation", 0);
                int XPos = TetrominoElements.GetAttributeInt("X", 0);
                int YPos = TetrominoElements.GetAttributeInt("Y", 0);

                if (TetrominoTypeLookup.ContainsKey(sTetrominoType))
                {
                    Type useTetrominoType = TetrominoTypeLookup[sTetrominoType];


                    Nomino Generated = (Nomino)Activator.CreateInstance(useTetrominoType,new Object[] { null });
                    Generated.X = XPos;
                    Generated.Y = YPos;
                    for (int i = 0; i < sRotation; i++)
                    {
                        Generated.Rotate(false);
                    }
                    yield return Generated;
                }


            }



        }
        private static Nomino[][] GroupNominos(IEnumerable<Nomino> FlatSource)
        {
            Dictionary<Type, List<Nomino>> Groups = new Dictionary<Type, List<Nomino>>();
            foreach (var iterate in FlatSource)
            {
                if (!Groups.ContainsKey(iterate.GetType())) Groups.Add(iterate.GetType(), new List<Nomino>());
                Groups[iterate.GetType()].Add(iterate);
            }

            return Groups.Values.Select((s) => s.ToArray()).ToArray();

        }


        private static String fullFieldBackgroundXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
  <TetrominoCollage Rows=""20"" Columns=""11"">	<Tetromino Scale=""1"" Type=""L"" Rotation=""2"" X=""5"" Y=""4"" />
    <Tetromino Scale=""1"" Type=""L"" Rotation=""3"" X=""4"" Y=""12"" />
    <Tetromino Scale=""1"" Type=""Z"" Rotation=""1"" X=""7"" Y=""0"" />
    <Tetromino Scale=""1"" Type=""I"" Rotation=""2"" X=""5"" Y=""13"" />
    <Tetromino Scale=""1"" Type=""Z"" Rotation=""2"" X=""5"" Y=""16"" />
    <Tetromino Scale=""1"" Type=""T"" Rotation=""3"" X=""5"" Y=""6"" />
    <Tetromino Scale=""1"" Type=""I"" Rotation=""-1"" X=""-1"" Y=""10"" />
    <Tetromino Scale=""1"" Type=""O"" Rotation=""3"" X=""2"" Y=""18"" />
    <Tetromino Scale=""1"" Type=""J"" Rotation=""2"" X=""8"" Y=""3"" />
    <Tetromino Scale=""1"" Type=""O"" Rotation=""2"" X=""9"" Y=""6"" />
    <Tetromino Scale=""1"" Type=""S"" Rotation=""0"" X=""6"" Y=""0"" />
    <Tetromino Scale=""1"" Type=""Z"" Rotation=""2"" X=""8"" Y=""13"" />
    <Tetromino Scale=""1"" Type=""O"" Rotation=""2"" X=""1"" Y=""12"" />
    <Tetromino Scale=""1"" Type=""I"" Rotation=""2"" X=""4"" Y=""0"" />
    <Tetromino Scale=""1"" Type=""J"" Rotation=""0"" X=""0"" Y=""14"" />
    <Tetromino Scale=""1"" Type=""L"" Rotation=""3"" X=""6"" Y=""11"" />
    <Tetromino Scale=""1"" Type=""L"" Rotation=""0"" X=""8"" Y=""18"" />
    <Tetromino Scale=""1"" Type=""I"" Rotation=""2"" X=""3"" Y=""-2"" />
    <Tetromino Scale=""1"" Type=""T"" Rotation=""3"" X=""3"" Y=""4"" />
    <Tetromino Scale=""1"" Type=""Z"" Rotation=""0"" X=""2"" Y=""6"" />
    <Tetromino Scale=""1"" Type=""I"" Rotation=""1"" X=""-2"" Y=""16"" />
    <Tetromino Scale=""1"" Type=""J"" Rotation=""3"" X=""9"" Y=""0"" />
    <Tetromino Scale=""1"" Type=""O"" Rotation=""1"" X=""9"" Y=""10"" />
    <Tetromino Scale=""1"" Type=""I"" Rotation=""1"" X=""-1"" Y=""3"" />
    <Tetromino Scale=""1"" Type=""I"" Rotation=""3"" X=""1"" Y=""2"" />
    <Tetromino Scale=""1"" Type=""I"" Rotation=""3"" X=""6"" Y=""6"" />
    <Tetromino Scale=""1"" Type=""Z"" Rotation=""0"" X=""3"" Y=""10"" />
    <Tetromino Scale=""1"" Type=""J"" Rotation=""2"" X=""8"" Y=""15"" />
    <Tetromino Scale=""1"" Type=""S"" Rotation=""2"" X=""7"" Y=""8"" />
    <Tetromino Scale=""1"" Type=""Z"" Rotation=""2"" X=""3"" Y=""16"" />
    <Tetromino Scale=""1"" Type=""L"" Rotation=""0"" X=""1"" Y=""13"" />
    <Tetromino Scale=""1"" Type=""I"" Rotation=""0"" X=""7"" Y=""2"" />
    <Tetromino Scale=""1"" Type=""T"" Rotation=""4"" X=""5"" Y=""3"" />
    <Tetromino Scale=""1"" Type=""L"" Rotation=""1"" X=""5"" Y=""12"" />
    <Tetromino Scale=""1"" Type=""L"" Rotation=""2"" X=""3"" Y=""2"" />
    <Tetromino Scale=""1"" Type=""J"" Rotation=""0"" X=""8"" Y=""11"" />
    <Tetromino Scale=""1"" Type=""O"" Rotation=""0"" X=""1"" Y=""0"" />
    <Tetromino Scale=""1"" Type=""S"" Rotation=""2"" X=""3"" Y=""7"" />
    <Tetromino Scale=""1"" Type=""Z"" Rotation=""0"" X=""0"" Y=""9"" />
    <Tetromino Scale=""1"" Type=""I"" Rotation=""3"" X=""-1"" Y=""3"" />
    <Tetromino Scale=""1"" Type=""I"" Rotation=""1"" X=""-1"" Y=""16"" />
    <Tetromino Scale=""1"" Type=""L"" Rotation=""2"" X=""3"" Y=""0"" />
    <Tetromino Scale=""1"" Type=""J"" Rotation=""2"" X=""8"" Y=""12"" />
    <Tetromino Scale=""1"" Type=""O"" Rotation=""3"" X=""5"" Y=""9"" />
    <Tetromino Scale=""1"" Type=""O"" Rotation=""2"" X=""8"" Y=""17"" />
    <Tetromino Scale=""1"" Type=""Z"" Rotation=""1"" X=""1"" Y=""15"" />
    <Tetromino Scale=""1"" Type=""J"" Rotation=""2"" X=""8"" Y=""7"" />
    <Tetromino Scale=""1"" Type=""J"" Rotation=""2"" X=""5"" Y=""15"" />
    <Tetromino Scale=""1"" Type=""J"" Rotation=""2"" X=""1"" Y=""10"" />
    <Tetromino Scale=""1"" Type=""I"" Rotation=""1"" X=""2"" Y=""13"" />
    <Tetromino Scale=""1"" Type=""I"" Rotation=""2"" X=""4"" Y=""17"" />
    <Tetromino Scale=""1"" Type=""L"" Rotation=""1"" X=""-1"" Y=""0"" />
    <Tetromino Scale=""1"" Type=""O"" Rotation=""0"" X=""0"" Y=""7"" />
    <Tetromino Scale=""1"" Type=""T"" Rotation=""1"" X=""1"" Y=""7"" />
    <Tetromino Scale=""1"" Type=""J"" Rotation=""1"" X=""7"" Y=""5"" /></TetrominoCollage>";


        //corner XML layouts. These (can) be used to generate a bitmap to draw in the corners.

        private static readonly String[] Lower_Left_CollageXML = new[] { @"<?xml version=""1.0"" encoding=""utf-8""?>
  <TetrominoCollage Rows=""6"" Columns=""6"">
    <Tetromino Scale=""1"" Type=""S"" Rotation=""0"" X=""0"" Y=""4"" />
    <Tetromino Scale=""1"" Type=""Z"" Rotation=""1"" X=""3"" Y=""0"" />
    <Tetromino Scale=""1"" Type=""T"" Rotation=""3"" X=""4"" Y=""2"" />
    <Tetromino Scale=""1"" Type=""I"" Rotation=""0"" X=""2"" Y=""4"" />
    <Tetromino Scale=""1"" Type=""L"" Rotation=""1"" X=""2"" Y=""2"" />
</TetrominoCollage>" };

        private static readonly String[] Lower_Right_CollageXML = new[] { @"<?xml version=""1.0"" encoding=""utf-8""?>
  <TetrominoCollage Rows = ""6"" Columns=""6"">
    <Tetromino Scale = ""1"" Type=""S"" Rotation=""1"" X=""-1"" Y=""0"" />
    <Tetromino Scale = ""1"" Type=""Z"" Rotation=""2"" X=""3"" Y=""3"" />
    <Tetromino Scale = ""1"" Type=""T"" Rotation=""4"" X=""1"" Y=""4"" />
    <Tetromino Scale = ""1"" Type=""I"" Rotation=""1"" X=""-2"" Y=""2"" />
    <Tetromino Scale = ""1"" Type=""L"" Rotation=""2"" X=""1"" Y=""2"" />
  </TetrominoCollage>" };

        private static readonly String[] Upper_Left_CollageXML = new[] { @"<?xml version=""1.0"" encoding=""utf-8""?>
  <TetrominoCollage Rows = ""6"" Columns=""6"">
    <Tetromino Scale=""1"" Type=""T"" Rotation=""2"" X=""3"" Y=""-1"" />
    <Tetromino Scale=""1"" Type=""Z"" Rotation=""1"" X=""1"" Y=""1"" />
    <Tetromino Scale=""1"" Type=""S"" Rotation=""0"" X=""0"" Y=""1"" />
    <Tetromino Scale=""1"" Type=""L"" Rotation=""2"" X=""0"" Y=""-1"" />
    <Tetromino Scale=""1"" Type=""J"" Rotation=""1"" X=""-1"" Y=""3"" />
  </TetrominoCollage>" };

                private static readonly String[] Upper_Right_CollageXML = new[] { @"<?xml version=""1.0"" encoding=""utf-8""?>
  <TetrominoCollage Rows = ""6"" Columns=""6"">
    <Tetromino Scale=""1"" Type=""T"" Rotation=""3"" X=""4"" Y=""1"" />
    <Tetromino Scale=""1"" Type=""I"" Rotation=""0"" X=""0"" Y=""-1"" />
    <Tetromino Scale=""1"" Type=""S"" Rotation=""1"" X=""3"" Y=""3"" />
    <Tetromino Scale=""1"" Type=""S"" Rotation=""0"" X=""3"" Y=""0"" />
    <Tetromino Scale=""1"" Type=""S"" Rotation=""1"" X=""1"" Y=""1"" />
  </TetrominoCollage>" };


        private static readonly String BackgroundCollageXML2 =
        @" <?xml version=""1.0"" encoding=""utf-8""?>
  <TetrominoCollage Rows=""8"" Columns=""8"">
    <Tetromino Type=""J"" Rotation=""3"" X=""2"" Y=""4"" />
    <Tetromino Type=""I"" Rotation=""4"" X=""-2"" Y=""3"" />
    <Tetromino Type=""T"" Rotation=""5"" X=""4"" Y=""2"" />
    <Tetromino Type=""J"" Rotation=""2"" X=""5"" Y=""-1"" />
    <Tetromino Type=""I"" Rotation=""0"" X=""2"" Y=""0"" />
    <Tetromino Type=""S"" Rotation=""5"" X=""5"" Y=""1"" />
    <Tetromino Type=""O"" Rotation=""2"" X=""6"" Y=""6"" />
    <Tetromino Type=""I"" Rotation=""2"" X=""6"" Y=""2"" />
    <Tetromino Type=""T"" Rotation=""8"" X=""0"" Y=""6"" />
    <Tetromino Type=""T"" Rotation=""0"" X=""2"" Y=""-1"" />
    <Tetromino Type=""T"" Rotation=""4"" X=""2"" Y=""7"" />
    <Tetromino Type=""L"" Rotation=""5"" X=""3"" Y=""5"" />
    <Tetromino Type=""L"" Rotation=""6"" X=""5"" Y=""4"" />
    <Tetromino Type=""L"" Rotation=""2"" X=""0"" Y=""4"" />
    <Tetromino Type=""T"" Rotation=""3"" X=""3"" Y=""2"" />
    <Tetromino Type=""J"" Rotation=""1"" X=""1"" Y=""2"" />
    <Tetromino Type=""L"" Rotation=""1"" X=""-1"" Y=""1"" />
    <Tetromino Type=""L"" Rotation=""3"" X=""0"" Y=""0"" />
  </TetrominoCollage>";

        private static readonly String[] DefaultBackgroundCollageXML = new string[] { @"<?xml version=""1.0"" encoding=""utf-8""?>

<TetrominoCollage Rows=""6"" Columns=""6"">
<Tetromino Type=""I"" Rotation=""1"" X=""-1"" Y=""-3"" />
<Tetromino Type=""I"" Rotation=""1"" X=""-1"" Y=""3"" />
<Tetromino Type=""T"" Rotation=""3"" X=""2"" Y=""1"" />
<Tetromino Type=""T"" Rotation=""1"" X=""3"" Y=""-1"" />
<Tetromino Type=""T"" Rotation=""1"" X=""3"" Y=""5"" />
<Tetromino Type=""L"" Rotation=""1"" X=""-1"" Y=""0"" />
<Tetromino Type=""L"" Rotation=""1"" X=""1"" Y=""3"" />
<Tetromino Type=""S"" Rotation=""0"" X=""1"" Y=""0"" />
<Tetromino Type=""J"" Rotation=""3"" X=""-1"" Y=""3"" />
<Tetromino Type=""J"" Rotation=""3"" X=""5"" Y=""3"" />
<Tetromino Type=""Z"" Rotation=""1"" X=""3"" Y=""1"" />
<Tetromino Type=""L"" Rotation=""0"" X=""3"" Y=""3"" />
</TetrominoCollage>",
            
            @"<?xml version=""1.0"" encoding=""utf-8""?>
<TetrominoCollage Rows=""6"" Columns=""6"">
    <Tetromino Type=""I"" Rotation=""0"" X=""5"" Y=""3"" />
    <Tetromino Type=""Z"" Rotation=""1"" X=""2"" Y=""3"" />
    <Tetromino  Type=""S"" Rotation=""0"" X=""1"" Y=""0"" />
    <Tetromino Type=""I"" Rotation=""0"" X=""-1"" Y=""3"" />
    <Tetromino Type=""L"" Rotation=""3"" X=""4"" Y=""-1"" />
    <Tetromino Type=""L"" Rotation=""3"" X=""4"" Y=""5"" />
    <Tetromino Type=""Z"" Rotation=""0"" X=""-2"" Y=""2"" />
    <Tetromino Type=""Z"" Rotation=""0"" X=""4"" Y=""2"" />
    <Tetromino Type=""T"" Rotation=""0"" X=""1"" Y=""2"" />
    <Tetromino Type=""Z"" Rotation=""1"" X=""2"" Y=""0"" />
    <Tetromino Type=""T"" Rotation=""2"" X=""0"" Y=""4"" />
    <Tetromino Type=""T"" Rotation=""2"" X=""0"" Y=""-2"" />
    <Tetromino Type=""L"" Rotation=""1"" X=""-1"" Y=""0"" />
</TetrominoCollage>"
,@"<?xml version=""1.0"" encoding=""utf-8""?>
<TetrominoCollage Rows=""8"" Columns=""8""> <Tetromino Scale=""1"" Type=""T"" Rotation=""0"" X=""6"" Y=""6"" />
    <Tetromino Scale=""1"" Type=""S"" Rotation=""0"" X=""3"" Y=""-1"" />
    <Tetromino Scale=""1"" Type=""S"" Rotation=""2"" X=""3"" Y=""6"" />
    <Tetromino Scale=""1"" Type=""T"" Rotation=""0"" X=""-2"" Y=""6"" />
    <Tetromino Scale=""1"" Type=""L"" Rotation=""1"" X=""0"" Y=""7"" />
    <Tetromino Scale=""1"" Type=""L"" Rotation=""1"" X=""0"" Y=""-1"" />
    <Tetromino Scale=""1"" Type=""Z"" Rotation=""1"" X=""1"" Y=""6"" />
    <Tetromino Scale=""1"" Type=""Z"" Rotation=""1"" X=""1"" Y=""-2"" />
    <Tetromino Scale=""1"" Type=""J"" Rotation=""2"" X=""-2"" Y=""4"" />
    <Tetromino Scale=""1"" Type=""J"" Rotation=""2"" X=""6"" Y=""4"" />
    <Tetromino Scale=""1"" Type=""L"" Rotation=""1"" X=""4"" Y=""4"" />
    <Tetromino Scale=""1"" Type=""S"" Rotation=""2"" X=""1"" Y=""4"" />
    <Tetromino Scale=""1"" Type=""I"" Rotation=""1"" X=""2"" Y=""3"" />
    <Tetromino Scale=""1"" Type=""L"" Rotation=""2"" X=""1"" Y=""3"" />
    <Tetromino Scale=""1"" Type=""S"" Rotation=""0"" X=""-1"" Y=""3"" />
    <Tetromino Scale=""1"" Type=""S"" Rotation=""0"" X=""7"" Y=""3"" />
    <Tetromino Scale=""1"" Type=""T"" Rotation=""2"" X=""5"" Y=""2"" />
    <Tetromino Scale=""1"" Type=""O"" Rotation=""0"" X=""2"" Y=""2"" />
    <Tetromino Scale=""1"" Type=""L"" Rotation=""1"" X=""-1"" Y=""0"" />
    <Tetromino Scale=""1"" Type=""I"" Rotation=""0"" X=""4"" Y=""1"" />
    <Tetromino Scale=""1"" Type=""L"" Rotation=""4"" X=""3"" Y=""0"" />
    <Tetromino Scale=""1"" Type=""O"" Rotation=""0"" X=""6"" Y=""0"" />
</TetrominoCollage>"





        };

        public static SKBitmap GetCornerDisplayBitmap_UpperLeft(NominoTheme _theme, int BlockSize = 500)
        {
            return GetCollageBitmap(Upper_Left_CollageXML, _theme, BlockSize,SKColors.Transparent);
        }
        public static SKBitmap GetCornerDisplayBitmap_UpperRight(NominoTheme _theme, int BlockSize = 500)
        {
            return GetCollageBitmap(Upper_Right_CollageXML, _theme, BlockSize,SKColors.Transparent);
        }
        public static SKBitmap GetCornerDisplayBitmap_LowerLeft(NominoTheme _theme, int BlockSize = 500)
        {
            return GetCollageBitmap(Lower_Left_CollageXML, _theme, BlockSize,SKColors.Transparent);
        }
        public static SKBitmap GetCornerDisplayBitmap_LowerRight(NominoTheme _theme, int BlockSize = 500)
        {
            return GetCollageBitmap(Lower_Right_CollageXML, _theme, BlockSize,SKColors.Transparent);
        }
        public static SKBitmap GetCollageBitmap(String[] sXMLStrings,NominoTheme _theme, int BlockSize = 500,SKColor? background = null)
        {
            return GetCollageBitmap(TetrisGame.Choose(sXMLStrings), _theme, BlockSize,background);
        }
        public static SKBitmap GetCollageBitmap(String sXMLString,NominoTheme _theme, int BlockSize = 500,SKColor? background = null)
        {
            XElement RootNode;
            Nomino[][] AddBlocks = GroupNominos(LoadTetrominoCollageFromXMLString(sXMLString, out RootNode));
            int Cols = RootNode.GetAttributeInt("Columns", 6);
            int Rows = RootNode.GetAttributeInt("Rows", 6);
            return GetBackgroundCollage(_theme, (Nomino[][])AddBlocks,Rows, Cols,BlockSize,background);
        }


        public static SKBitmap GetBackgroundCollage(NominoTheme _theme,int BlockSize = 500,SKColor? background = null)
        {
            XElement RootNode;
            Nomino[][] AddBlocks = GroupNominos(LoadTetrominoCollageFromXMLString(TetrisGame.Choose(DefaultBackgroundCollageXML), out RootNode));
            int Cols = RootNode.GetAttributeInt("Columns", 6);
            int Rows = RootNode.GetAttributeInt("Rows", 6);

            return GetBackgroundCollage(_theme, (Nomino[][])AddBlocks,Rows, Cols,BlockSize,background);
        }
        public static SKBitmap GetBackgroundCollage(NominoTheme _theme, Nomino[] Contents,int RowCount,int ColumnCount,SKColor? background = null)
        {
            var Grouped = GroupNominos(Contents);
            return GetBackgroundCollage(_theme, Grouped, RowCount, ColumnCount,100,background);
        }
        
        private static SKColor blackcolor = SKColors.Black;
        public static SKBitmap GetBackgroundCollage(NominoTheme _theme,Nomino[][] Contents,int RowCount,int ColumnCount,int BlockSize = 100,SKColor? background = null)
        {

            background = background ?? SKColors.Black;
            XElement RootNode = null;
            Nomino[][] AddBlocks = Contents;

            int generatedlevel = TetrisGame.StatelessRandomizer.Next(0, 21);
            int Cols = ColumnCount;
            int Rows = RowCount;
            TetrominoCollageRenderer tcr = new TetrominoCollageRenderer(Cols, Rows, BlockSize, BlockSize, generatedlevel, _theme, background.Value);

            //Nomino[][] AddBlocks = new Nomino[][] { new Nomino[] { IBlock, IBlock2 }, new Nomino[] { TBlock1, TBlock2, TBlock3 }, new Nomino[] { SBlock }, new Nomino[] { LBlock1, LBlock2 }, new Nomino[] { JBlock1, JBlock2 }, new Nomino[] { ZBlock1 }, new Nomino[] { LBlock3 } };
            bool doRandomize = TetrisGame.StatelessRandomizer.NextDouble() > 0.5;

            foreach (var groupadd in AddBlocks)
            {
                foreach (var blockadd in groupadd)
                {
                    _theme.ApplyTheme(blockadd, null, tcr._field, NominoTheme.ThemeApplicationReason.FieldSet);
                    tcr.AddNomino(blockadd);
                }
                if (doRandomize) (tcr.DummyHandler.Statistics as TetrisStatistics).SetLineCount(typeof(Tetrominoes.Tetromino_I), TetrisGame.StatelessRandomizer.Next(0, 21));
            }

            SKBitmap CreateBitmap = tcr.Render();




            return CreateBitmap;


        }

        private SKColor ClearColor = SKColors.Transparent;
        public TetrisField Field { get { return _field; } }
        private TetrisField _field = null;
        public NominoTheme Theme { get { return _field.Theme; } set { _field.Theme = value; } }
        public int ColumnCount { get; set; }
        public int RowCount { get; set; }
        public int ColumnWidth { get; set; }
        public int RowHeight { get; set; }

        public void AddNomino(Nomino Source)
        {

            _field.SetGroupToField(Source);

        }
        public IBlockGameCustomizationHandler DummyHandler { get; }
        public TetrominoCollageRenderer(int pColumnCount, int pRowCount, int pColumnWidth, int pRowHeight,int pLevel,NominoTheme _theme,SKColor pClearColor)
        {
            //Note: We can get away with no CustomizationHandler here, as we aren't going to actually be having the Field "do" anything other than use it as a render source.
            ClearColor = pClearColor;
            ColumnCount = pColumnCount;
            RowCount = pRowCount;
            ColumnWidth = pColumnWidth;
            RowHeight = pRowHeight;
            DummyHandler = new FakeRendererHandler(pRowCount, pColumnCount, pLevel);
            _field = new TetrisField(_theme,DummyHandler, RowCount, ColumnCount);
        }
        public SKBitmap Render()
        {
            Size BitmapSize = new Size((int)(ColumnCount+1) * (ColumnWidth), (int)RowHeight* (RowCount+1));
            SKImageInfo info = new SKImageInfo(BitmapSize.Width, BitmapSize.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            SKBitmap BuiltRepresentation = new SKBitmap(info, SKBitmapAllocFlags.ZeroPixels);

            using (SKCanvas DrawRep = new SKCanvas(BuiltRepresentation))
            {
                DrawRep.Clear(ClearColor);
                //var bg = _field.Theme.GetThemePlayFieldBackground(_field, _field.Handler);
                //bg.

                Object grabdata = RenderingProvider.Static.GetExtendedData(_field.GetType(), _field,
                    (o) =>
                        new TetrisFieldDrawSkiaParameters()
                        {
                            Bounds = new SKRect(0, 0, BitmapSize.Width, BitmapSize.Height),
                            COLCOUNT = ColumnCount,
                            ROWCOUNT = RowCount,
                            FieldBitmap = null,
                            LastFieldSave = SKRect.Empty,
                            VISIBLEROWS = RowCount
                        }) ;

                TetrisFieldDrawSkiaParameters parameters = (TetrisFieldDrawSkiaParameters)grabdata;
                parameters.LastFieldSave = parameters.Bounds;
                //if (!Source.GameHandler.AllowFieldImageCache) parameters.FieldBitmap = null;
                RenderingProvider.Static.DrawElement(null, DrawRep, _field, parameters);
            }


            return BuiltRepresentation;

        }

    }
}
