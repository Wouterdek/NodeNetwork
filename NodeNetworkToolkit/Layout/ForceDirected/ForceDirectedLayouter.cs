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
	public class ForceDirectedLayouter
	{
		public void Layout(Configuration config, int maxIterations)
		{
			var engine = new Engine();
			var state = new BufferedState();

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

		public async Task LayoutAsync(Configuration config, CancellationToken token)
		{
			var engine = new Engine();
			var state = new LiveState();

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

				await Dispatcher.Yield(DispatcherPriority.Input);
			} while (!token.IsCancellationRequested);
		}
	}
}
