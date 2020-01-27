using System;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace ExampleShaderEditorApp.Render
{
    public class Shader : IDisposable
    {
        public static Shader CompileShader(string[] source, ShaderType shaderType)
        {
            //Create shader object
            int shader = GL.CreateShader(shaderType);
            //Load shader source
            GL.ShaderSource(shader, string.Join("\n", source));
            //Compile source code to shader program
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out var status);
            if (status == 0)
            {
                //Shader failed to compile, get logs
                GL.GetShaderInfoLog(shader, out string log);
                throw new ArgumentException("Shader failed to compile: \n" + log);
            }

            return new Shader(shaderType, shader);
        }

        public ShaderType Type;
        public int Id { get; }

        private Shader(ShaderType shaderType, int shaderId)
        {
            Type = shaderType;
            Id = shaderId;
        }

        public void Dispose()
        {
            GL.DeleteShader(Id);
        }
    }
}
