using System;
using OpenTK.Graphics.OpenGL;

namespace ExampleShaderEditorApp.Render
{
    public class Mesh : IDisposable
    {
        public static Mesh LoadMesh(float[] vertexPositions, float[] textureCoordinates, float[] normals, int[] faceIndices)
        {
            //Check that vertexPositions is valid
            if (vertexPositions == null || vertexPositions.Length == 0 || vertexPositions.Length % 3 != 0)
            {
                throw new ArgumentException("vertexPositions must be non-null, non-empty, and have a number of elements equal to a multiple of 3");
            }

            //Check that faceVertexIndices is valid
            if (faceIndices == null || faceIndices.Length == 0 || faceIndices.Length % 3 != 0)
            {
                throw new ArgumentException("faceIndices must be non-null, non-empty, and have a number of elements equal to a multiple of 3");
            }

            //If any of the other arguments is null, make them an empty array instead (easier to work with)
            textureCoordinates = textureCoordinates == null ? new float[0] : textureCoordinates;
            normals = normals == null ? new float[0] : normals;

            //Verify the dimensions of the arrays
            if (textureCoordinates.Length % 2 != 0)
            {
                throw new ArgumentException("textureCoordinates must have a number of elements equal to a multiple of 2");
            }
            else if (normals.Length % 3 != 0)
            {
                throw new ArgumentException("normals must have a number of elements equal to a multiple of 3");
            }

            //Create Vertex Array Object
            //The buffers and their properties will be stored in this buffer:
            //See also: https://stackoverflow.com/questions/17149728/when-should-GL.vertexattribpointer-be-called
            int vertexArrayId = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayId);
            
            //Create vertex positions buffer
            int vertexBufferId = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexPositions.Length*sizeof(float), vertexPositions, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
            GL.EnableVertexAttribArray(0);

            //Create texture coordinates buffer
            int textureCoordinatesBuffer = Int32.MaxValue;
            if (textureCoordinates.Length != 0)
            {
                textureCoordinatesBuffer = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, textureCoordinatesBuffer);
                GL.BufferData(BufferTarget.ArrayBuffer, textureCoordinates.Length * sizeof(float), textureCoordinates, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
                GL.EnableVertexAttribArray(1);
            }

            //Create normals buffer
            int normalsBufferId = Int32.MaxValue;
            if (normals.Length != 0)
            {
                normalsBufferId = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, normalsBufferId);
                GL.BufferData(BufferTarget.ArrayBuffer, normals.Length * sizeof(float), normals, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
                GL.EnableVertexAttribArray(2);
            }

            //Create indices buffer
            int indicesBufferId = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indicesBufferId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, faceIndices.Length * sizeof(int), faceIndices, BufferUsageHint.StaticDraw);

            //According to section "5.1.2 Automatic Unbinding of Deleted Objects" and 5.1.3 in the OpenGL spec,
            //you can 'delete' buffers, textures, renderbuffers, ... without actually removing them from GPU memory.
            //When these objects are references by a VAO which is not currently bound to the context, removing them
            //will only delete the 'name' but not the actual object. The objects will be deleted when the last reference
            //to them is removed.

            //Unbind vertex array
            GL.BindVertexArray(0);

            //Delete buffers when the vertex array is deleted
            GL.DeleteBuffers(1, new[] { vertexBufferId });
            if (textureCoordinatesBuffer != Int32.MaxValue)
            {
                GL.DeleteBuffers(1, new[] { textureCoordinatesBuffer });
            }
            if (normalsBufferId != Int32.MaxValue)
            {
                GL.DeleteBuffers(1, new[] { normalsBufferId });
            }
            if (indicesBufferId != Int32.MaxValue)
            {
                GL.DeleteBuffers(1, new[] { indicesBufferId });
            }

            return new Mesh(vertexArrayId, faceIndices.Length);
        }

        public int VertexArrayObjectId { get; }
        public int IndexCount { get; }

        private Mesh(int vertexArrayObjectId, int indexCount)
        {
            this.VertexArrayObjectId = vertexArrayObjectId;
            this.IndexCount = indexCount;
        }

        public void Dispose()
        {
            GL.DeleteVertexArrays(1, new[] { VertexArrayObjectId });
        }
    }
}
