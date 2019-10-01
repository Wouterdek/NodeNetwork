using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using NodeNetwork.ViewModels;

namespace NodeNetwork.Toolkit.Layout.ForceDirected
{
	/// <summary>
	/// Reposition the nodes in a network using a physics-based approach.
	/// The nodes are interpreted as point masses, and the connections are represented
	/// by springs. This system, along with a few additional forces such as friction and a
	/// horizontal force, is then simulated to calculate the new position of the nodes.
	/// </summary>
	public class ForceDirectedLayouter
	{
		/// <summary>
		/// Layout the nodes in the network.
		/// </summary>
		/// <param name="config">The configuration to use.</param>
		/// <param name="maxIterations">The maximum amount of iterations after which the physics simulation ends.</param>
		public void Layout(Configuration config, int maxIterations)
		{
			var engine = new Engine();
			var state = new BufferedState();

            // Move each node so no two nodes have the exact same position.
            engine.ApplyRandomShift(config.Network);

            int deltaT = (int)Math.Ceiling(10.0 / (double)config.UpdatesPerIteration);
			for (int i = 0; i < maxIterations * config.UpdatesPerIteration; i++)
			{
				engine.Update(deltaT, state, config);
			}

			foreach (var newNodePosition in state.NodePositions)
			{
				newNodePosition.Key.Position = new Point(newNodePosition.Value.X, newNodePosition.Value.Y);
			}
		}

		/// <summary>
		/// Layout the nodes in the network, updating the user interface at each iteration.
		/// This method, contrary to Layout(), lets users see the simulation as it happens.
		/// The cancellation token should be used to end the simulation.
		/// </summary>
		/// <param name="config">The configuration to use.</param>
		/// <param name="token">A cancellation token to end the layout process.</param>
		/// <returns>The async task</returns>
		public async Task LayoutAsync(Configuration config, CancellationToken token)
		{
			var engine = new Engine();
			var state = new LiveState();

            // Move each node so no two nodes have the exact same position.
            engine.ApplyRandomShift(config.Network);

            DateTime start = DateTime.Now;
			TimeSpan t = TimeSpan.Zero;
			do
			{
				// Current real time
				var newT = DateTime.Now - start;
				var deltaT = newT - t;
				
				// Current modified time
				//int virtT = (int)(t.Milliseconds * Settings.TimeModifier);
				int virtDeltaT = (int)(deltaT.Milliseconds * config.TimeModifier);
				int virtDeltaTPerUpdate = virtDeltaT / config.UpdatesPerIteration;
				for (int i = 0; i < config.UpdatesPerIteration; i++)
				{
					// Modified time in this update step
					engine.Update(virtDeltaTPerUpdate, state, config);
				}

				t = newT;

				await Task.Delay(14, token);
			} while (!token.IsCancellationRequested);
		}
	}
}
