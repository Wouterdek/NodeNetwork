using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleShaderEditorApp.Model
{
    public class Vec2
    {
        public float X { get; }
        public float Y { get; }

        public Vec2(float x = 0, float y = 0)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
