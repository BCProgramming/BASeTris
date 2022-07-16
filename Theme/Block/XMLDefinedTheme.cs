using BASeTris.Rendering.Adapters;
using BASeTris.Theme.Block;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BASeTris.Theme
{


    /// <summary>
    /// A CustomPixelTheme that defines the block and colour info through an XML file.
    /// </summary>
    public class XMLDefinedTheme : CustomPixelTheme<int, int>
    {

        public XMLDefinedTheme(XElement ElementSource,String pBasePath)
        {

        }
        public XMLDefinedTheme(String pXMLFile)
        {

        }
        Dictionary<uint, int> ColorMap;
        Dictionary<int, BCColor> PixelMap; //map pixel type to color.
        Dictionary<int, int[][]> BlockMap; //map block type to int bitmap of pixel types.
        private void InitializeFromElement(XElement Source, String pBasePath)
        {
            //initialize our theme information from the data in the specified XML Element.

            //information we need to get from the XML file:
            //Info mapping colours to int pixel types, so we can populate a matrix with
            //Info mapping int pixel types to colours
            //info mapping int block types to matrix of pixel types
            //info mapping a Nomino class type and an index to a blocktype int. (eg block 1 of Tetromino_T should be block type 4 defined above)
            //info mapping NominoElement class types to BlockFlags.
            XElement ColorDefinition = Source.Element("ColorMap");
            XElement PixelDefinition = Source.Element("PixelMap");
            XElement BlockDefinition = Source.Element("BlockMap");
            LoadColorDefinitions(ColorDefinition);
            LoadPixelDefinitions(PixelDefinition);
            



        }
        private void LoadPixelDefinitions(XElement Source)
        {
            if (Source == null) return;
            /*<PixelMap><Pixel Type="<int>" Color="#FFEEFF"></PixelMap>*/
            PixelMap = new Dictionary<int, BCColor>();
            foreach(var loadpixel in Source.Elements("Pixel"))
            {
                String sPixelType = loadpixel.Attribute("Type").Value;
                String sPixelColor = loadpixel.Attribute("Color").Value;

                if(int.TryParse(sPixelType,out int ptype))
                {
                    

                    uint PixelColor = uint.Parse(sPixelColor.Replace("#",""), System.Globalization.NumberStyles.HexNumber);

                    PixelMap.Add(ptype, new BCColor(PixelColor));
                }
            }
        }
        private void LoadColorDefinitions(XElement Source)
        {
            if (Source == null) return;
            //<ColorMap><ColorDefinition Type="<int>" Color="<Color>" /></ColorMap>
            ColorMap = new Dictionary<uint, int>();
            foreach(var ColorDef in Source.Elements("ColorDefinition"))
            {
                String sPixelType = ColorDef.Attribute("Type").Value;
                String sPixelColor = ColorDef.Attribute("Color").Value;

                if(int.TryParse(sPixelType,out int ptype))
                {
                    uint PixelColor = uint.Parse(sPixelColor.Replace("#", ""), System.Globalization.NumberStyles.HexNumber);

                    ColorMap.Add(PixelColor, ptype);
                }
            }
        }
        private void LoadBlockDefinitions(XElement Source)
        {
            if (Source == null) return;
            BlockMap = new Dictionary<int, int[][]>();
            //example:
            /*<BlockMap><BlockDefinition Type="<int>"><Row>3,2,1,2,2,2,3,2,1</Row></BlockDefinition></BlockMap>  */
            foreach(var BlockDef in Source.Elements("BlockDefinition"))
            {
                var loaddef = LoadBlockDefinition(BlockDef);
                BlockMap.Add(loaddef.Item1, loaddef.Item2);

            }

        }
        private (int,int[][]) LoadBlockDefinition(XElement Source)
        {
            
            List<int[]> RowList = new List<int[]>();
            String pBlockType = Source.Attribute("Type").Value;
            if(!int.TryParse(pBlockType,out int btype))
            {
                throw new ArgumentException("Invalid Block type " + pBlockType);
            }

            var srcAttribute = Source.Attribute("src");
            if (srcAttribute != null)
            {
                SKBitmap sourcebmp = null;
                //load the data from the src attribute value, which will be an image file. needs to be lossless and we map the colors to pixel types using the ColorMap Definitions.
                //first: also support using existing image data from the Assets. 
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
                    int[] RowInts = new int[sourcebmp.Width];
                    for(int x = 0;x<sourcebmp.Width;x++)
                    {
                        var pixelvalue = sourcebmp.GetPixel(x, y);
                        uint ui = (uint)pixelvalue;
                        if(ColorMap.ContainsKey(ui))
                        {
                            RowInts[x] = ColorMap[ui];
                        }
                    }
                    RowList.Add(RowInts);
                }


            }
            else
            {

                foreach (var loadrow in Source.Elements("Row"))
                {
                    String pRowData = loadrow.Value;
                    try
                    {
                        int[] RowInts = (from p in pRowData.Split(',') where p.Length > 0 select int.Parse(p)).ToArray();
                        RowList.Add(RowInts);
                    }
                    catch (Exception exr)
                    {
                        throw;
                    }
                    
                }
            }
            
            return (btype,RowList.ToArray());
        }
        public override string Name => throw new NotImplementedException();

        public override BlockFlags GetBlockFlags(Nomino Group, NominoElement element, TetrisField field)
        {
            throw new NotImplementedException();
        }

        public override SKPointI GetBlockSize(TetrisField field, int BlockType)
        {
            throw new NotImplementedException();
        }

        public override BlockTypeReturnData GetBlockType(Nomino group, NominoElement element, TetrisField field)
        {
            
            throw new NotImplementedException();
        }

        public override Dictionary<int, int[][]> GetBlockTypeDictionary()
        {
            throw new NotImplementedException();
        }

        public override SKColor GetColor(TetrisField field, Nomino Element, NominoElement block, int BlockType, int PixelType)
        {
            throw new NotImplementedException();
        }

        public override int[] PossibleBlockTypes()
        {
            throw new NotImplementedException();
        }

        protected override BlockFlags GetBlockFlags(NominoElement testvalue)
        {
            
            throw new NotImplementedException();
        }
    }
}
