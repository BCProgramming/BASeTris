using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace BASeTris.Rendering.Skia
{
    public static class SkiaExtensions
    {
        //TODO: I think Blur behind might be possible. It looks like we might be able to use an SKRuntimeEffect and a fragment shader.


        public static String BlurShaderCode = @"//""in"" attributes from our vertex shader
varying vec4 vColor;
varying vec2 vTexCoord;

//declare uniforms
uniform sampler2D u_texture;
uniform float resolution;
uniform float radius;
uniform vec2 dir;

void main() {
    //this will be our RGBA sum
    vec4 sum = vec4(0.0);
    
    //our original texcoord for this fragment
    vec2 tc = vTexCoord;
    
    //the amount to blur, i.e. how far off center to sample from 
    //1.0 -> blur by one pixel
    //2.0 -> blur by two pixels, etc.
    float blur = radius/resolution; 
    
    //the direction of our blur
    //(1.0, 0.0) -> x-axis blur
    //(0.0, 1.0) -> y-axis blur
    float hstep = dir.x;
    float vstep = dir.y;
    
    //apply blurring, using a 9-tap filter with predefined gaussian weights
    
    sum += texture2D(u_texture, vec2(tc.x - 4.0*blur*hstep, tc.y - 4.0*blur*vstep)) * 0.0162162162;
    sum += texture2D(u_texture, vec2(tc.x - 3.0*blur*hstep, tc.y - 3.0*blur*vstep)) * 0.0540540541;
    sum += texture2D(u_texture, vec2(tc.x - 2.0*blur*hstep, tc.y - 2.0*blur*vstep)) * 0.1216216216;
    sum += texture2D(u_texture, vec2(tc.x - 1.0*blur*hstep, tc.y - 1.0*blur*vstep)) * 0.1945945946;
    
    sum += texture2D(u_texture, vec2(tc.x, tc.y)) * 0.2270270270;
    
    sum += texture2D(u_texture, vec2(tc.x + 1.0*blur*hstep, tc.y + 1.0*blur*vstep)) * 0.1945945946;
    sum += texture2D(u_texture, vec2(tc.x + 2.0*blur*hstep, tc.y + 2.0*blur*vstep)) * 0.1216216216;
    sum += texture2D(u_texture, vec2(tc.x + 3.0*blur*hstep, tc.y + 3.0*blur*vstep)) * 0.0540540541;
    sum += texture2D(u_texture, vec2(tc.x + 4.0*blur*hstep, tc.y + 4.0*blur*vstep)) * 0.0162162162;

    gl_FragColor = vColor * sum;
}";


        public static SKShader CreateBlurFragmentEffect()
        {
            using var effect = SKRuntimeEffect.Create(BlurShaderCode, out var errorText);


            // input values

            // shader values


            // create actual shader
            var shader = effect.ToShader(false);
            //using var shader = effect.ToShader(inputs, children, true);
            return shader;
            // draw as normal
            //canvas.Clear(SKColors.Black);
            using var paint = new SKPaint { Shader = shader };
            //canvas.DrawRect(SKRect.Create(400, 400), paint);



        }

        public static void SaveToFile(this SKImage bitmap,String sFileName)
        {
            SKEncodedImageFormat imageFormat = SKEncodedImageFormat.Png;
            int quality = (int)100;

            using (MemoryStream memStream = new MemoryStream())
            {

                SKData skd = bitmap.Encode(imageFormat, quality);
                skd.SaveTo(memStream);
                byte[] data = memStream.ToArray();

                if (data == null)
                {
                    return;
                }
                else if (data.Length == 0)
                {
                    return;
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter(new FileStream(sFileName, FileMode.Create)))
                    {
                        sw.BaseStream.Write(data, 0, data.Length);
                    }
                }
            }
            
        }
        public static Color ToColor(this SKColor src)
        {
            return Color.FromArgb(src.Alpha, src.Red, src.Green, src.Blue);
        }
        public static SKColor ToSKColor(this Color src)
        {
            return new SKColor(src.R, src.G, src.B, src.A);
        }
        public static RectangleF ToRectangleF(this SKRect Source)
        {
            return new RectangleF(Source.Left, Source.Top, Source.Width, Source.Height);
        }
        public static void ApplySizedFont(this SKPaint paint, IStateOwner pOwner,float desiredSize,SKColor pColor)
        {
            
            float useSize = desiredSize * (float)pOwner.ScaleFactor;
            var CurrentColor = SKColors.White;
            paint.Typeface = TetrisGame.RetroFontSK;
            paint.TextSize = useSize;
            paint.Color = pColor;
        }
        public static void DrawTextSK(this SKCanvas g, DrawTextInformationSkia DrawData)
        {

            var characterpositions = MeasureCharacterSizes(DrawData.ForegroundPaint, DrawData.Text);
            char[] drawcharacters = DrawData.Text.ToCharArray();
            foreach (int pass in new[] { 1, 2 })
            {
                for (int i = 0; i < drawcharacters.Length; i++)
                {
                    char drawcharacter = drawcharacters[i];
                    SKPoint DrawPosition = new SKPoint(characterpositions[i].Left + DrawData.Position.X, characterpositions[i].Top + DrawData.Position.Y);
                    DrawData.CharacterHandler.DrawCharacter(g, drawcharacter, DrawData, DrawPosition, new SKPoint(characterpositions[i].Size.Width, characterpositions[i].Size.Height), i, drawcharacters.Length, pass);
                }
            }


        }
        public static void DrawTextSK(this SKCanvas Target, String pText, SKPoint Position, SKTypeface typeface, SKColor Color, float DesiredSize, double ScaleFactor)
        {
            var rBytes = UTF8Encoding.Default.GetBytes(pText.ToCharArray());
            using (SKPaint skp = new SKPaint())
            {
                skp.Color = Color;
                /*skp.ColorFilter = SKColorFilter.CreateBlendMode(
                    SkiaSharp.Views.Desktop.Extensions.ToSKColor(
                    System.Drawing.Color.FromArgb(255, 255, 0, 0)),SKBlendMode.Screen);*/
                skp.TextSize = (float)(DesiredSize * ScaleFactor);
                skp.Typeface = typeface;
                skp.FilterQuality = SKFilterQuality.High;
                skp.SubpixelText = true;
                Target.DrawText(rBytes, Position.X, Position.Y, skp);
            }

            //SkiaCanvas.DrawText(UTF8Encoding.Default.GetBytes("testing".ToCharArray()), 67, 67, skp);



        }
        private static List<SKRect> MeasureCharacterSizes(SKPaint skp, String text)
        {
            List<SKRect> results = new List<SKRect>();
            float xpos = 0;
            for (int index = 0; index < text.Length; index++)
            {
                SKRect measuredchar = new SKRect();
                skp.MeasureText("█", ref measuredchar);
                measuredchar = new SKRect(xpos, measuredchar.Top, xpos + measuredchar.Width, measuredchar.Bottom);
                xpos += measuredchar.Width;
                results.Add(measuredchar);

            }
            return results;
        }


        /*
         SKMaskFilter mask = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 50);
        SKImageFilter ImageFilter = SKImageFilter.CreateBlur(5, 5);
        SKPaint paint = new SKPaint() {
            ImageFilter = ImageFilter,
            Color = new SKColor(0, 0, 0, 200),
            MaskFilter = mask,
        };
         
         */
    }

}
