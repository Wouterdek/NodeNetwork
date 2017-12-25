using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExampleShaderEditorApp.Render;
using ExampleShaderEditorApp.ViewModels;
using MathNet.Numerics.LinearAlgebra;
using OpenGL;
using ReactiveUI;

namespace ExampleShaderEditorApp.Views
{
    public partial class ShaderPreviewView : IViewFor<ShaderPreviewViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(ShaderPreviewViewModel), typeof(ShaderPreviewView), new PropertyMetadata(null));

        public ShaderPreviewViewModel ViewModel
        {
            get => (ShaderPreviewViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ShaderPreviewViewModel)value;
        }
        #endregion

        private readonly Renderer _renderer = new Renderer();
        private Mesh _suzanne, _cube;
        private Render.Model _previewModel;

        #region VertexShader
        public Shader VertexShader
        {
            get => (Shader)this.GetValue(VertexShaderProperty);
            set
            {
                VertexShader?.Dispose();
                this.SetValue(VertexShaderProperty, value);
            }
        }
        public static readonly DependencyProperty VertexShaderProperty = DependencyProperty.Register(
            "VertexShader", typeof(Shader), typeof(ShaderPreviewView), new PropertyMetadata(null));
        #endregion

        #region FragmentShader
        public Shader FragmentShader
        {
            get => (Shader)this.GetValue(FragmentShaderProperty);
            set
            {
                FragmentShader?.Dispose();
                this.SetValue(FragmentShaderProperty, value);
            }
        }
        public static readonly DependencyProperty FragmentShaderProperty = DependencyProperty.Register(
            "FragmentShader", typeof(Shader), typeof(ShaderPreviewView), new PropertyMetadata(null));
        #endregion
        
        #region ShaderProgram
        public ShaderProgram ShaderProgram
        {
            get => (ShaderProgram)this.GetValue(ShaderProgramProperty);
            set
            {
                ShaderProgram?.Dispose();
                this.SetValue(ShaderProgramProperty, value);
            }
        }
        public static readonly DependencyProperty ShaderProgramProperty = DependencyProperty.Register(
            "ShaderProgram", typeof(ShaderProgram), typeof(ShaderPreviewView), new PropertyMetadata(null));
        #endregion

        public ShaderPreviewView()
        {
            InitializeComponent();
        }

        private void GlControl_ContextCreated(object sender, GlControlEventArgs e)
        {
            LoadMeshes();

            _previewModel = new Render.Model
            {
                Mesh = _suzanne
            };

            this.WhenAnyValue(vm => vm.ViewModel).Where(vm => vm != null).Subscribe(vm =>
            {
                vm.PreviewObject.Model = _previewModel;
            });

            this.WhenAnyValue(v => v.ViewModel.VertexShaderSource)
                .Select(source => Shader.CompileShader(source.Select(s => s + "\n").ToArray(), ShaderType.VertexShader))
                .BindTo(this, v => v.VertexShader);

            this.WhenAnyValue(v => v.ViewModel.FragmentShaderSource)
                .Select(source => Shader.CompileShader(source.Select(s => s + "\n").ToArray(), ShaderType.FragmentShader))
                .BindTo(this, v => v.FragmentShader);

            this.WhenAnyValue(v => v.VertexShader, v => v.FragmentShader)
                .Select(c => ShaderProgram.Link(c.Item1, c.Item2))
                .BindTo(this, vm => vm.ShaderProgram);

            this.WhenAnyValue(v => v.ShaderProgram).Subscribe(shaderProgram => _previewModel.Shader = shaderProgram);
        }

        private void LoadMeshes()
        {
            using (StreamReader stream = new StreamReader(typeof(ShaderPreviewView).Assembly.GetManifestResourceStream("ExampleShaderEditorApp.Resources.Models.cube.obj")))
            {
                _cube = WavefrontLoader.Load(stream);
            }

            using (StreamReader stream = new StreamReader(typeof(ShaderPreviewView).Assembly.GetManifestResourceStream("ExampleShaderEditorApp.Resources.Models.suzanne.obj")))
            {
                _suzanne = WavefrontLoader.Load(stream);
            }
        }

        private void GlControl_OnContextDestroying(object sender, GlControlEventArgs e)
        {
            _suzanne.Dispose();
            _cube.Dispose();
            _previewModel.Shader.Dispose();
        }

        private void GlControl_Render(object sender, GlControlEventArgs e)
        {
            if (ViewModel != null)
            {
                _renderer.Render(glControl.ClientSize.Width, glControl.ClientSize.Height, ViewModel.WorldRoot, ViewModel.ActiveCamera);
            }
        }
    }
}
