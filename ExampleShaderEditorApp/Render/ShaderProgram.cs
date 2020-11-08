using System;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace ExampleShaderEditorApp.Render
{
    public class ShaderProgram : IDisposable
    {
        public static ShaderProgram Link(params Shader[] shaders)
        {
            if (shaders == null || shaders.Length == 0 || shaders.Any(s => s == null))
            {
                throw new ArgumentException("'shaders' can not be null or empty");
            }

            //Create new blank shader program
            int program = GL.CreateProgram();

            //Attach all shaders to the new program
            foreach (Shader shader in shaders)
            {
                GL.AttachShader(program, shader.Id);
            }

            //Link program, merging the list of shaders into one program
            GL.LinkProgram(program);

            //Check for link errors
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var linkStatus);
            if (linkStatus == 0)
            {
                GL.GetProgramInfoLog(program, out string log);
                throw new ArgumentException("Shader program failed to link:\n" + log);
            }

            return new ShaderProgram(program);
        }

        public int Id { get; }

        private ShaderProgram(int programId)
        {
            this.Id = programId;
        }

        public bool SetUniformFloat(string argumentName, float value)
        {
            GL.UseProgram(Id);
            int varLoc = GL.GetUniformLocation(Id, argumentName);
            if (varLoc == -1)
            {
                return false;
            }

            GL.Uniform1(varLoc, value);
            return true;
        }

        public bool SetUniformVector(string argumentName, Vector<double> values)
        {
            GL.UseProgram(Id);
            int varLoc = GL.GetUniformLocation(Id, argumentName);
            if (varLoc == -1)
            {
                return false;
            }
            
            switch (values.Count)
            {
                case 2:
                    GL.Uniform2(varLoc, values[0], values[1]);
                    break;
                case 3:
                    GL.Uniform3(varLoc, values[0], values[1], values[2]);
                    break;
                case 4:
                    GL.Uniform4(varLoc, values[0], values[1], values[2], values[3]);
                    break;
            }
            return true;
        }

        public bool SetUniformVector(string argumentName, Vector<float> values)
        {
            GL.UseProgram(Id);
            int varLoc = GL.GetUniformLocation(Id, argumentName);
            if (varLoc == -1)
            {
                return false;
            }

            switch (values.Count)
            {
                case 2:
                    GL.Uniform2(varLoc, values[0], values[1]);
                    break;
                case 3:
                    GL.Uniform3(varLoc, values[0], values[1], values[2]);
                    break;
                case 4:
                    GL.Uniform4(varLoc, values[0], values[1], values[2], values[3]);
                    break;
            }
            return true;
        }

        public bool SetUniformMatrix(string argumentName, Matrix<float> values, bool transpose = false)
        {
            float[] array = values.AsColumnMajorArray() ?? values.ToColumnMajorArray();
            return SetUniformMatrix(argumentName, array, values.RowCount, values.ColumnCount, transpose);
        }

        public bool SetUniformMatrix(string argumentName, Matrix<double> values, bool transpose = false)
        {
            float[] array = (values.AsColumnMajorArray() ?? values.ToColumnMajorArray()).Select(c => (float)c).ToArray();
            return SetUniformMatrix(argumentName, array, values.RowCount, values.ColumnCount, transpose);
        }

        private float[] MatrixToFloatArray(Matrix3 m)
        {
            return new[]
            {
                m.M11, m.M21, m.M31,
                m.M12, m.M22, m.M32,
                m.M13, m.M23, m.M33,
            };
        }

        private float[] MatrixToFloatArray(Matrix4 m)
        {
            return new[]
            {
                m.M11, m.M21, m.M31, m.M41,
                m.M12, m.M22, m.M32, m.M42,
                m.M13, m.M23, m.M33, m.M43,
                m.M14, m.M24, m.M34, m.M44,
            };
        }

        public bool SetUniformMatrix(string argumentName, Matrix3 values, bool transpose = false)
        {
            GL.UseProgram(Id);
            int varLoc = GL.GetUniformLocation(Id, argumentName);
            if (varLoc == -1)
            {
                return false;
            }

            SetUniformSquareMatrix(varLoc, MatrixToFloatArray(values), 3, transpose);
            return true;
        }

        public bool SetUniformMatrix(string argumentName, Matrix4 values, bool transpose = false)
        {
            GL.UseProgram(Id);
            int varLoc = GL.GetUniformLocation(Id, argumentName);
            if (varLoc == -1)
            {
                return false;
            }

            SetUniformSquareMatrix(varLoc, MatrixToFloatArray(values), 4, transpose);
            return true;
        }

        public bool SetUniformMatrix(string argumentName, float[] values, int rowCount, int columnCount, bool transpose = false)
        {
            GL.UseProgram(Id);
            int varLoc = GL.GetUniformLocation(Id, argumentName);
            if (varLoc == -1)
            {
                return false;
            }

            if (rowCount == columnCount)
            {
                SetUniformSquareMatrix(varLoc, values, rowCount, transpose);
            }
            else
            {
                SetUniformRectangularMatrix(varLoc, values, rowCount, columnCount, transpose);
            }
            return true;
        }

        private void SetUniformSquareMatrix(int argument, float[] values, int size, bool transpose)
        {
            switch (size)
            {
                case 2:
                    GL.UniformMatrix2(argument, 1, transpose, values);
                    break;
                case 3:
                    GL.UniformMatrix3(argument, 1, transpose, values);
                    break;
                case 4:
                    GL.UniformMatrix4(argument, 1, transpose, values);
                    break;
                default:
                    throw new ArgumentException("Unsupported matrix size");
            }
        }

        private void SetUniformRectangularMatrix(int argument, float[] values, int rows, int columns, bool transpose)
        {
            switch (columns)
            {
                case 2:
                    if (rows == 3)
                    {
                        GL.UniformMatrix2x3(argument, 1, transpose, values);
                    }else if (rows == 4)
                    {
                        GL.UniformMatrix2x4(argument, 1, transpose, values);
                    }
                    break;
                case 3:
                    if (rows == 2)
                    {
                        GL.UniformMatrix3x2(argument, 1, transpose, values);
                    }
                    else if (rows == 4)
                    {
                        GL.UniformMatrix3x4(argument, 1, transpose, values);
                    }
                    break;
                case 4:
                    if (rows == 2)
                    {
                        GL.UniformMatrix4x2(argument, 1, transpose, values);
                    }
                    else if (rows == 3)
                    {
                        GL.UniformMatrix4x3(argument, 1, transpose, values);
                    }
                    break;
                default:
                    throw new ArgumentException("Unsupported matrix size");
            }
        }

        public void Dispose()
        {
            GL.DeleteProgram(Id);
        }
    }
}
