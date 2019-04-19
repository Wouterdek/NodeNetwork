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

namespace ExampleShaderEditorApp.ViewModels.Nodes
{
    public class ColorNodeViewModel : ShaderNodeViewModel
    {
        static ColorNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<ColorNodeViewModel>));
        }

        public ShaderNodeOutputViewModel ColorOutput { get; } = new ShaderNodeOutputViewModel();

        public ColorNodeViewModel()
        {
            this.Name = "Color";
            this.Category = NodeCategory.Misc;

            ColorEditorViewModel editor = new ColorEditorViewModel();
            ColorOutput.Name = "Color";
            ColorOutput.Editor = editor;
            ColorOutput.ReturnType = typeof(Vec3);
            ColorOutput.Value = editor.ValueChanged;
            this.Outputs.Add(ColorOutput);
        }
    }
}
