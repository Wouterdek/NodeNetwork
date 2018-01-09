using System;
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

            IMatrix4x4 viewMatrix = CreateViewMatrix(camera);
            IMatrix4x4 projectionMatrix = CreateProjectionMatrix(camera);
            IMatrix4x4 viewProjectionMatrix = projectionMatrix.Multiply(viewMatrix);

            RenderChildren(root, viewProjectionMatrix, camera);
        }

        private void RenderChildren(RenderObject obj, IMatrix4x4 viewProjectionMatrix, Camera camera)
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

        private void RenderModel(Render.Model model, IMatrix4x4 viewProjectionMatrix, Matrix<double> modelMatrix, Vector<double> cameraPosition)
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

        private IMatrix4x4 CreateViewMatrix(Camera camera)
        {
            Matrix<double> objectToWorldTransform = camera.GetObjectToWorldTransform();
            IModelMatrix modelMat = new ModelMatrixDouble(objectToWorldTransform.AsColumnMajorArray() ?? objectToWorldTransform.ToColumnMajorArray());
            Vector<double> worldPosition = objectToWorldTransform.Multiply(Vector<double>.Build.Dense(new double[] {0, 0, 0, 1}));

            IModelMatrix viewMat = new ModelMatrix();
            viewMat.Rotate(modelMat.Rotation);
            viewMat.Translate(-worldPosition[0], -worldPosition[1], -worldPosition[2]);
            return viewMat;
        }

        private Matrix4x4 CreateProjectionMatrix(Camera camera)
        {
            return new PerspectiveProjectionMatrix((camera.VerticalFOV / (float)Math.PI) * 180f, camera.HorizontalFOV / camera.VerticalFOV, camera.NearPlaneZ, camera.FarPlaneZ);
        }
    }
}
