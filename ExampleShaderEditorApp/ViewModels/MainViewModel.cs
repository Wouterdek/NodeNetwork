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
using NodeNetwork.Toolkit.ContextMenu;
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

        public AddNodeContextMenuViewModel AddNodeMenuVM { get; } = new AddNodeContextMenuViewModel("Add {0}");
        public AddNodeContextMenuViewModel AddNodeForPendingConnectionMenuVM { get; } = new AddNodeContextMenuViewModel("Add {0}");

        public ReactiveCommand<Unit, Unit> CollapseAllCommand { get; }

        private readonly NodeTemplate[] nodeTemplates = 
        {
            new NodeTemplate(() => new ColorNodeViewModel()),
            new NodeTemplate(() => new GeometryNodeViewModel()),
            new NodeTemplate(() => new Math2NodeViewModel()),
            new NodeTemplate(() => new MathNodeViewModel()),
            new NodeTemplate(() => new Vec2PackNodeViewModel()),
            new NodeTemplate(() => new Vec2UnpackNodeViewModel()),
            new NodeTemplate(() => new Vec3MathNodeViewModel()),
            new NodeTemplate(() => new Vec3PackNodeViewModel()),
            new NodeTemplate(() => new Vec3UnpackNodeViewModel()),
            new NodeTemplate(() => new TimeNodeViewModel())
        };

        public MainViewModel()
        {
            NodeListViewModel.NodeTemplates.AddRange(nodeTemplates);

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

            CollapseAllCommand = ReactiveCommand.Create(() =>
            {
                foreach (var node in NetworkViewModel.Nodes.Items)
                {
                    node.IsCollapsed = true;
                }
            });

            SetupContextMenuViewModels();
        }

        private void SetupContextMenuViewModels()
        {
            AddNodeMenuVM.Network = NetworkViewModel;
            AddNodeMenuVM.AddNodeTypes(nodeTemplates);
            AddNodeMenuVM.Commands.Add(new LabeledCommand
            {
                Label = "Collapse All",
                Command = CollapseAllCommand
            });

            AddNodeForPendingConnectionMenuVM.Network = NetworkViewModel;
            AddNodeForPendingConnectionMenuVM.AddNodeTypes(nodeTemplates);
            NetworkViewModel.OnPendingConnectionDropped = () =>
            {
                var pendingCon = NetworkViewModel.PendingConnection;
                NetworkViewModel.RemovePendingConnection();
                AddNodeForPendingConnectionMenuVM.SearchQuery = "";
                AddNodeForPendingConnectionMenuVM.ShowAddNodeForPendingConnectionMenu(pendingCon);
            };
        }
    }
}
