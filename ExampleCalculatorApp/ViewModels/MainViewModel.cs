using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCalculatorApp.ViewModels.Nodes;
using ExampleCalculatorApp.Views;
using Newtonsoft.Json;
using NodeNetwork;
using NodeNetwork.Toolkit;
using NodeNetwork.Toolkit.NodeList;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace ExampleCalculatorApp.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        static MainViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new MainWindow(), typeof(IViewFor<MainViewModel>));
        }

        public NodeListViewModel ListViewModel { get; } = new NodeListViewModel();

        #region NetworkViewModel
        private NetworkViewModel _networkViewModel;
        public NetworkViewModel NetworkViewModel
        {
            get => _networkViewModel;
            set => this.RaiseAndSetIfChanged(ref _networkViewModel, value);
        }
        #endregion

        #region ValueLabel
        private string _valueLabel;
        public string ValueLabel
        {
            get => _valueLabel;
            set => this.RaiseAndSetIfChanged(ref _valueLabel, value);
        }
        #endregion

        #region OutputNode
        private readonly ObservableAsPropertyHelper<OutputNodeViewModel> _outputNode;
        public OutputNodeViewModel OutputNode => _outputNode.Value;
        #endregion

        public ReactiveCommand<Unit, NetworkViewModel> LoadFile { get; }
        public ReactiveCommand<Unit, Unit> SaveFile { get; }

        public Interaction<bool, string> SelectFile { get; } = new Interaction<bool, string>();

        public MainViewModel()
        {
            ListViewModel.AddNodeType(() => new SumNodeViewModel());
            ListViewModel.AddNodeType(() => new SubtractionNodeViewModel());
            ListViewModel.AddNodeType(() => new ProductNodeViewModel());
            ListViewModel.AddNodeType(() => new DivisionNodeViewModel());
            ListViewModel.AddNodeType(() => new ConstantNodeViewModel());

            LoadDefaultNetwork();

            this.WhenAnyValue(vm => vm.NetworkViewModel)
                .Select(n => n.Nodes.OfType<OutputNodeViewModel>().First())
                .ToProperty(this, vm => vm.OutputNode, out _outputNode);
            
            this.WhenAnyObservable(vm => vm.OutputNode.ResultInput.ValueChanged)
                .Select(v => (NetworkViewModel.LatestValidation?.IsValid ?? true) ? v.ToString() : "Error")
                .BindTo(this, vm => vm.ValueLabel);

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                TypeNameHandling = TypeNameHandling.All
            };

            LoadFile = ReactiveCommand.CreateFromTask(async () =>
            {
                string file = await SelectFile.Handle(true);
                if (file == null)
                {
                    return null;
                }

                string vmData = File.ReadAllText(file, Encoding.UTF8);
                return JsonConvert.DeserializeObject<NetworkViewModel>(vmData, serializerSettings);
            });
            LoadFile.Where(n => n != null).Subscribe(newNetwork => NetworkViewModel = newNetwork);

            SaveFile = ReactiveCommand.CreateFromTask(async () =>
            {
                string file = await SelectFile.Handle(false);
                if (file != null)
                {
                    string data = JsonConvert.SerializeObject(NetworkViewModel, serializerSettings);
                    File.WriteAllText(file, data, Encoding.UTF8);
                }
            });
        }

        private void LoadDefaultNetwork()
        {
            OutputNodeViewModel output = new OutputNodeViewModel();
            NetworkViewModel = new NetworkViewModel
            {
                Nodes = { output },
                Validator = new CalculatorNetworkValidator
                {
                    OutputNode = output
                }
            };
        }
    }

    class CalculatorNetworkValidator : NetworkValidator
    {
        public OutputNodeViewModel OutputNode { get; set; }
        
        public override NetworkValidationResult Validate(NetworkViewModel network)
        {
            bool containsLoops = GraphAlgorithms.FindLoops(network).Any();
            if (containsLoops)
            {
                return new NetworkValidationResult(false, false, new ErrorMessageViewModel("Network contains loops!"));
            }

            bool containsDivisionByZero = GraphAlgorithms.GetConnectedNodesBubbling(OutputNode)
                .OfType<DivisionNodeViewModel>()
                .Any(n => n.Input2.Value == 0);
            if (containsDivisionByZero)
            {
                return new NetworkValidationResult(false, true, new ErrorMessageViewModel("Network contains division by zero!"));
            }

            return new NetworkValidationResult(true, true, null);
        }
    }
}
