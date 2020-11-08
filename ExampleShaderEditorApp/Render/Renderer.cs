using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace ExampleShaderEditorApp.Render
{
    public class Renderer
    {
        public void Render(int width, int height, RenderObject root, Camera camera, float time)
        {
            GL.Viewport(0, 0, width, height);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.ClearColor(0, 0, 0, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            var viewMatrix = CreateViewMatrix(camera);
            var projectionMatrix = CreateProjectionMatrix(camera);
            var viewProjectionMatrix = projectionMatrix * viewMatrix;

            RenderChildren(root, viewProjectionMatrix, camera, time);
        }

        private void RenderChildren(RenderObject obj, Matrix4 viewProjectionMatrix, Camera camera, float time)
        {
            foreach (RenderObject child in obj.Children)
            {
                RenderChildren(child, viewProjectionMatrix, camera, time);
            }

            if (obj.Model?.Mesh != null && obj.Model?.Shader != null)
            {
                Vector<double> cameraPos = camera.GetWorldPosition().Subtract(obj.GetWorldPosition());
                RenderModel(obj.Model, viewProjectionMatrix, obj.GetObjectToWorldTransform(), cameraPos, time);
            }
        }

        private void RenderModel(Model model, Matrix4 viewProjectionMatrix, Matrix<double> modelMatrix, Vector<double> cameraPosition, float time)
        {
            if (!model.Shader.SetUniformMatrix("viewProjectionTransformation", viewProjectionMatrix))
            {
                throw new Exception("Vertex shader is missing 'viewProjectionTransformation'");
            }
            if (!model.Shader.SetUniformMatrix("modelTransformation", modelMatrix))
            {
                throw new Exception("Vertex shader is missing 'modelTransformation'");
            }
            model.Shader.SetUniformVector("cameraPos", cameraPosition);
            model.Shader.SetUniformFloat("timeSeconds", time);

            GL.BindVertexArray(model.Mesh.VertexArrayObjectId);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, model.Texture?.Id ?? 0);
            GL.UseProgram(model.Shader.Id);
            
            GL.DrawElements(PrimitiveType.Triangles, model.Mesh.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        private Matrix4 CreateViewMatrix(Camera camera)
        {
            return MathMatrixToOpenglMatrix(camera.BuildViewMatrix());
        }

        private Matrix4 CreateProjectionMatrix(Camera camera)
        {
            var mat = Matrix4.CreatePerspectiveFieldOfView(camera.VerticalFOV, 
                camera.HorizontalFOV / camera.VerticalFOV, camera.NearPlaneZ, camera.FarPlaneZ);
            mat.Transpose();
            return mat;
        }

        //Conversion

        private Matrix<double> OpenGlMatrixToMathMatrix(Matrix4 m)
        {
            return Matrix<double>.Build.DenseOfRowArrays(
                new[] { m.Row0, m.Row1, m.Row2, m.Row3 }.Select(v => new double[] { v.X, v.Y, v.Z, v.W })
            );
        }

        private Matrix4 MathMatrixToOpenglMatrix(Matrix<double> values)
        {
            var vectors = (values.AsRowArrays() ?? values.ToRowArrays())
                    .Select(arr => new Vector4((float)arr[0], (float)arr[1], (float)arr[2], (float)arr[3])).ToList();
            return new Matrix4(
                vectors[0], vectors[1], vectors[2], vectors[3]
            );
        }
    }
}
