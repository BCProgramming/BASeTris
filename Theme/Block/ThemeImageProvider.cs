using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BASeTris.Theme.Block.NESTetrominoTheme;

namespace BASeTris.Theme.Block
{
    //ThemeImageProvider's is written against a particular concrete theme implementation. It is a way for an external DI class to basically provide the "bitmap" information
    //used within those themes. This is a second attempt at "extensible" theming, XMLDefinedTheme being the first attempt which encountered a number of issues in implementation.

    //ThemeImageProviders "know" what sort of things the themes request. They service those requests for the image data.

    //Originally the Themes themselves usually had the "bitmap" data coded into them as fields. This abstracts those away. Realistically therefore
    //each Theme should have a default ThemeImageProvider that has whatever used to be coded into the theme itself by default.

    public abstract class ThemeImageProvider<BaseHandler,RequestType,PixelEnum,BlockEnum> : ThemeImageProvider where PixelEnum:Enum where BaseHandler : NominoTheme where RequestType:ThemeImageRequestData
    {
        //given a theme-specific request tag, return the pixel bitmap that matches up.
        public abstract PixelEnum[][] GetBlockPixels(RequestType RequestTag);
        public sealed override int[][] GetBlockPixels(ThemeImageRequestData tird)
        {
            PixelEnum[][] PixelResult = GetBlockPixels((RequestType)tird);
            int[][] intenum = (from e in PixelResult select Array.ConvertAll(e, (v) => Convert.ToInt32(v))).ToArray();
            return intenum;
        }
    }
    public abstract class ThemeImageProvider
    {
        public abstract int[][] GetBlockPixels(ThemeImageRequestData tird);
    }
    public abstract class ThemeImageRequestData
    {
    }

    public class StandardNESThemeImageProvider : ThemeImageProvider<NESTetrominoTheme, StandardNESThemeImageRequestData, NESTetrominoTheme.BCT, NESTetrominoTheme.NESBlockTypes>
    {

        public static BCT[][] DarkerBlock_Core = new BCT[][]
       {
            new []{BCT.Transparent, BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent},
            new []{BCT.Transparent, BCT.Glint, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Glint, BCT.Glint, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Glint, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark}
       };
        public static BCT[][] DarkerBlock = DoubleAndOutline(DarkerBlock_Core);
        public static BCT[][] LighterBlock_Core = new BCT[][]
        {
            new []{BCT.Transparent, BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent},
            new []{BCT.Transparent, BCT.Glint, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light},
            new []{BCT.Transparent, BCT.Base_Light, BCT.Glint, BCT.Glint, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light},
            new []{BCT.Transparent, BCT.Base_Light, BCT.Glint, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light},
            new []{BCT.Transparent, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light},
            new []{BCT.Transparent, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light},
            new []{BCT.Transparent, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light},
            new []{BCT.Transparent, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light},
            new []{BCT.Transparent, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light, BCT.Base_Light}
        };
        public static BCT[][] LighterBlock = DoubleAndOutline(LighterBlock_Core);

        public static BCT[][] CenterWhiteBlock_Core = new BCT[][]
        {
            new []{BCT.Transparent, BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent,BCT.Transparent},
            new []{BCT.Transparent, BCT.Glint, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Glint, BCT.Base_Dark},
            new []{BCT.Transparent, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark, BCT.Base_Dark}
        };
        public static BCT[][] CenterWhiteBlock = DoubleAndOutline(CenterWhiteBlock_Core);


        public override BCT[][] GetBlockPixels(StandardNESThemeImageRequestData Request)
        {
            switch (Request.BlockType)
            {
                case NESBlockTypes.Darker:
                    return DarkerBlock;
                    break;
                case NESBlockTypes.Lighter:
                    return LighterBlock;
                    break;
                case NESBlockTypes.Boxed:
                    return CenterWhiteBlock;

            }
            return null;
        }

        private static BCT[][] DoubleAndOutline(BCT[][] Source)
        {
            return Source;
            /*return (from u in Source select (from f in u select (f==BCT.Transparent?BCT.Black:f)).ToArray()).ToArray(); 
            //not used for now...
            int UseWidth = Source[0].Length*2+2;
            BCT[][] result = new BCT[Source.Length * 2 + 2][];
            //add the top outline.
            result[0] = Enumerable.Repeat<BCT>(BCT.Black, UseWidth).ToArray();
            result[result.Length - 1] = result[0];
            //iterate through each row in the source
            for(int row = 0;row<Source.Length;row++)
            {
                BCT[] buildrow = new BCT[Source.Length * 2 + 2];
                buildrow[0] = BCT.Black;
                buildrow[buildrow.Length - 1] = BCT.Black;
                int currentcopylocation = 0;
                for(int copyindex=0;copyindex<Source[row].Length;copyindex++)
                {
                    //copy this value twice.
                    buildrow[2 + currentcopylocation] = buildrow[1 + currentcopylocation] = Source[row][copyindex];
                    currentcopylocation += 2;
                }
                result[row * 2+1] = result[row * 2 + 2] = buildrow;

            }
            return result;
            */
        }


    }
    public class StandardNESThemeImageRequestData : ThemeImageRequestData
    {
        public int Level { get; set; }
        public NESTetrominoTheme.BCT PixelType { get; set; }
        public NESTetrominoTheme.NESBlockTypes BlockType {get;set;}
    }
}
