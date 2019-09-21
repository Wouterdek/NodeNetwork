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
		/// <summary>
		/// The network whose nodes are to be repositioned.
		/// </summary>
		public NetworkViewModel Network { get; set; }

		/// <summary>
		/// A time modifier that is used to speed up, or slow down, time during the simulation.
		/// A greater time modifier speeds up the physics simulation, at the cost of accuracy and stability.
		/// </summary>
		public float TimeModifier { get; set; } = 3.5f;
		
		/// <summary>
		/// Number of updates per iteration.
		/// Increasing this number increases the accuracy of the physics simulation at the cost of performance.
		/// </summary>
		public int UpdatesPerIteration { get; set; } = 1;

		/// <summary>
		/// How strongly should nodes push eachother away?
		/// A greater NodeRepulsionForce increases the distance between nodes.
		/// </summary>
		public float NodeRepulsionForce { get; set; } = 100;

		/// <summary>
		/// A function that maps each connection onto the equilibrium distance of its corresponding spring.
		/// A greater equilibrium distance increases the distance between the two connected endpoints.
		/// </summary>
		public Func<ConnectionViewModel, double> EquilibriumDistance { get; set; } = conn => 100;

		/// <summary>
		/// A function that maps each connection onto the springiness/stiffness constant of its corresponding spring.
		/// (c.f. Hooke's law)
		/// </summary>
		public Func<ConnectionViewModel, double> SpringConstant { get; set; } = conn => 1;

		/// <summary>
		/// A function that maps each connection onto the strength of its row force.
		/// Since inputs/outputs are on the left/right of a node, networks tend to be layed out horizontally.
		/// The row force is added onto the endpoints of the connection to make the nodes end up in a more horizontal layout.
		/// </summary>
		public Func<ConnectionViewModel, double> RowForce { get; set; } = conn => 100;

		/// <summary>
		/// A function that maps each node onto its mass in the physics simulation.
		/// Greater mass makes the node harder to move.
		/// </summary>
		public Func<NodeViewModel, float> NodeMass { get; set; } = node => 10;

		/// <summary>
		/// The friction coefficient is used to control friction forces in the simulation.
		/// Greater friction makes the simulation converge faster, as it slows nodes down when
		/// they are swinging around. If the friction is too high, the nodes will stop moving before
		/// they reach their optimal position or might not even move at all.
		/// </summary>
		public Func<NodeViewModel, float> FrictionCoefficient { get; set; } = node => 2.5f;

		/// <summary>
		/// A predicate function that specifies whether or not a node is fixed.
		/// Fixed nodes do not get moved in the simulation.
		/// </summary>
		public Func<NodeViewModel, bool> IsFixedNode { get; set; } = node => false;
	}
}
