using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using DynamicData;
using ExampleShaderEditorApp.Model;
using NodeNetwork.Views;
using ReactiveUI;

namespace ExampleShaderEditorApp.ViewModels.Nodes
{
    public class TimeNodeViewModel : ShaderNodeViewModel
    {
        static TimeNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<TimeNodeViewModel>));
        }

        public ShaderNodeOutputViewModel Result { get; } = new ShaderNodeOutputViewModel();

        public TimeNodeViewModel()
        {
            this.Name = "Time";
            this.Category = NodeCategory.Misc;

            Result.Name = "Seconds";
            Result.ReturnType = typeof(float);
            Result.Value = Observable.Return(new ShaderFunc(() => "seconds"));
            Outputs.Add(Result);
        }
    }
}
