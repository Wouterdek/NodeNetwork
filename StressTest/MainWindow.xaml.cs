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

namespace StressTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly NetworkViewModel _network;

        public MainWindow()
        {
            InitializeComponent();
            NNViewRegistrar.RegisterSplat();
            
            _network = new NetworkViewModel();
            _network.Validator = network =>
            {
                bool containsLoops = GraphAlgorithms.FindLoops(network).Any();
                if (containsLoops)
                {
                    return new NetworkValidationResult(false, false, new ErrorMessageViewModel("Network contains loops!"));
                }

                return new NetworkValidationResult(true, true, null);
            };
            _network.Nodes.Add(CreateNode());
            NetworkView.ViewModel = _network;
	        this.WhenAnyValue(v => v.ShowOutputChecky.IsChecked).Subscribe(isChecked =>
		        {
			        _network.Nodes.Items.First().Outputs.Items.ElementAt(0).Visibility =
				        isChecked.Value ? EndpointVisibility.AlwaysVisible : EndpointVisibility.AlwaysHidden;
		        });
        }
        
        public NodeViewModel CreateNode()
        {
            var input = new ValueNodeInputViewModel<int?>
            {
                Name = "A"
            };

            var output = new ValueNodeOutputViewModel<int?>
            {
                Name = "B",
                Value = Observable.CombineLatest(input.ValueChanged, Observable.Return(-1), (i1, i2) => (int?)(i1 ?? i2)+1)
            };

            NodeViewModel node = new NodeViewModel();
			node.Inputs.Add(input);
			node.Outputs.Add(output);
            output.Value.Subscribe(v => node.Name = v.ToString());

            return node;
        }

        private void GenerateNodes(object sender, RoutedEventArgs e)
        {
            _network.Nodes.Clear();

            int maxX = 10;
            int maxY = 10;
            for (int x = 0; x < maxX; x++)
            {
                for (int y = 0; y < maxY; y++)
                {
                    NodeViewModel node = CreateNode();
                    node.Position = new Point(x * 200, y * 200);
                    _network.Nodes.Add(node);
                    Debug.WriteLine($"Added node {(x*maxY)+y}");
                }
            }
        }
        
        private void GenerateConnections(object sender, RoutedEventArgs e)
        {
            var connections = _network.Nodes.Items.Zip(_network.Nodes.Items.Skip(1),
                (node1, node2) => _network.ConnectionFactory(node2.Inputs.Items.ElementAt(0), node1.Outputs.Items.ElementAt(0)));
            _network.Connections.AddRange(connections);
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            _network.Nodes.Clear();
            _network.Connections.Clear();
        }

        private void AutoLayout(object sender, RoutedEventArgs e)
        {
            var layout = new ForceDirectedLayouter();
            layout.Layout(new Configuration
            {
                Network = _network
            }, 1000);
        }
    }
}
