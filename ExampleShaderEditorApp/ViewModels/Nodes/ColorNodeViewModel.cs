using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using ExampleShaderEditorApp.Model;
using ExampleShaderEditorApp.ViewModels.Editors;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;
using Splat;

namespace ExampleShaderEditorApp.ViewModels.Nodes
{
    public class ColorNodeViewModel : ShaderNodeViewModel
    {
        static ColorNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<ColorNodeViewModel>));
        }

        public ShaderNodeOutputViewModel ColorOutput { get; } = new ShaderNodeOutputViewModel();

        
        private async void LoadIcon()
        {
            // This reloads the icon for each instance of the viewmodel
            // A more efficient implementation would load this once into a static field, then reuse it in each vm instance.
            this.HeaderIcon = await BitmapLoader.Current.LoadFromResource(
                "pack://application:,,,/Resources/Icons/colorwheel.png", 20, 20);
        }

        public ColorNodeViewModel()
        {
            this.Name = "Color";
            this.Category = NodeCategory.Misc;
            LoadIcon();

            ColorEditorViewModel editor = new ColorEditorViewModel();
            ColorOutput.Editor = editor;
            ColorOutput.ReturnType = typeof(Vec3);
            ColorOutput.Value = editor.ValueChanged;
            this.Outputs.Add(ColorOutput);
        }
    }
}
