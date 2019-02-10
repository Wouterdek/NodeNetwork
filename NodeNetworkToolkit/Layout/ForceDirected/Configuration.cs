using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNetwork.ViewModels;

namespace NodeNetwork.Toolkit.Layout.ForceDirected
{
	public class Configuration
	{
		public NetworkViewModel Network { get; set; }
		public float TimeModifier { get; set; } = 2.5f;
		public int UpdatesPerIteration { get; set; } = 1;
		public float NodeRepulsionForce { get; set; } = 100;
		public Func<ConnectionViewModel, double> EquilibriumDistance { get; set; } = conn => 100;
		public Func<ConnectionViewModel, double> SpringConstant { get; set; } = conn => 1;
		public Func<ConnectionViewModel, double> RowForce { get; set; } = conn => 100;
		public Func<NodeViewModel, float> NodeMass { get; set; } = node => 10;
		public Func<NodeViewModel, float> FrictionCoefficient { get; set; } = node => 0.93f;
		public Func<NodeViewModel, bool> IsFixedNode { get; set; } = node => false;
	}
}
