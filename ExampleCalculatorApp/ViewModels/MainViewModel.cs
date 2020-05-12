using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using ExampleCalculatorApp.ViewModels.Nodes;
using ExampleCalculatorApp.Views;
using NodeNetwork;
using NodeNetwork.Toolkit;
using NodeNetwork.Toolkit.NodeList;
using NodeNetwork.Toolkit.ValueNode;
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

        /// <summary>
        /// Gets the network view model bulider with the output type of T.
        /// </summary>
        /// <value>
        /// The network view model bulider.
        /// </value>
        public NetworkViewModelBuilder<OutputNodeViewModel> NetworkViewModelBulider { get; private set; } = new NetworkViewModelBuilder<OutputNodeViewModel>();

        #region ValueLabel
        private string _valueLabel;
        public string ValueLabel
        {
            get => _valueLabel;
            set => this.RaiseAndSetIfChanged(ref _valueLabel, value);
        }
        #endregion

        public ReactiveCommand<string, Unit> Save { get; }
        public ReactiveCommand<string, Unit> Load { get; }

        public MainViewModel()
        {
            ListViewModel.AddNodeType(() => new SumNodeViewModel());
            ListViewModel.AddNodeType(() => new SubtractionNodeViewModel());
            ListViewModel.AddNodeType(() => new ProductNodeViewModel());
            ListViewModel.AddNodeType(() => new DivisionNodeViewModel());
            ListViewModel.AddNodeType(() => new ConstantNodeViewModel());

            Save = ReactiveCommand.Create<string>(NetworkViewModelBulider.Save);
            Load = ReactiveCommand.Create<string>(NetworkViewModelBulider.Load);

            NetworkViewModelBulider.SuspensionDriver.LoadAll("C:\\Nodes\\Math\\");
            if (NetworkViewModelBulider.SuspensionDriver.HasExpressions)
            {
                // Load the default state
                NetworkViewModelBulider.LoadDefault();
            }
            else
            {
                // No expressions exist
                NetworkViewModelBulider.Clear(new OutputNodeViewModel());
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

                    bool containsDivisionByZero = GraphAlgorithms.GetConnectedNodesBubbling(nvm.Output)
                        .OfType<DivisionNodeViewModel>()
                        .Any(n => n.Input2.Value == 0);
                    if (containsDivisionByZero)
                    {
                        return new NetworkValidationResult(false, true, new ErrorMessageViewModel("Network contains division by zero!"));
                    }

                    return new NetworkValidationResult(true, true, null);
                };
            
                nvm.Output.Result.ValueChanged
                    .Select(v => (nvm.NetworkViewModel.LatestValidation?.IsValid ?? true) ? v.ToString() : "Error")
                    .BindTo(this, vm => vm.ValueLabel);
            });
        }
    }
}
