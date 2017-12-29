using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeNetwork.ViewModels
{
    /// <summary>
    /// Parent interface for the inputs/outputs of nodes between which connections can be made.
    /// </summary>
    public interface IEndpoint
    {
        /// <summary>
        /// The node that contains this endpoint.
        /// </summary>
        NodeViewModel Parent { get; }
    }
}
