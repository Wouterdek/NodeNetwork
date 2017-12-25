using System;
using OpenGL;

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
            //See also: https://stackoverflow.com/questions/17149728/when-should-Gl.vertexattribpointer-be-called
            uint vertexArrayId = Gl.GenVertexArray();
            Gl.BindVertexArray(vertexArrayId);
            
            //Create vertex positions buffer
            uint vertexBufferId = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)vertexPositions.Length*sizeof(float), vertexPositions, BufferUsage.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);
            Gl.EnableVertexAttribArray(0);

            //Create texture coordinates buffer
            uint textureCoordinatesBuffer = UInt32.MaxValue;
            if (textureCoordinates.Length != 0)
            {
                textureCoordinatesBuffer = Gl.GenBuffer();
                Gl.BindBuffer(BufferTarget.ArrayBuffer, textureCoordinatesBuffer);
                Gl.BufferData(BufferTarget.ArrayBuffer, (uint)textureCoordinates.Length * sizeof(float), textureCoordinates, BufferUsage.StaticDraw);
                Gl.VertexAttribPointer(1, 2, VertexAttribType.Float, false, 0, IntPtr.Zero);
                Gl.EnableVertexAttribArray(1);
            }

            //Create normals buffer
            uint normalsBufferId = UInt32.MaxValue;
            if (normals.Length != 0)
            {
                normalsBufferId = Gl.GenBuffer();
                Gl.BindBuffer(BufferTarget.ArrayBuffer, normalsBufferId);
                Gl.BufferData(BufferTarget.ArrayBuffer, (uint)normals.Length * sizeof(float), normals, BufferUsage.StaticDraw);
                Gl.VertexAttribPointer(2, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);
                Gl.EnableVertexAttribArray(2);
            }

            //Create indices buffer
            uint indicesBufferId = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, indicesBufferId);
            Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint)faceIndices.Length * sizeof(int), faceIndices, BufferUsage.StaticDraw);

            //According to section "5.1.2 Automatic Unbinding of Deleted Objects" and 5.1.3 in the OpenGL spec,
            //you can 'delete' buffers, textures, renderbuffers, ... without actually removing them from GPU memory.
            //When these objects are references by a VAO which is not currently bound to the context, removing them
            //will only delete the 'name' but not the actual object. The objects will be deleted when the last reference
            //to them is removed.

            //Unbind vertex array
            Gl.BindVertexArray(0);

            //Delete buffers when the vertex array is deleted
            Gl.DeleteBuffers(vertexBufferId);
            if (textureCoordinatesBuffer != UInt32.MaxValue)
            {
                Gl.DeleteBuffers(textureCoordinatesBuffer);
            }
            if (normalsBufferId != UInt32.MaxValue)
            {
                Gl.DeleteBuffers(normalsBufferId);
            }
            if (indicesBufferId != UInt32.MaxValue)
            {
                Gl.DeleteBuffers(indicesBufferId);
            }

            return new Mesh(vertexArrayId, faceIndices.Length);
        }

        public uint VertexArrayObjectId { get; }
        public int IndexCount { get; }

        private Mesh(uint vertexArrayObjectId, int indexCount)
        {
            this.VertexArrayObjectId = vertexArrayObjectId;
            this.IndexCount = indexCount;
        }

        public void Dispose()
        {
            Gl.DeleteVertexArrays(VertexArrayObjectId);
        }
    }
}
