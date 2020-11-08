using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using ExampleShaderEditorApp.ViewModels.Nodes;
using NodeNetwork;
using NodeNetwork.Toolkit;
using NodeNetwork.Toolkit.NodeList;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace ExampleShaderEditorApp.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        public NodeListViewModel NodeListViewModel { get; } = new NodeListViewModel();
        public NetworkViewModel NetworkViewModel { get; } = new NetworkViewModel();
        public ShaderPreviewViewModel ShaderPreviewViewModel { get; } = new ShaderPreviewViewModel();

        public ShaderOutputNodeViewModel ShaderOutputNode { get; } = new ShaderOutputNodeViewModel();

        public MainViewModel()
        {
            NodeListViewModel.AddNodeType(() => new Vec2PackNodeViewModel());
            NodeListViewModel.AddNodeType(() => new Vec2UnpackNodeViewModel());
            NodeListViewModel.AddNodeType(() => new Vec3PackNodeViewModel());
            NodeListViewModel.AddNodeType(() => new Vec3UnpackNodeViewModel());
            NodeListViewModel.AddNodeType(() => new ColorNodeViewModel());
            NodeListViewModel.AddNodeType(() => new GeometryNodeViewModel());
            NodeListViewModel.AddNodeType(() => new TimeNodeViewModel());
            NodeListViewModel.AddNodeType(() => new MathNodeViewModel());
            NodeListViewModel.AddNodeType(() => new Math2NodeViewModel());
            NodeListViewModel.AddNodeType(() => new Vec3MathNodeViewModel());

            NetworkViewModel.Validator = network =>
            {
                bool containsLoops = GraphAlgorithms.FindLoops(network).Any();
                if (containsLoops)
                {
                    return new NetworkValidationResult(false, false, new ErrorMessageViewModel("Network contains loops!"));
                }

                return new NetworkValidationResult(true, true, null);
            };

            NetworkViewModel.Nodes.Add(ShaderOutputNode);
            ShaderOutputNode.ColorInput.ValueChanged
                .Where(shader => shader != null)
                //.Where(_ => NetworkViewModel.LatestValidation.IsValid)
                .Select(shader =>
                {
                    string func = shader.Compile();
                    return new[]{
                        "#version 330 core",
                        "",
                        "in vec3 pos;",
                        "in vec3 norm;",
                        "in vec3 cam;",
                        "in float seconds;",
                        "out vec3 outColor;",
                        "",
                        "void main() {",
                        $"    outColor = {func};",
                        "}"};
                })
                .BindTo(this, vm => vm.ShaderPreviewViewModel.FragmentShaderSource);
        }
    }
}
