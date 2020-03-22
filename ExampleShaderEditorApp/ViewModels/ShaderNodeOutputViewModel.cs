using System;
using ExampleShaderEditorApp.Model;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.Views;
using ReactiveUI;

namespace ExampleShaderEditorApp.ViewModels
{
    public class ShaderNodeOutputViewModel : ValueNodeOutputViewModel<ShaderFunc>
    {
        public Type ReturnType { get; set; }
    }
}
