using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenGL;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace ExampleShaderEditorApp.Render
{
    public class Texture : IDisposable
    {
        public static Texture LoadTexture(Bitmap img)
        {
            if (img.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new ArgumentException("Unsupported bitmat pixel format");
            }

            //Create new OpenGL texture
            uint textureId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2d, textureId);
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, Gl.CLAMP_TO_EDGE);
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, Gl.CLAMP_TO_EDGE);
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, Gl.LINEAR);
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, Gl.LINEAR);

            //Load image data into texture
            BitmapData data = img.LockBits(new Rectangle(Point.Empty, img.Size), ImageLockMode.ReadOnly, img.PixelFormat);
            Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba8, data.Width, data.Height, 0, OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            return new Texture(textureId, img.Width, img.Height);
        }

        public uint Id { get; }
        public int Width { get; }
        public int Height { get; }

        private Texture(uint id, int width, int height)
        {
            Id = id;
            Width = width;
            Height = height;
        }

        public void Dispose()
        {
            Gl.DeleteTextures(Id);
        }
    }
}
