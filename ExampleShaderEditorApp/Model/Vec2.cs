using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExampleShaderEditorApp.Model
{
    [DataContract]
    public class Vec2
    {
        [DataMember] public float X { get; }
        [DataMember] public float Y { get; }

        public Vec2(float x = 0, float y = 0)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
