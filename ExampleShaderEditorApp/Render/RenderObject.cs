using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ExampleShaderEditorApp.Render
{
    public class RenderObject
    {
        public RenderObject Parent { get; protected set; }

        private readonly List<RenderObject> _children = new List<RenderObject>();
        public IEnumerable<RenderObject> Children => _children;

        #region RelativePosition
        private Vector<double> _relativePosition = Vector.Build.Dense(new double[] { 0, 0, 0 });
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
        private Quaternion _quaternion = Quaternion.Identity;
        public Quaternion RelativeRotation
        {
            get => _quaternion;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                _quaternion = value;
            }
        } 
        #endregion
        
        public Render.Model Model { get; set; }

        public string Name { get; set; }

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
