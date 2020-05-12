using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExampleShaderEditorApp.Model
{
    [DataContract]
    public class Vec3
    {
        [DataMember] public float X { get; }
        [DataMember] public float Y { get; }
        [DataMember] public float Z { get; }

        public Vec3(float x = 0, float y = 0, float z = 0)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }
}
