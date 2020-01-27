using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using SystemPixelFormat = System.Drawing.Imaging.PixelFormat;
using OpenGLPixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace ExampleShaderEditorApp.Render
{
    public class Texture : IDisposable
    {
        public static Texture LoadTexture(Bitmap img)
        {
            if (img.PixelFormat != SystemPixelFormat.Format32bppArgb)
            {
                throw new ArgumentException("Unsupported bitmat pixel format");
            }

            //Create new OpenGL texture
            int textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            //Load image data into texture
            BitmapData data = img.LockBits(new Rectangle(Point.Empty, img.Size), ImageLockMode.ReadOnly, img.PixelFormat);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, data.Width, data.Height, 0, OpenGLPixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            return new Texture(textureId, img.Width, img.Height);
        }

        public int Id { get; }
        public int Width { get; }
        public int Height { get; }

        private Texture(int id, int width, int height)
        {
            Id = id;
            Width = width;
            Height = height;
        }

        public void Dispose()
        {
            GL.DeleteTextures(1, new[] { Id });
        }
    }
}
