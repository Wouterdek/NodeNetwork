using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ExampleShaderEditorApp.Render
{
    [DataContract]
    public class RenderObject
    {
        [DataMember] public RenderObject Parent { get; protected set; }

        [IgnoreDataMember] private readonly List<RenderObject> _children = new List<RenderObject>();
        [DataMember] public IEnumerable<RenderObject> Children => _children;

        #region RelativePosition
        [IgnoreDataMember] private Vector<double> _relativePosition = Vector.Build.Dense(new double[] { 0, 0, 0 });
        [DataMember]
        public Vector<double> RelativePosition
        {
            get => _relativePosition;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                if (value.Count != 3)
                {
                    throw new ArgumentException("The position vector must be of size 3");
                }
                _relativePosition = value;
            }
        }
        #endregion

        #region RelativeRotation
        [IgnoreDataMember] private Quaternion _quaternion = Quaternion.Identity;
        [DataMember]
        public Quaternion RelativeRotation
        {
            get => _quaternion;
            set => _quaternion = value ?? throw new ArgumentNullException();
        }
        #endregion

        [DataMember] public Render.Model Model { get; set; }

        [DataMember] public string Name { get; set; }

        public void AddChild(RenderObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            if (obj.Parent != null)
            {
                throw new ArgumentException("An object cannot have more than one parent");
            }

            _children.Add(obj);
            obj.Parent = this;
        }

        public bool RemoveChild(RenderObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (_children.Remove(obj))
            {
                obj.Parent = null;
                return true;
            }
            return false;
        }

        public Matrix<double> GetObjectToWorldTransform()
        {
            //Create local affine transformation matrix
            Matrix<double> affineRotation = Matrix<double>.Build.DenseIdentity(4);
            affineRotation.SetSubMatrix(0, 0, RelativeRotation.GetMatrix());

            Matrix<double> affineTranslation = Matrix<double>.Build.DenseIdentity(4);
            affineTranslation.SetColumn(3, 0, RelativePosition.Count, RelativePosition);

            Matrix<double> localTransformation = affineTranslation.Multiply(affineRotation);

            if (Parent != null)
            {
                Matrix<double> parentTransform = Parent.GetObjectToWorldTransform();
                return parentTransform.Multiply(localTransformation);
            }

            return localTransformation;
        }

        public IEnumerable<RenderObject> WalkToRoot()
        {
            yield return this;
            if (Parent != null)
            {
                foreach (var obj in Parent.WalkToRoot())
                {
                    yield return obj;
                }
            }
        }

        public Vector<double> GetWorldPosition()
        {
            return GetObjectToWorldTransform()
                .Multiply(Vector.Build.Dense(new double[] {0, 0, 0, 1}))
                .SubVector(0, 3);
        }
    }
}
