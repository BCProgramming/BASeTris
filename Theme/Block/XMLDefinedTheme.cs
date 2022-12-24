using BASeTris.Rendering.Adapters;
using BASeTris.Theme.Block;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BASeTris.Theme
{
    //TODO: lots of work required before this one is "ready".
    //XML reading is good, needs the themestyle implementations that interpret the appropriate information to select the right pixelmaps.


    

    /// <summary>
    /// A CustomPixelTheme that defines the block and colour info through an XML file.
    /// </summary>
    /// There are some tricky parts to this implementation. Primarily, there are different ways themes could be applied. So, we need to have "standard" names/tags for the theme blocks and then have different implementations that
    /// indicate how to apply them. For example, there could be "by level" theme which selects based on the current level like some of the default themes.
    /// //Note: probably need to rewrite this.
    /// 


    ///Needed data implemented here:
    ///Info for mapping from Colors to Pixel Types. This is for loading Bitmaps
    ///Info for mapping pixel types to blocks, with names for each block.
    ///Info for mapping pixel types to colors, with names for each color mapping

    /// needed data via DI:
    /// implemented via a "ThemeStyle" The purpose of a ThemeStyle is to be, well, a style of Theme. Basically, the XML uses names that will be recognized by a particular Theme Style. a Tetris Theme style might for example have color maps for each level, the ThemeStyle would be what recognizes that and
    /// tells this class that it needs to apply a different ColorMap.



    ///<theme Style="Tetris_Per_Level" Name="ThemeName">
    /// <ColorMap>
    /// <ColorDefinition Type="#" Color="#FFFFFF"></ColorDefinition> <!--repeated...-->
    /// </ColorMap>
    /// <!--Blockmaps map pixel types to blocks, with names for each block-->
    /// <Blockmaps>
    /// <Blockmap Name="Standard" src="block_pixeled_red" />
    /// <Blockmap Name="Outlined" src="block_pixeled_red_outline" />
    /// </Blockmaps>
    /// <!--Pixelmaps map pixel types to colors, with names for each mapping-->
    /// <Pixelmaps>
    /// <PixelMap Name="Level_0">
    /// <Pixel Type="Typename" Color="Color"/>
    /// </PixelMap>
    /// </Pixelmaps>
    /// </theme>


    public class XMLDefinedTheme : CustomPixelTheme<int, int>
    {
        private static List<XMLDefinedTheme> _AllThemes = null;
        public static IList<XMLDefinedTheme> AllThemes 
        { get 
            {
                if (_AllThemes == null)
                {
                    _AllThemes = LoadThemes(new String[] { TetrisGame.AppDataFolder }.Concat(TetrisGame.CommandLineDataFolder).ToArray());
                }
                return _AllThemes;
            }
        }
        private static List<XMLDefinedTheme> LoadThemes(String[] SourceBasePaths)
        {
            List<XMLDefinedTheme> BuildContent = new List<XMLDefinedTheme>();
            foreach (var loadbasepath in SourceBasePaths)
            {
                if (loadbasepath == null) continue;
                String ThemePath = Path.Combine(loadbasepath, "CustomThemes");
                if (Directory.Exists(ThemePath))
                {
                    Debug.Print($"Found existent Custom Theme Path {ThemePath}.");
                    DirectoryInfo di = new DirectoryInfo(ThemePath);
                    foreach (var XMLFile in di.GetFiles("*.xml"))
                    {
                        try
                        {
                            XMLDefinedTheme xtheme = new XMLDefinedTheme(XMLFile.FullName);
                            BuildContent.Add(xtheme);
                        }
                        catch (Exception exr)
                        {
                            Debug.Print($"Exception loading custom theme file {XMLFile.FullName}, {exr.Message}");
                            Debug.Print(exr.ToString());
                        }

                    }
                }
                else
                {
                    Debug.Print($"Custom Theme Path {ThemePath} Does not exist.");
                }
            }
            return BuildContent;

        }
        private string _ThemeStyleName = null;
        private String _ThemeName = null;
        public String ThemeName { get { return _ThemeName; } }
        public XMLDefinedTheme(XElement ElementSource,String pBasePath)
        {

        }
        public XMLDefinedTheme(String pXMLFile)
        {

        }
        Dictionary<uint, String> ColorMap = new Dictionary<uint, String>();
        Dictionary<String, uint> ReverseColorMap = new Dictionary<string, uint>();
        Dictionary<String,Dictionary<int, BCColor>> PixelMaps; //map pixel type to color.
        Dictionary<int, String[][]> BlockMap; //map int block type to string bitmap of pixel types.
        Dictionary<String, int> BlockTypeNames = new Dictionary<string, int>();

        


        private void InitializeFromElement(XElement Source, String pBasePath)
        {
            //initialize our theme information from the data in the specified XML Element.
            _ThemeStyleName = Source.Attribute("Style")?.Value;
            _ThemeName = Source.Attribute("Name")?.Value;
            if (_ThemeStyleName == null) throw new InvalidDataException("XML source node does not specify a Theme Style.");
            //information we need to get from the XML file:
            //Info mapping colours to int pixel types, so we can populate a matrix with
            //Info mapping int pixel types to colours
            //info mapping int block types to matrix of pixel types
            //info mapping a Nomino class type and an index to a blocktype int. (eg block 1 of Tetromino_T should be block type 4 defined above)
            //info mapping NominoElement class types to BlockFlags.

            XElement ColorDefinition = Source.Element("ColorMap");
            XElement PixelDefinition = Source.Element("PixelMaps");
            XElement BlockDefinition = Source.Element("BlockMaps");
            (ColorMap,ReverseColorMap) = LoadColorDefinitions(ColorDefinition);
            LoadPixelDefinitions(PixelDefinition);
            LoadBlockDefinitions(BlockDefinition);



        }
        private void LoadPixelDefinitions(XElement Source)
        {
            if (Source == null) return;
            /*<PixelMap Name="name"><Pixel Type="<int>" Color="#FFEEFF"></PixelMap>*/
            if(PixelMaps==null) PixelMaps = new Dictionary<string, Dictionary<int, BCColor>>();
            XAttribute PixelMapNameAttribute = Source.Attribute("Name");
            if (PixelMapNameAttribute == null) throw new InvalidDataException("Pixel Map is missing a name attribute.");
            String sPixelMapName = PixelMapNameAttribute.Value;
            if (PixelMaps.ContainsKey(sPixelMapName)) throw new InvalidDataException($"Pixel map name {sPixelMapName} Appears more than once.");
            Dictionary<int, BCColor> BuildPixelMap = new Dictionary<int, BCColor>();
            foreach(var loadpixel in Source.Elements("Pixel"))
            {
                var IncludeAttribute = loadpixel.Attribute("Include");
                if (IncludeAttribute != null)
                {
                    String sCommonName = IncludeAttribute.Value;
                    if (!PixelMaps.ContainsKey(sCommonName))
                    {
                        throw new InvalidDataException($"Include pixel name {sCommonName} doesn't exist"); 
                    }
                }
                else
                {
                    String sPixelType = loadpixel.Attribute("Type").Value;
                    String sPixelColor = loadpixel.Attribute("Color").Value;

                    if (int.TryParse(sPixelType, out int ptype))
                    {
                        String HexNumber = sPixelColor;
                        var addColor = ParseColor(sPixelColor);
                        BuildPixelMap.Add(ptype, addColor);

                    }
                }
            }
            PixelMaps.Add(sPixelMapName, BuildPixelMap);
        }
        private BCColor ParseColor(String sText)
        {
            String HexNumber = sText;
            KnownColor result;
            if (Enum.TryParse<KnownColor>(sText, true, out result))
            {
                Color useColor = Color.FromKnownColor(result);
                return useColor;
            }
            else
            {
                uint PixelColor = uint.Parse(sText.Replace("#", ""), System.Globalization.NumberStyles.HexNumber);
                return new BCColor(PixelColor);
            }
        }
        private (Dictionary<uint,String>, Dictionary<String,uint>) LoadColorDefinitions(XElement Source)
        {
            if (Source == null) return (null,null);
            //<ColorMaps><ColorMap><ColorDefinition Type="Name" Color="<Color>" /></ColorMap></ColorMaps>
            var createresult = new Dictionary<uint, String>();
            var createreverseresult = new Dictionary<String, uint>();
            foreach(var ColorDef in Source.Elements("ColorDefinition"))
            {
                String sPixelType = ColorDef.Attribute("Type").Value;
                String sPixelColor = ColorDef.Attribute("Color").Value;

                
                    var grabColor = ParseColor(sPixelColor);
                    var PixelColor = grabColor.Value;
                    createresult.Add(PixelColor, sPixelType);
                createreverseresult.Add(sPixelType, PixelColor);
     
            }
            return (createresult,createreverseresult);
        }
        private void LoadBlockDefinitions(XElement Source)
        {
            if (Source == null) return;
            BlockMap = new Dictionary<int, String[][]>();
            //example:
            /*<BlockMap><BlockDefinition Type="<int>"><Row>3,2,1,2,2,2,3,2,1</Row></BlockDefinition></BlockMap>  */
            foreach(var BlockDef in Source.Elements("BlockDefinition"))
            {
                var loaddef = LoadBlockDefinition(BlockDef);
                BlockMap.Add(loaddef.Item1, loaddef.Item2);

            }

        }
        private (int,String[][]) LoadBlockDefinition(XElement Source)
        {
            
            List<String[]> RowList = new List<String[]>();
            String pBlockType = Source.Attribute("Type").Value;
            if(!int.TryParse(pBlockType,out int btype))
            {
                throw new ArgumentException("Invalid Block type " + pBlockType);
            }
            String pBlockName = Source.Attribute("Name")?.Value;
            if (pBlockName == null) throw new InvalidDataException("BlockName is invalid");
            BlockTypeNames.Add(pBlockName, btype);
            var srcAttribute = Source.Attribute("src");
            if (srcAttribute != null)
            {
                SKBitmap sourcebmp = null;
                //load the data from the src attribute value, which will be an image file. needs to be lossless and we map the colors to pixel types using the ColorMap Definitions.
                //also support using existing image data from the Assets. 
                if(TetrisGame.Imageman.HasImage((srcAttribute.Value)))
                {
                    sourcebmp = TetrisGame.Imageman.GetSKBitmap(srcAttribute.Value);
                }
                else if(File.Exists(srcAttribute.Value))
                {
                    using(StreamReader sr = new StreamReader(new FileStream(srcAttribute.Value,FileMode.Open)))
                    {
                        sourcebmp = SKBitmap.Decode(sr.BaseStream);
                    }
                }

                
                for(int y = 0;y<sourcebmp.Height; y++)
                {
                    String[] RowBlocks = new String[sourcebmp.Width];
                    for(int x = 0;x<sourcebmp.Width;x++)
                    {
                        var pixelvalue = sourcebmp.GetPixel(x, y);
                        uint ui = (uint)pixelvalue;
                        if(ColorMap.ContainsKey(ui))
                        {
                            RowBlocks[x] = ColorMap[ui];
                        }
                    }
                    RowList.Add(RowBlocks);
                }


            }
            else
            {

                foreach (var loadrow in Source.Elements("Row"))
                {
                    String pRowData = loadrow.Value;
                    try
                    {
                        String[] RowBlocks = (from p in pRowData.Split(',') where p.Length > 0 select p).ToArray();
                        RowList.Add(RowBlocks);
                    }
                    catch (Exception exr)
                    {
                        throw;
                    }
                    
                }
            }
            
            return (btype,RowList.ToArray());
        }
        public override string Name => this.ThemeName;

        public override BlockFlags GetBlockFlags(Nomino Group, NominoElement element, TetrisField field)
        {
            throw new NotImplementedException();
        }

        public override SKPointI GetBlockSize(TetrisField field, int BlockType)
        {
            String[][] worksize = BlockMap[BlockType];
            return new SKPointI(worksize[0].Length, worksize.Length);
        }

        public override BlockTypeReturnData GetBlockType(Nomino group, NominoElement element, TetrisField field)
        {
            
            throw new NotImplementedException();
        }

        public override Dictionary<int, int[][]> GetBlockTypeDictionary()
        {
            Dictionary<int, int[][]> BuildMap = new Dictionary<int, int[][]>();
            //BlockMap and BlockTypeNames
            foreach (var iterate in BlockMap)
            {
                

            }
            

            throw new NotImplementedException();
        }
        //map array of string array to array of int array based on provided type mapping dictionary.
        
        public override SKColor GetColor(TetrisField field, Nomino Element, NominoElement block, int BlockType, int PixelType)
        {
            throw new NotImplementedException();
        }

        public override int[] PossibleBlockTypes()
        {
            return BlockMap.Keys.ToArray();
        }

        protected override BlockFlags GetBlockFlags(NominoElement testvalue)
        {

            return CustomPixelTheme<int, int>.BlockFlags.Static; //hard-coded as static for now, should 
        }
    }
    public class XMLThemeMapData
    {
        String Name { get; set; }
        
    }
    public class XMLThemeColorMapData
    {
        public int PixelType { get; private set; }
        public BCColor Color { get; private set; }
        public XMLThemeColorMapData(int pPixelType,BCColor pColor)
        {
            this.PixelType = pPixelType;
            this.Color = pColor;
        }
    }
    public abstract class ThemeStyleHandler
    {
    }
}
