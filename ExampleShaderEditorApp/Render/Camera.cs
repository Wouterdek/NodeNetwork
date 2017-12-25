using System;

namespace ExampleShaderEditorApp.Render
{
    public class Camera : RenderObject
    {
        public float NearPlaneZ { get; set; } = 0.01f;
        public float FarPlaneZ { get; set; } = 1000f;
        public float HorizontalFOV { get; set; } = (float)(Math.PI / 2.0);
        public float VerticalFOV { get; set; } = (float)(Math.PI / 2.0);
    }
}
