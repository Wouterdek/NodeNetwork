using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace ExampleShaderEditorApp.Render
{
    public class WavefrontLoader
    {
        public static Mesh Load(StreamReader reader)
        {
            WavefrontLoader loader = new WavefrontLoader();
            loader.ReadStream(reader);
            return loader.BuildModel();
        }

        private readonly IList<Vector<float>> vertices = new List<Vector<float>>();
        private readonly IList<Vector<float>> textureCoordinates = new List<Vector<float>>();
        private readonly IList<Vector<float>> normals = new List<Vector<float>>();
        
        private readonly IList<int> vertexIndices = new List<int>();
        private readonly IList<int> textureIndices = new List<int>();
        private readonly IList<int> normalIndices = new List<int>();

        private WavefrontLoader() { }

        private void ReadStream(StreamReader reader)
        {
            string curLine;
                while((curLine = reader.ReadLine()) != null){
                curLine = curLine.Trim(); //Remove leading or trailing whitespace
                if (curLine.StartsWith("#"))
                {
                    //This line is a comment, skip
                    continue;
                }
                else if (curLine.StartsWith("v "))
                {
                    LoadVertex(curLine.Substring(2).Split(' '));
                }
                else if (curLine.StartsWith("vt "))
                {
                    LoadTextureCoordinate(curLine.Substring(3).Split(' '));
                }
                else if (curLine.StartsWith("vn "))
                {
                    LoadVertexNormal(curLine.Substring(3).Split(' '));
                }
                else if (curLine.StartsWith("f "))
                {
                    LoadFace(curLine.Substring(2).Split(' '));
                }
            }
        }

        private void LoadVertex(string[] args)
        {
            float x = float.Parse(args[0], CultureInfo.InvariantCulture);
            float y = float.Parse(args[1], CultureInfo.InvariantCulture);
            float z = float.Parse(args[2], CultureInfo.InvariantCulture);
            vertices.Add(Vector.Build.Dense(new []{x, y, z}));
        }

        private void LoadTextureCoordinate(string[] args)
        {
            float x = float.Parse(args[0], CultureInfo.InvariantCulture);
            float y = float.Parse(args[1], CultureInfo.InvariantCulture);
            textureCoordinates.Add(Vector.Build.Dense(new[] { x, y }));
        }

        private void LoadVertexNormal(string[] args)
        {
            float x = float.Parse(args[0], CultureInfo.InvariantCulture);
            float y = float.Parse(args[1], CultureInfo.InvariantCulture);
            float z = float.Parse(args[2], CultureInfo.InvariantCulture);
            normals.Add(Vector.Build.Dense(new[] { x, y, z }));
        }

        private void LoadFace(string[] args)
        {
            foreach (string facePart in args)
            {
                string[] coordParts = facePart.Split('/');

                int vertexIndex = int.Parse(coordParts[0]);

                int textureIndex = 0;
                if (coordParts.Length >= 2 && !coordParts[1].Equals(""))
                {
                    textureIndex = int.Parse(coordParts[1]);
                }

                int normalIndex = 0;
                if (coordParts.Length == 3)
                {
                    normalIndex = int.Parse(coordParts[2]);
                }

                vertexIndices.Add(vertexIndex);
                textureIndices.Add(textureIndex);
                normalIndices.Add(normalIndex);
            }
        }

        private Mesh BuildModel()
        {
            //This method uses the cheap and easy way to fix the 'multiple indices to one index' problem.
            //Easy to implement and relatively cheap, but breaks if a vertex is associated with multiple texture/normal coordinates

            float[] vertexArray = LinearizeVector3List(vertices);
            float[] textureArray = new float[vertices.Count * 2];
            float[] normalArray = new float[vertices.Count * 3];

            int[] indexArray = new int[vertexIndices.Count];
            for (int i = 0; i < vertexIndices.Count; i++)
            {
                indexArray[i] = vertexIndices[i] - 1;
            }

            //Loop over indices
            for (int i = 0; i < vertexIndices.Count; i++)
            {
                int vertexIndex = vertexIndices[i] - 1;
                int textureIndex = textureIndices[i] - 1;
                int normalIndex = normalIndices[i] - 1;

                if (textureIndex != -1)
                {
                    textureArray[(vertexIndex * 2)] = textureCoordinates[textureIndex][0];
                    textureArray[(vertexIndex * 2) + 1] = textureCoordinates[textureIndex][1];
                }

                if (normalIndex != -1)
                {
                    normalArray[(vertexIndex * 3)] = normals[normalIndex][0];
                    normalArray[(vertexIndex * 3) + 1] = normals[normalIndex][1];
                    normalArray[(vertexIndex * 3) + 2] = normals[normalIndex][2];
                }
            }

            return Mesh.LoadMesh(vertexArray, textureArray, normalArray, indexArray);
        }

        private float[] LinearizeVector3List(IList<Vector<float>> vectors)
        {
            float[] arr = new float[vectors.Count * 3];
            for (int i = 0; i < vectors.Count; i++)
            {
                Vector<float> vector = vectors[i];
                arr[(i * 3)] = vector[0];
                arr[(i * 3) + 1] = vector[1];
                arr[(i * 3) + 2] = vector[2];
            }
            return arr;
        }
    }
}
