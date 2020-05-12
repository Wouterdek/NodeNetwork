using System.Runtime.Serialization;

namespace ExampleShaderEditorApp.Render
{
    [DataContract]
    public class Model
    {
        [DataMember] public Mesh Mesh { get; set; }
        [DataMember] public Texture Texture { get; set; }
        [DataMember] public ShaderProgram Shader { get; set; }
    }
}
