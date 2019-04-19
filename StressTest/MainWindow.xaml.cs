using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            
            _network = new NetworkViewModel();
			_network.Nodes.Add(CreateNode());
            NetworkView.ViewModel = _network;
	        this.WhenAnyValue(v => v.ShowOutputChecky.IsChecked).Subscribe(isChecked =>
		        {
			        _network.Nodes[0].Outputs.Items.ElementAt(0).Visibility =
				        isChecked.Value ? EndpointVisibility.AlwaysVisible : EndpointVisibility.AlwaysHidden;
		        });
        }
        
        public NodeViewModel CreateNode()
        {
            NodeInputViewModel input = new NodeInputViewModel
            {
                Name = "A"
            };

            NodeOutputViewModel output = new NodeOutputViewModel
            {
                Name = "B"
            };

            NodeViewModel node = new NodeViewModel();
			node.Inputs.Add(input);
			node.Outputs.Add(output);

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
            var connections = _network.Nodes.Zip(_network.Nodes.Skip(1),
                (node1, node2) => _network.ConnectionFactory(node2.Inputs.Items.ElementAt(0), node1.Outputs.Items.ElementAt(0)));
            _network.Connections.AddRange(connections);
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            _network.Nodes.Clear();
            _network.Connections.Clear();
        }
    }
}
