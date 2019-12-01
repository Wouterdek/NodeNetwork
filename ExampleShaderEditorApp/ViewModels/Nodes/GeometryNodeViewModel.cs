using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using ExampleShaderEditorApp.Model;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;
using Splat;

namespace ExampleShaderEditorApp.ViewModels.Nodes
{
    public class GeometryNodeViewModel : ShaderNodeViewModel
    {
        static GeometryNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<GeometryNodeViewModel>));
        }

        public ShaderNodeOutputViewModel VertexPositionOutput { get; } = new ShaderNodeOutputViewModel();
        public ShaderNodeOutputViewModel NormalOutput { get; } = new ShaderNodeOutputViewModel();
        public ShaderNodeOutputViewModel CameraOutput { get; } = new ShaderNodeOutputViewModel();

        private async void LoadIcons()
        {
            // This reloads the icons for each instance of the viewmodel
            // A more efficient implementation would load these once into a static field, then reuse it in each vm instance.
            VertexPositionOutput.Icon = await BitmapLoader.Current.LoadFromResource(
                "pack://application:,,,/Resources/Icons/pos.png", 20, 20);
            NormalOutput.Icon = await BitmapLoader.Current.LoadFromResource(
                "pack://application:,,,/Resources/Icons/norm.png", 20, 20);
            CameraOutput.Icon = await BitmapLoader.Current.LoadFromResource(
                "pack://application:,,,/Resources/Icons/eye.png", 20, 20);
        }

        public GeometryNodeViewModel()
        {
            this.Name = "Geometry";
            this.Category = NodeCategory.Misc;
            LoadIcons();

            VertexPositionOutput.Name = "Position";
            VertexPositionOutput.ReturnType = typeof(Vec3);
            VertexPositionOutput.Value = Observable.Return(new ShaderFunc(() => "pos"));
            VertexPositionOutput.Editor = null;
            this.Outputs.Add(VertexPositionOutput);

            NormalOutput.Name = "Normal";
            NormalOutput.ReturnType = typeof(Vec3);
            NormalOutput.Value = Observable.Return(new ShaderFunc(() => "norm"));
            NormalOutput.Editor = null;
            this.Outputs.Add(NormalOutput);

            CameraOutput.Name = "Camera";
            CameraOutput.ReturnType = typeof(Vec3);
            CameraOutput.Value = Observable.Return(new ShaderFunc(() => "cam"));
            CameraOutput.Editor = null;
            this.Outputs.Add(CameraOutput);
        }
    }
}
