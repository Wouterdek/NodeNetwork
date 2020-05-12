using System;
using System.Linq;
using System.Runtime.Serialization;
using MathNet.Numerics.LinearAlgebra;

namespace ExampleShaderEditorApp.Render
{
    [DataContract]
    public class Camera : RenderObject
    {
        [DataMember] public float NearPlaneZ { get; set; } = 0.01f;
        [DataMember] public float FarPlaneZ { get; set; } = 1000f;
        [DataMember] public float HorizontalFOV { get; set; } = (float)(Math.PI / 2.0);
        [DataMember] public float VerticalFOV { get; set; } = (float)(Math.PI / 2.0);

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
