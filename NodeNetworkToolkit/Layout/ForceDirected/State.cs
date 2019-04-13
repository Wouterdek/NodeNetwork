using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NodeNetwork.ViewModels;

namespace NodeNetwork.Toolkit.Layout.ForceDirected
{
	internal interface IState
	{
		Vector GetNodePosition(NodeViewModel node);
		void SetNodePosition(NodeViewModel node, Vector pos);
		Vector GetEndpointPosition(Endpoint endpoint);
		Vector GetNodeSpeed(NodeViewModel node);
		void SetNodeSpeed(NodeViewModel node, Vector speed);
	}

	internal class BufferedState : IState
	{
		private readonly Dictionary<NodeViewModel, Vector> _nodePositions = new Dictionary<NodeViewModel, Vector>();
		private readonly Dictionary<Endpoint, Vector> _endpointRelativePositions = new Dictionary<Endpoint, Vector>();
		public IEnumerable<KeyValuePair<NodeViewModel, Vector>> NodePositions => _nodePositions;

		private readonly Dictionary<NodeViewModel, Vector> _nodeSpeeds = new Dictionary<NodeViewModel, Vector>();

		public Vector GetNodePosition(NodeViewModel node)
		{
			if (!_nodePositions.TryGetValue(node, out Vector result))
			{
				result = new Vector(node.Position.X, node.Position.Y);
			}

			return result;
		}

		public void SetNodePosition(NodeViewModel node, Vector pos)
		{
			_nodePositions[node] = pos;
		}

		public Vector GetEndpointPosition(Endpoint endpoint)
		{
			if (!_endpointRelativePositions.TryGetValue(endpoint, out Vector result))
			{
				result = new Vector(endpoint.Port.CenterPoint.X, endpoint.Port.CenterPoint.Y) - GetNodePosition(endpoint.Parent);
				_endpointRelativePositions[endpoint] = result;
			}

			return result + GetNodePosition(endpoint.Parent);
		}

		public Vector GetNodeSpeed(NodeViewModel node)
		{
			if (!_nodeSpeeds.TryGetValue(node, out Vector result))
			{
				result = new Vector(0, 0);
			}

			return result;
		}

		public void SetNodeSpeed(NodeViewModel node, Vector speed)
		{
			_nodeSpeeds[node] = speed;
		}
	}

	internal class LiveState : IState
	{
		private readonly Dictionary<NodeViewModel, Vector> _nodeSpeeds = new Dictionary<NodeViewModel, Vector>();

		public Vector GetNodePosition(NodeViewModel node)
		{
			return new Vector(node.Position.X, node.Position.Y);
		}

		public void SetNodePosition(NodeViewModel node, Vector pos)
		{
			node.Position = new Point(pos.X, pos.Y);
		}

		public Vector GetEndpointPosition(Endpoint endpoint)
		{
			return new Vector(endpoint.Port.CenterPoint.X, endpoint.Port.CenterPoint.Y);
		}

		public Vector GetNodeSpeed(NodeViewModel node)
		{
			if (!_nodeSpeeds.TryGetValue(node, out Vector result))
			{
				result = new Vector(0, 0);
			}

			return result;
		}

		public void SetNodeSpeed(NodeViewModel node, Vector speed)
		{
			_nodeSpeeds[node] = speed;
		}
	}
}
