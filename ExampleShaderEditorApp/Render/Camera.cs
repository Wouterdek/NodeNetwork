using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace ExampleShaderEditorApp.Render
{
    public class Camera : RenderObject
    {
        public float NearPlaneZ { get; set; } = 0.01f;
        public float FarPlaneZ { get; set; } = 1000f;
        public float HorizontalFOV { get; set; } = (float)(Math.PI / 2.0);
        public float VerticalFOV { get; set; } = (float)(Math.PI / 2.0);

        internal Matrix<double> BuildViewMatrix()
        {
            Matrix<double> result = Matrix<double>.Build.DenseIdentity(4);
            foreach (var obj in WalkToRoot())
            {
                //Create local affine transformation matrix
                Matrix<double> affineRotation = Matrix<double>.Build.DenseIdentity(4);
                affineRotation.SetSubMatrix(0, 0, RelativeRotation.Invert().GetMatrix());

                Matrix<double> affineTranslation = Matrix<double>.Build.DenseIdentity(4);
                affineTranslation.SetColumn(3, 0, RelativePosition.Count, (-1.0f * RelativePosition));

                Matrix<double> localTransformation = affineTranslation.Multiply(affineRotation);

                result *= localTransformation;
            }

            return result;
        }
    }
}
