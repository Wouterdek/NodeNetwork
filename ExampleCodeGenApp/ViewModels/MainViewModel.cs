using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ExampleCodeGenApp.Model;
using ExampleCodeGenApp.ViewModels.Nodes;
using NodeNetwork.Toolkit.NodeList;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        public NetworkViewModelBuilder<ButtonEventNode> NetworkViewModelBulider { get; } = new NetworkViewModelBuilder<ButtonEventNode>();
        public NodeListViewModel NodeList { get; } = new NodeListViewModel();
        public CodePreviewViewModel CodePreview { get; } = new CodePreviewViewModel();
        public CodeSimViewModel CodeSim { get; } = new CodeSimViewModel();
        public ReactiveCommand<string, Unit> Save { get; }
        public ReactiveCommand<string, Unit> Load { get; }

        public MainViewModel()
        {
            //NodeList.AddNodeType(() => new ButtonEventNode());
            NodeList.AddNodeType(() => new ForLoopNode());
            NodeList.AddNodeType(() => new IntLiteralNode());
            NodeList.AddNodeType(() => new PrintNode());
            NodeList.AddNodeType(() => new TextLiteralNode());

            NetworkViewModelBulider.SuspensionDriver.LoadAll("C:\\Nodes\\Code\\");
            if (NetworkViewModelBulider.SuspensionDriver.HasExpressions)
            {
                // Load the default state
                NetworkViewModelBulider.LoadDefault();
            }
            else
            {
                // No expressions exist
                NetworkViewModelBulider.Clear(new ButtonEventNode { CanBeRemovedByUser = false });
            }

            Save = ReactiveCommand.Create<string>(NetworkViewModelBulider.Save);
            Load = ReactiveCommand.Create<string>(NetworkViewModelBulider.Load);
            
            NetworkViewModelBulider.OnInitialise.Subscribe(nvm =>
            {
                nvm.NetworkViewModel.IsReadOnly = false;
                nvm.NetworkViewModel.IsZoomEnabled = true;
                var codeObservable = nvm.Output.OnClickFlow.Values.Connect().Select(_ => new StatementSequence(nvm.Output.OnClickFlow.Values.Items));
                codeObservable.BindTo(this, vm => vm.CodePreview.Code).DisposeWith(nvm.CleanUp);
                codeObservable.BindTo(this, vm => vm.CodeSim.Code).DisposeWith(nvm.CleanUp);
            });
        }
    }
}
