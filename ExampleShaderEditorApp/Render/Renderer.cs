using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using OpenGL;

namespace ExampleShaderEditorApp.Render
{
    public class Renderer
    {
        public void Render(int width, int height, RenderObject root, Camera camera)
        {
            Gl.Viewport(0, 0, width, height);

            Gl.Enable(EnableCap.DepthTest);
            Gl.Enable(EnableCap.CullFace);
            Gl.CullFace(CullFaceMode.Back);

            Gl.ClearColor(0, 0, 0, 1);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4x4f viewMatrix = CreateViewMatrix(camera);
            Matrix4x4f projectionMatrix = CreateProjectionMatrix(camera);
            Matrix4x4f viewProjectionMatrix = projectionMatrix * viewMatrix;

            RenderChildren(root, viewProjectionMatrix, camera);
        }

        private void RenderChildren(RenderObject obj, Matrix4x4f viewProjectionMatrix, Camera camera)
        {
            foreach (RenderObject child in obj.Children)
            {
                RenderChildren(child, viewProjectionMatrix, camera);
            }

            if (obj.Model?.Mesh != null && obj.Model?.Shader != null)
            {
                Vector<double> cameraPos = camera.GetWorldPosition().Subtract(obj.GetWorldPosition());
                RenderModel(obj.Model, viewProjectionMatrix, obj.GetObjectToWorldTransform(), cameraPos);
            }
        }

        private void RenderModel(Render.Model model, Matrix4x4f viewProjectionMatrix, Matrix<double> modelMatrix, Vector<double> cameraPosition)
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

            Gl.BindVertexArray(model.Mesh.VertexArrayObjectId);
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2d, model.Texture?.Id ?? 0);
            Gl.UseProgram(model.Shader.Id);
            
            Gl.DrawElements(PrimitiveType.Triangles, model.Mesh.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        private Matrix4x4f CreateViewMatrix(Camera camera)
        {
            Matrix<double> viewMatrix = camera.BuildViewMatrix();
            double[] elements = viewMatrix.AsColumnMajorArray() ?? viewMatrix.ToColumnMajorArray();
            float[] floats = new float[elements.Length];
            for (int i = 0; i < elements.Length; i++)
            {
                floats[i] = (float)elements[i];
            }
            return new Matrix4x4f(floats);
        }

        private Matrix4x4f CreateProjectionMatrix(Camera camera)
        {
            return Matrix4x4f.Perspective((camera.VerticalFOV / (float) Math.PI) * 180f,
                camera.HorizontalFOV / camera.VerticalFOV, camera.NearPlaneZ, camera.FarPlaneZ);
        }
    }
}
