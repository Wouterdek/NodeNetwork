using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DynamicData;
using NodeNetwork;
using NodeNetwork.Toolkit;
using NodeNetwork.Toolkit.Layout.ForceDirected;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;
using StressTest.ViewModels.Nodes;

namespace StressTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly NetworkViewModelBuilder<OutputNodeViewModel> NetworkViewModelBulider = new NetworkViewModelBuilder<OutputNodeViewModel>();

        public MainWindow()
        {
            InitializeComponent();


            NetworkViewModelBulider.SuspensionDriver.LoadAll("C:\\Nodes\\Stress\\");
            if (NetworkViewModelBulider.SuspensionDriver.HasExpressions)
            {
                // Load the default state
                NetworkViewModelBulider.LoadDefault();
            }
            else
            {
                // No expressions exist prepare for new
                NetworkViewModelBulider.Clear(new OutputNodeViewModel());
            }
            this.Events().Closing.Subscribe(_ => NetworkViewModelBulider.SuspensionDriver.SaveAll("C:\\Nodes\\Stress\\"));

            NetworkViewModelBulider.OnInitialise.Subscribe(nvm =>
            {
                nvm.NetworkViewModel.Validator = network =>
                {
                    bool containsLoops = GraphAlgorithms.FindLoops(network).Any();
                    if (containsLoops)
                    {
                        return new NetworkValidationResult(false, false, new ErrorMessageViewModel("Network contains loops!"));
                    }

                    return new NetworkValidationResult(true, true, null);
                };

                nvm.Output.Result.ValueChanged
                    .Select(v => (nvm.NetworkViewModel.LatestValidation?.IsValid ?? true) ? v.ToString() : "Error")
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x=> Result.Text = x);
                nvm.NetworkViewModel.IsReadOnly = false;
                nvm.NetworkViewModel.IsZoomEnabled = true;
                NetworkView.ViewModel = nvm.NetworkViewModel;

                this.WhenAnyValue(v => v.ShowOutputChecky.IsChecked).Subscribe(isChecked =>
                    {
                        try
                        {
                            nvm.NetworkViewModel.Nodes.Items.First().Outputs.Items.ElementAt(0).Visibility =
                                  isChecked.Value ? EndpointVisibility.AlwaysVisible : EndpointVisibility.AlwaysHidden;
                        }
                        catch { }
                    });
            });
        }

        

        private void GenerateNodes(object sender, RoutedEventArgs e)
        {
            NetworkViewModelBulider.Clear();

            int maxX = 10;
            int maxY = 10;
            for (int x = 0; x < maxX; x++)
            {
                for (int y = 0; y < maxY; y++)
                {
                    if (((x * maxY) + y) == 99)
                    {
                        NetworkViewModelBulider.AddOutput(new OutputNodeViewModel { Position = new Point(x * 200, y * 200) });
                    }
                    else
                    {
                        DefaultNodeViewModel node = new DefaultNodeViewModel { Position = new Point(x * 200, y * 200) };
                        NetworkViewModelBulider.NetworkViewModel.Nodes.Add(node);
                    }

                    Debug.WriteLine($"Added node {(x * maxY) + y}");
                }
            }
        }

        private void GenerateConnections(object sender, RoutedEventArgs e)
        {
            var connections = NetworkViewModelBulider.NetworkViewModel.Nodes.Items.Zip(NetworkViewModelBulider.NetworkViewModel.Nodes.Items.Skip(1),
                (node1, node2) => NetworkViewModelBulider.NetworkViewModel.ConnectionFactory(node2.Inputs.Items.ElementAt(0), node1.Outputs.Items.ElementAt(0)));
            NetworkViewModelBulider.NetworkViewModel.Connections.AddRange(connections);
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            NetworkViewModelBulider.NetworkViewModel.Nodes.Clear();
            NetworkViewModelBulider.NetworkViewModel.Connections.Clear();
        }

        private async void AutoLayout(object sender, RoutedEventArgs e)
        {
           await NetworkViewModelBulider.AutoLayout.Execute();
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            NetworkViewModelBulider.Save("default");
        }
    }
}
