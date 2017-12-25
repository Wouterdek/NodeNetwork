using System;
using System.Text;
using OpenGL;

namespace ExampleShaderEditorApp.Render
{
    public class Shader : IDisposable
    {
        public static Shader CompileShader(string[] source, ShaderType shaderType)
        {
            //Create shader object
            uint shader = Gl.CreateShader(shaderType);
            //Load shader source
            Gl.ShaderSource(shader, source);
            //Compile source code to shader program
            Gl.CompileShader(shader);

            Gl.GetShader(shader, ShaderParameterName.CompileStatus, out var status);
            if (status != Gl.TRUE)
            {
                //Shader failed to compile, get logs
                StringBuilder builder = new StringBuilder(2048);
                Gl.GetShaderInfoLog(shader, builder.Capacity, out var logLength, builder);
                string log = builder.ToString(0, logLength);
                throw new ArgumentException("Shader failed to compile: \n" + log);
            }

            return new Shader(shaderType, shader);
        }

        public ShaderType Type;
        public uint Id { get; }

        private Shader(ShaderType shaderType, uint shaderId)
        {
            Type = shaderType;
            Id = shaderId;
        }

        public void Dispose()
        {
            Gl.DeleteShader(Id);
        }
    }
}
