using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
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
using System.Windows.Threading;
using ExampleShaderEditorApp.Render;
using ExampleShaderEditorApp.ViewModels;
using MathNet.Numerics.LinearAlgebra;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
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
            nameof(VertexShader), typeof(Shader), typeof(ShaderPreviewView), new PropertyMetadata(null));
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
            nameof(FragmentShader), typeof(Shader), typeof(ShaderPreviewView), new PropertyMetadata(null));
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
            nameof(ShaderProgram), typeof(ShaderProgram), typeof(ShaderPreviewView), new PropertyMetadata(null));
        #endregion

        private CompositeDisposable _disposable;

        private GLControl glControl;

        private readonly DateTime startTime;

        public ShaderPreviewView()
        {
            InitializeComponent();
            startTime = DateTime.Now;

            glControl = new GLControl(new GraphicsMode(new ColorFormat(24), 24), 3, 3, GraphicsContextFlags.Default)
            {
                Dock = System.Windows.Forms.DockStyle.Fill
            };
            glControl.Paint += GlControl_Render;

            InitContext();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(16);
            timer.Tick += (s, e) => {
                glControl.Invalidate();
            };
            timer.Start();

            this.winformsHost.Child = glControl;
        }

        private void InitContext()
        {
            glControl.MakeCurrent();

            LoadMeshes();

            _previewModel = new Render.Model
            {
                Mesh = _suzanne
            };

            _disposable = new CompositeDisposable();

            this.WhenAnyValue(vm => vm.ViewModel).Where(vm => vm != null).Subscribe(vm =>
            {
                vm.PreviewObject.Model = _previewModel;
            }).DisposeWith(_disposable);

            this.WhenAnyValue(v => v.ViewModel.VertexShaderSource)
                .Select(source => Shader.CompileShader(source.Select(s => s + "\n").ToArray(), ShaderType.VertexShader))
                .BindTo(this, v => v.VertexShader)
                .DisposeWith(_disposable);

            this.WhenAnyValue(v => v.ViewModel.FragmentShaderSource)
                .Select(source => Shader.CompileShader(source.Select(s => s + "\n").ToArray(), ShaderType.FragmentShader))
                .BindTo(this, v => v.FragmentShader)
                .DisposeWith(_disposable);

            this.WhenAnyValue(v => v.VertexShader, v => v.FragmentShader)
                .Where(t => t.Item1 != null && t.Item2 != null)
                .Select(c => ShaderProgram.Link(c.Item1, c.Item2))
                .BindTo(this, vm => vm.ShaderProgram)
                .DisposeWith(_disposable);

            this.WhenAnyValue(v => v.ShaderProgram)
                .Subscribe(shaderProgram => _previewModel.Shader = shaderProgram)
                .DisposeWith(_disposable);
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

        /*
        TODO: should be called when the GlControl context is destroyed, but there seems to be no event available.
        private void DestroyContext(object sender, EventArgs e)
        {
            _suzanne.Dispose();
            _cube.Dispose();
            _previewModel.Shader.Dispose();
            _disposable.Dispose();
        }
        */

        private void GlControl_Render(object sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                glControl.MakeCurrent();
                
                float seconds = ((float)(DateTime.Now - startTime).TotalMilliseconds)/1000f;
                _renderer.Render(glControl.ClientSize.Width, glControl.ClientSize.Height, ViewModel.WorldRoot, ViewModel.ActiveCamera, seconds);
                glControl.SwapBuffers();
            }
        }
    }
}
