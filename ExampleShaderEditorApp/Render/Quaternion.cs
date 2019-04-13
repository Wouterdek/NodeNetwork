using MathNet.Numerics.LinearAlgebra;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace ExampleShaderEditorApp.Render
{
    public class Quaternion
    {
        public static Quaternion Identity => new Quaternion();

        public UnitVector3D RotationAxis { get; set; } = new UnitVector3D(0, 0, 1);
        public Angle Angle { get; set; } = Angle.FromRadians(0);

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
