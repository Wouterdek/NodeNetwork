using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ExampleCodeGenApp.Model;
using ExampleCodeGenApp.ViewModels.Nodes;
using NodeNetwork.Toolkit.Layout;
using NodeNetwork.Toolkit.Layout.ForceDirected;
using NodeNetwork.Toolkit.NodeList;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        public NetworkViewModel Network { get; } = new NetworkViewModel();
        public NodeListViewModel NodeList { get; } = new NodeListViewModel();
        public CodePreviewViewModel CodePreview { get; } = new CodePreviewViewModel();
        public CodeSimViewModel CodeSim { get; } = new CodeSimViewModel();

        public ReactiveCommand<Unit, Unit> AutoLayout { get; }
		public ReactiveCommand<Unit, Unit> StartAutoLayoutLive { get; }
		public ReactiveCommand<Unit, Unit> StopAutoLayoutLive { get; }

		public MainViewModel()
        {
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
        }
    }
}
