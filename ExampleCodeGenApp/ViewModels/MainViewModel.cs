using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DynamicData;
using ExampleCodeGenApp.Model;
using ExampleCodeGenApp.ViewModels.Nodes;
using NodeNetwork.Toolkit.Group;
using NodeNetwork.Toolkit.Layout;
using NodeNetwork.Toolkit.Layout.ForceDirected;
using NodeNetwork.Toolkit.NodeList;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.Utilities;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels
{
    internal class IOBinding : GroupIOBinding
    {
        public ValueNodeOutputViewModel<T> CreateCompatibleOutput<T>(ValueNodeInputViewModel<T> input)
        {
            return new CodeGenOutputViewModel<T>(((CodeGenPortViewModel)input.Port).PortType)
            {
                Name = input.Name
            };
        }

        public ValueNodeOutputViewModel<IObservableList<T>> CreateCompatibleOutput<T>(ValueListNodeInputViewModel<T> input)
        {
            return new CodeGenOutputViewModel<IObservableList<T>>(((CodeGenPortViewModel)input.Port).PortType);
        }

        public ValueNodeInputViewModel<T> CreateCompatibleInput<T>(ValueNodeOutputViewModel<T> output)
        {
            return new CodeGenInputViewModel<T>(((CodeGenPortViewModel)output.Port).PortType)
            {
                Name = output.Name
            };
        }

        public ValueListNodeInputViewModel<T> CreateCompatibleInput<T>(ValueNodeOutputViewModel<IObservableList<T>> output)
        {
            return new CodeGenListInputViewModel<T>(((CodeGenPortViewModel)output.Port).PortType)
            {
                Name = output.Name
            };
        }

        public void BindOutputToInput<T>(ValueNodeOutputViewModel<T> output, ValueNodeInputViewModel<T> input)
        {
            output.Value = input.ValueChanged.Do(_ =>
            {
                this.ToString();
            });
        }

        public void BindOutputToInput<T>(ValueNodeOutputViewModel<IObservableList<T>> output, ValueListNodeInputViewModel<T> input)
        {
            output.Value = Observable.Return(input.Values);
        }

        public IOBinding(NodeViewModel groupNode, NodeViewModel entranceNode, NodeViewModel exitNode) 
            : base(groupNode, entranceNode, exitNode)
        {
            groupNode.Inputs.Connect().Transform(i =>
            {
                NodeOutputViewModel result = CreateCompatibleOutput((dynamic)i);
                BindOutputToInput((dynamic)result, (dynamic)i);
                return result;
            }).PopulateInto(entranceNode.Outputs);
            groupNode.Outputs.Connect().Transform(o =>
            {
                NodeInputViewModel result = CreateCompatibleInput((dynamic)o);
                BindOutputToInput((dynamic)o, (dynamic)result);
                return result;
            }).PopulateInto(exitNode.Inputs);
        }

        public override NodeInputViewModel AddNewEntranceInput(NodeOutputViewModel candidateOutput)
        {
            NodeInputViewModel input = CreateCompatibleInput((dynamic)candidateOutput);
            GroupNode.Inputs.Add(input);
            return input;
        }

        public override NodeOutputViewModel AddNewEntranceOutput(NodeInputViewModel candidateInput)
        {
            NodeInputViewModel input = AddNewEntranceInput(CreateCompatibleOutput((dynamic)candidateInput));
            int idx = GroupNode.Inputs.Items.IndexOf(input);
            return EntranceNode.Outputs.Items.ElementAt(idx);
        }

        public override NodeInputViewModel AddNewExitInput(NodeOutputViewModel candidateOutput)
        {
            NodeOutputViewModel output = AddNewExitOutput(CreateCompatibleInput((dynamic)candidateOutput));
            int idx = GroupNode.Outputs.Items.IndexOf(output);
            return ExitNode.Inputs.Items.ElementAt(idx);
        }

        public override NodeOutputViewModel AddNewExitOutput(NodeInputViewModel candidateInput)
        {
            NodeOutputViewModel output = CreateCompatibleOutput((dynamic)candidateInput);
            GroupNode.Outputs.Add(output);
            return output;
        }


        public override NodeInputViewModel GetEntranceInput(NodeOutputViewModel entranceOutput)
        {
            return GroupNode.Inputs.Items.ElementAt(EntranceNode.Outputs.Items.IndexOf(entranceOutput));
        }

        public override NodeOutputViewModel GetEntranceOutput(NodeInputViewModel entranceInput)
        {
            return EntranceNode.Outputs.Items.ElementAt(GroupNode.Inputs.Items.IndexOf(entranceInput));
        }

        public override NodeInputViewModel GetExitInput(NodeOutputViewModel exitOutput)
        {
            return ExitNode.Inputs.Items.ElementAt(GroupNode.Outputs.Items.IndexOf(exitOutput));
        }

        public override NodeOutputViewModel GetExitOutput(NodeInputViewModel exitInput)
        {
            return GroupNode.Outputs.Items.ElementAt(ExitNode.Inputs.Items.IndexOf(exitInput));
        }
    }

    class NetworkBreadcrumb : BreadcrumbViewModel
    {
        #region Network
        private NetworkViewModel _network;
        public NetworkViewModel Network
        {
            get => _network;
            set => this.RaiseAndSetIfChanged(ref _network, value);
        }
        #endregion
    }

    public class MainViewModel : ReactiveObject
    {
        #region Network
        private ObservableAsPropertyHelper<NetworkViewModel> _network;
        public NetworkViewModel Network => _network.Value;
        #endregion

        public BreadcrumbBarViewModel NetworkBreadcrumbBar { get; } = new BreadcrumbBarViewModel();
        public NodeListViewModel NodeList { get; } = new NodeListViewModel();
        public CodePreviewViewModel CodePreview { get; } = new CodePreviewViewModel();
        public CodeSimViewModel CodeSim { get; } = new CodeSimViewModel();

        public ReactiveCommand<Unit, Unit> AutoLayout { get; }
		public ReactiveCommand<Unit, Unit> StartAutoLayoutLive { get; }
		public ReactiveCommand<Unit, Unit> StopAutoLayoutLive { get; }

        public ReactiveCommand<Unit, Unit> GroupNodes { get; }
        public ReactiveCommand<Unit, Unit> OpenGroup { get; }

        public MainViewModel()
        {
            this.WhenAnyValue(vm => vm.NetworkBreadcrumbBar.ActiveItem).Cast<NetworkBreadcrumb>()
                .Select(b => b?.Network)
                .ToProperty(this, vm => vm.Network, out _network);
            NetworkBreadcrumbBar.ActivePath.Add(new NetworkBreadcrumb
            {
                Name = "Main",
                Network = new NetworkViewModel()
            });

            ButtonEventNode eventNode = new ButtonEventNode {CanBeRemovedByUser = false};
            Network.Nodes.Add(eventNode);

            //NodeList.AddNodeType(() => new ButtonEventNode());
            NodeList.AddNodeType(() => new ForLoopNode());
            NodeList.AddNodeType(() => new IntLiteralNode());
            NodeList.AddNodeType(() => new PrintNode());
            NodeList.AddNodeType(() => new TextLiteralNode());

            var codeObservable = eventNode.OnClickFlow.Values.Connect().Select(_ => new StatementSequence(eventNode.OnClickFlow.Values.Items));
            codeObservable.BindTo(this, vm => vm.CodePreview.Code);
            codeObservable.BindTo(this, vm => vm.CodeSim.Code);

			ForceDirectedLayouter layouter = new ForceDirectedLayouter();
			var config = new Configuration
			{
				Network = Network,
			};
			AutoLayout = ReactiveCommand.Create(() => layouter.Layout(config, 10000));
			StartAutoLayoutLive = ReactiveCommand.CreateFromObservable(() => 
				Observable.StartAsync(ct => layouter.LayoutAsync(config, ct)).TakeUntil(StopAutoLayoutLive)
			);
			StopAutoLayoutLive = ReactiveCommand.Create(() => { }, StartAutoLayoutLive.IsExecuting);

            var grouper = new NodeGrouper
            {
                GroupNodeFactory = subnet => new GroupNodeViewModel(subnet),
                EntranceNodeFactory = () => new NodeViewModel(),
                ExitNodeFactory = () => new NodeViewModel(),
                SubNetworkFactory = () => new NetworkViewModel(),
                IOBindingFactory = (groupNode, entranceNode, exitNode) =>
                    new IOBinding(groupNode, entranceNode, exitNode)
            };
            GroupNodes = ReactiveCommand.Create(() =>
            {
                grouper.MergeIntoGroup(Network, Network.SelectedNodes.Items);
            }, this.WhenAnyObservable(vm => vm.Network.SelectedNodes.CountChanged).Select(c => c > 1));

            OpenGroup = ReactiveCommand.Create(() =>
            {
                var node = (GroupNodeViewModel)Network.SelectedNodes.Items.First();
                NetworkBreadcrumbBar.ActivePath.Add(new NetworkBreadcrumb
                {
                    Network = node.Subnet,
                    Name = node.Name
                });
            }, this.WhenAnyValue(vm => vm.Network).Select(n => n.SelectedNodes.Connect()).Switch()
                .Select(_ => Network.SelectedNodes.Count == 1 && Network.SelectedNodes.Items.First() is GroupNodeViewModel)
            );
        }
    }
}
