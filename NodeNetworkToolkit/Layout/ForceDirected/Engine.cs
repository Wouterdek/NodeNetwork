using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NodeNetwork.ViewModels;

namespace NodeNetwork.Toolkit.Layout.ForceDirected
{
	internal class Engine
    {
        internal void ApplyRandomShift(NetworkViewModel network)
        {
            Random random = new Random();
            foreach (var node in network.Nodes.Items)
            {
                node.Position = node.Position + new Vector(random.NextDouble(), random.NextDouble());
            }
        }

		internal void Update(int deltaTMillis, IState state, Configuration config)
		{
			// Calculate forces
			int nodeCount = config.Network.Nodes.Count;
			IList<(NodeViewModel, Vector)> nodeForces = new List<(NodeViewModel, Vector)>(nodeCount);

			foreach (var node in config.Network.Nodes.Items)
			{
				if (!config.IsFixedNode(node))
				{
                    nodeForces.Add((node, CalculateNodeForce(node, state, config)));
                }
			}

			// Apply forces
			foreach (var (node, force) in nodeForces)
			{
                Vector speed = state.GetNodeSpeed(node);
                Vector pos = state.GetNodePosition(node);
                double deltaT = deltaTMillis / 1000.0;
                state.SetNodePosition(node, pos + ((speed * deltaT) + (force * deltaT * deltaT / 2)));
                state.SetNodeSpeed(node, speed + ((force / config.NodeMass(node)) * deltaT));
            }
		}

		private Vector CalculateNodeForce(NodeViewModel node, IState state, Configuration config)
		{
			Vector force = new Vector();

			// Calculate total force on node from endpoints
            if (node.Inputs.Count > 0 || node.Outputs.Count > 0)
            {
                force += node.Inputs.Items.Cast<Endpoint>().Concat(node.Outputs.Items)
                    .Select(e => CalculateEndpointForce(e, state, config))
                    .Aggregate((v1, v2) => v1 + v2);
			}

			// Apply node repulsion force so nodes don't overlap
			var nodeCenter = state.GetNodePosition(node) + (new Vector(node.Size.Width, node.Size.Height) / 2.0);
			foreach (var otherNode in config.Network.Nodes.Items)
			{
				if (node == otherNode)
				{
					continue;
				}

				var otherNodeCenter = state.GetNodePosition(otherNode) + (new Vector(otherNode.Size.Width, otherNode.Size.Height) / 2.0);
				var thisToOther = otherNodeCenter - nodeCenter;
				var dist = thisToOther.Length;
				thisToOther.Normalize();

                var repulsionX = thisToOther.X * (-1 * ((node.Size.Width + otherNode.Size.Width) / 2) / dist);
				var repulsionY = thisToOther.Y * (-1 * ((node.Size.Height + otherNode.Size.Height) / 2) / dist);
                force += new Vector(repulsionX, repulsionY) * config.NodeRepulsionForce;
            }

			// Apply friction to make the movement converge to a stable state.
			float gravity = 9.8f;
			float normalForce = gravity * config.NodeMass(node);
            float kineticFriction = normalForce * config.FrictionCoefficient(node);
            Vector frictionVector = new Vector();
			var nodeSpeed = state.GetNodeSpeed(node);
			if (nodeSpeed.Length > 0)
			{
				frictionVector = new Vector(nodeSpeed.X, nodeSpeed.Y);
				frictionVector.Normalize();
				frictionVector *= -1.0 * kineticFriction;
			}
			force += frictionVector;

			return force;
		}

		private Vector CalculateEndpointForce(Endpoint endpoint, IState state, Configuration config)
		{
			var pos = state.GetEndpointPosition(endpoint);

			Vector force = new Vector();

			foreach (var conn in endpoint.Connections.Items)
			{
				var otherSide = conn.Input == endpoint ? (Endpoint)conn.Output : conn.Input;
				var otherSidePos = state.GetEndpointPosition(otherSide);
				var dist = (otherSidePos - pos).Length;
				var angle = Math.Acos((otherSidePos.X - pos.X) / dist);
				if (otherSidePos.Y < pos.Y)
				{
					angle *= -1.0;
				}

				// Put a spring between connected endpoints.
				var hookForce = (dist - config.EquilibriumDistance(conn)) * config.SpringConstant(conn);
				force += new Vector(Math.Cos(angle), Math.Sin(angle)) * hookForce;

				// Try to 'straighten' out the graph horizontally.
				var isLeftSide = endpoint.PortPosition == PortPosition.Left;
				var rowForce = (isLeftSide ? 1 : -1) * config.RowForce(conn);
				force.X += rowForce;
			}

			return force;
		}
	}
}
