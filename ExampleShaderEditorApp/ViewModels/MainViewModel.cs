using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using ExampleShaderEditorApp.ViewModels.Nodes;
using NodeNetwork;
using NodeNetwork.Toolkit;
using NodeNetwork.Toolkit.NodeList;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace ExampleShaderEditorApp.ViewModels
{
    [DataContract]
    public class MainViewModel : ReactiveObject
    {
        public NodeListViewModel NodeListViewModel { get; } = new NodeListViewModel();
        public NetworkViewModelBuilder<ShaderOutputNodeViewModel> NetworkViewModelBulider { get; private set; } = new NetworkViewModelBuilder<ShaderOutputNodeViewModel>();
        public ShaderPreviewViewModel ShaderPreviewViewModel { get; } = new ShaderPreviewViewModel();
        public ReactiveCommand<string, Unit> Save { get; }
        public ReactiveCommand<string, Unit> Load { get; }

        public MainViewModel()
        {
            NodeListViewModel.AddNodeType(() => new Vec2PackNodeViewModel());
            NodeListViewModel.AddNodeType(() => new Vec2UnpackNodeViewModel());
            NodeListViewModel.AddNodeType(() => new Vec3PackNodeViewModel());
            NodeListViewModel.AddNodeType(() => new Vec3UnpackNodeViewModel());
            NodeListViewModel.AddNodeType(() => new ColorNodeViewModel());
            NodeListViewModel.AddNodeType(() => new GeometryNodeViewModel());
            NodeListViewModel.AddNodeType(() => new MathNodeViewModel());
            NodeListViewModel.AddNodeType(() => new Math2NodeViewModel());
            NodeListViewModel.AddNodeType(() => new Vec3MathNodeViewModel());


            Save = ReactiveCommand.Create<string>(NetworkViewModelBulider.Save);
            Load = ReactiveCommand.Create<string>(NetworkViewModelBulider.Load);
            NetworkViewModelBulider.SuspensionDriver.LoadAll("C:\\Nodes\\Shader\\");
            if (NetworkViewModelBulider.SuspensionDriver.HasExpressions)
            {
                // Load the default state
                NetworkViewModelBulider.LoadDefault();
            }
            else
            {
                // No expressions exist
                NetworkViewModelBulider.Clear(new ShaderOutputNodeViewModel());
            }

            NetworkViewModelBulider.OnInitialise.Subscribe(nvm =>
            {
                nvm.NetworkViewModel.IsReadOnly = false;
                nvm.NetworkViewModel.IsZoomEnabled = true;
                nvm.NetworkViewModel.Validator = network =>
                {
                    bool containsLoops = GraphAlgorithms.FindLoops(network).Any();
                    if (containsLoops)
                    {
                        return new NetworkValidationResult(false, false, new ErrorMessageViewModel("Network contains loops!"));
                    }

                    return new NetworkValidationResult(true, true, null);
                };


                nvm.Output.ColorInput.ValueChanged
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
                        "out vec3 outColor;",
                        "",
                        "void main() {",
                        $"    outColor = {func};",
                        "}"};
                })
                .BindTo(this, vm => vm.ShaderPreviewViewModel.FragmentShaderSource);
            });
        }
    }
}
