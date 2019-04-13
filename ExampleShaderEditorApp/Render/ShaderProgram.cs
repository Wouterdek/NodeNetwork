using System;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using OpenGL;

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
            uint program = Gl.CreateProgram();

            //Attach all shaders to the new program
            foreach (Shader shader in shaders)
            {
                Gl.AttachShader(program, shader.Id);
            }

            //Link program, merging the list of shaders into one program
            Gl.LinkProgram(program);

            //Check for link errors
            Gl.GetProgram(program, ProgramProperty.LinkStatus, out var linkStatus);
            if (linkStatus == Gl.FALSE)
            {
                StringBuilder logBuilder = new StringBuilder(2048);
                Gl.GetProgramInfoLog(program, logBuilder.Capacity, out var programLogLength, logBuilder);
                string log = logBuilder.ToString(0, programLogLength);
                throw new ArgumentException("Shader program failed to link:\n" + log);
            }

            return new ShaderProgram(program);
        }

        public uint Id { get; }

        private ShaderProgram(uint programId)
        {
            this.Id = programId;
        }

        public bool SetUniformVector(string argumentName, Vector<double> values)
        {
            Gl.UseProgram(Id);
            int varLoc = Gl.GetUniformLocation(Id, argumentName);
            if (varLoc == -1)
            {
                return false;
            }
            
            switch (values.Count)
            {
                case 2:
                    Gl.Uniform2(varLoc, values.AsArray() ?? values.ToArray());
                    break;
                case 3:
                    Gl.Uniform3(varLoc, values.AsArray() ?? values.ToArray());
                    break;
                case 4:
                    Gl.Uniform4(varLoc, values.AsArray() ?? values.ToArray());
                    break;
            }
            return true;
        }

        public bool SetUniformVector(string argumentName, Vector<float> values)
        {
            Gl.UseProgram(Id);
            int varLoc = Gl.GetUniformLocation(Id, argumentName);
            if (varLoc == -1)
            {
                return false;
            }

            switch (values.Count)
            {
                case 2:
                    Gl.Uniform2(varLoc, values.AsArray() ?? values.ToArray());
                    break;
                case 3:
                    Gl.Uniform3(varLoc, values.AsArray() ?? values.ToArray());
                    break;
                case 4:
                    Gl.Uniform4(varLoc, values.AsArray() ?? values.ToArray());
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

        public bool SetUniformMatrix(string argumentName, Matrix3x3f values, bool transpose = false)
        {
            Gl.UseProgram(Id);
            int varLoc = Gl.GetUniformLocation(Id, argumentName);
            if (varLoc == -1)
            {
                return false;
            }

            SetUniformSquareMatrix(varLoc, (float[])values, 3, transpose);
            return true;
        }

        public bool SetUniformMatrix(string argumentName, Matrix4x4f values, bool transpose = false)
        {
            Gl.UseProgram(Id);
            int varLoc = Gl.GetUniformLocation(Id, argumentName);
            if (varLoc == -1)
            {
                return false;
            }

            SetUniformSquareMatrix(varLoc, (float[])values, 4, transpose);
            return true;
        }

        public bool SetUniformMatrix(string argumentName, float[] values, int rowCount, int columnCount, bool transpose = false)
        {
            Gl.UseProgram(Id);
            int varLoc = Gl.GetUniformLocation(Id, argumentName);
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
                    Gl.UniformMatrix2(argument, transpose, values);
                    break;
                case 3:
                    Gl.UniformMatrix3(argument, transpose, values);
                    break;
                case 4:
                    Gl.UniformMatrix4(argument, transpose, values);
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
                        Gl.UniformMatrix2x3(argument, transpose, values);
                    }else if (rows == 4)
                    {
                        Gl.UniformMatrix2x4(argument, transpose, values);
                    }
                    break;
                case 3:
                    if (rows == 2)
                    {
                        Gl.UniformMatrix3x2(argument, transpose, values);
                    }
                    else if (rows == 4)
                    {
                        Gl.UniformMatrix3x4(argument, transpose, values);
                    }
                    break;
                case 4:
                    if (rows == 2)
                    {
                        Gl.UniformMatrix4x2(argument, transpose, values);
                    }
                    else if (rows == 3)
                    {
                        Gl.UniformMatrix4x3(argument, transpose, values);
                    }
                    break;
                default:
                    throw new ArgumentException("Unsupported matrix size");
            }
        }

        public void Dispose()
        {
            Gl.DeleteProgram(Id);
        }
    }
}
