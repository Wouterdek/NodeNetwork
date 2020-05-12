using MathNet.Numerics.LinearAlgebra;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using System.Runtime.Serialization;

namespace ExampleShaderEditorApp.Render
{
    [DataContract]
    public class Quaternion
    {
        public static Quaternion Identity => new Quaternion();

        [DataMember] public UnitVector3D RotationAxis { get; set; } = UnitVector3D.Create(0, 0, 1);
        [DataMember] public Angle Angle { get; set; } = Angle.FromRadians(0);

        public Matrix<double> GetMatrix()
        {
            return Matrix3D.RotationAroundArbitraryVector(RotationAxis, Angle);
        }

        public Quaternion Invert()
        {
            return new Quaternion
            {
                Angle = -Angle,
                RotationAxis = RotationAxis
            };
        }
    }
}
