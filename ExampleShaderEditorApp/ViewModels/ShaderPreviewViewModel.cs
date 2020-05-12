using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ExampleShaderEditorApp.Render;
using ExampleShaderEditorApp.Views;
using MathNet.Numerics.LinearAlgebra;
using ReactiveUI;

namespace ExampleShaderEditorApp.ViewModels
{
    [DataContract]
    public class ShaderPreviewViewModel : ReactiveObject
    {
        [DataMember] public RenderObject WorldRoot { get; set; } = new RenderObject();

        #region ActiveCamera
        [IgnoreDataMember] private Camera _activeCamera;
        [DataMember]
        public Camera ActiveCamera
        {
            get => _activeCamera;
            set => this.RaiseAndSetIfChanged(ref _activeCamera, value);
        }
        #endregion

        #region PreviewObject
        [IgnoreDataMember] private RenderObject _previewObject;
        [DataMember]
        public RenderObject PreviewObject
        {
            get => _previewObject;
            set => this.RaiseAndSetIfChanged(ref _previewObject, value);
        }
        #endregion

        #region VertexShaderSource
        [IgnoreDataMember] private string[] _vertexShaderSource;
        [DataMember]
        public string[] VertexShaderSource
        {
            get => _vertexShaderSource;
            set => this.RaiseAndSetIfChanged(ref _vertexShaderSource, value);
        }
        #endregion

        #region FragmentShaderSource
        [IgnoreDataMember] private string[] _fragmentShaderSource;
        [DataMember]
        public string[] FragmentShaderSource
        {
            get => _fragmentShaderSource;
            set => this.RaiseAndSetIfChanged(ref _fragmentShaderSource, value);
        }
        #endregion
        
        public ShaderPreviewViewModel()
        {
            ActiveCamera = new Camera
            {
                RelativePosition = Vector<double>.Build.Dense(new double[]{0, 0, 1})
            };
            WorldRoot.AddChild(ActiveCamera);

            PreviewObject = new RenderObject();
            WorldRoot.AddChild(PreviewObject);

            VertexShaderSource = ReadResource("ExampleShaderEditorApp.Resources.Shaders.vertex.glsl");
            FragmentShaderSource = ReadResource("ExampleShaderEditorApp.Resources.Shaders.fragment.glsl");
        }

        private string[] ReadResource(string path)
        {
            using (Stream stream = typeof(ShaderPreviewViewModel).Assembly.GetManifestResourceStream(path))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                }
            }
        }
    }
}
