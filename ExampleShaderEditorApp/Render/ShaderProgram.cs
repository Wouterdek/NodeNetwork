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
            if (shaders == null || shaders.Length == 0)
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

        public void SetUniformVector(string argumentName, Vector<double> values)
        {
            Gl.UseProgram(Id);
            int varLoc = FindVariableLocation(argumentName);
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
        }

        public void SetUniformVector(string argumentName, Vector<float> values)
        {
            Gl.UseProgram(Id);
            int varLoc = FindVariableLocation(argumentName);
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
        }

        public void SetUniformMatrix(string argumentName, Matrix<float> values, bool transpose = false)
        {
            float[] array = values.AsColumnMajorArray() ?? values.ToColumnMajorArray();
            SetUniformMatrix(argumentName, array, values.RowCount, values.ColumnCount, transpose);
        }

        public void SetUniformMatrix(string argumentName, Matrix<double> values, bool transpose = false)
        {
            float[] array = (values.AsColumnMajorArray() ?? values.ToColumnMajorArray()).Select(c => (float)c).ToArray();
            SetUniformMatrix(argumentName, array, values.RowCount, values.ColumnCount, transpose);
        }

        public void SetUniformMatrix(string argumentName, Matrix3x3 values, bool transpose = false)
        {
            Gl.UseProgram(Id);
            SetUniformSquareMatrix(FindVariableLocation(argumentName), values.Buffer, 3, transpose);
        }

        public void SetUniformMatrix(string argumentName, Matrix4x4 values, bool transpose = false)
        {
            Gl.UseProgram(Id);
            SetUniformSquareMatrix(FindVariableLocation(argumentName), values.Buffer, 4, transpose);
        }

        public void SetUniformMatrix(string argumentName, IMatrix values, bool transpose = false)
        {
            SetUniformMatrix(argumentName, values.ToArray().Select(c => (float)c).ToArray(), (int)values.Height, (int)values.Width, transpose);
        }

        public void SetUniformMatrix(string argumentName, float[] values, int rowCount, int columnCount, bool transpose = false)
        {
            Gl.UseProgram(Id);
            if (rowCount == columnCount)
            {
                SetUniformSquareMatrix(FindVariableLocation(argumentName), values, rowCount, transpose);
            }
            else
            {
                SetUniformRectangularMatrix(FindVariableLocation(argumentName), values, rowCount, columnCount, transpose);
            }
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

        private int FindVariableLocation(string argumentName)
        {
            int location = Gl.GetUniformLocation(Id, argumentName);
            if (location == -1)
            {
                throw new ArgumentException($"{argumentName} is not a uniform variable in this shader program.");
            }
            return location;
        }

        public void Dispose()
        {
            Gl.DeleteProgram(Id);
        }
    }
}
