using System;
using NodeNetwork.ViewModels;

namespace NodeNetwork.Toolkit
{
    /// <summary>
    /// Used in systems that need an example of a type of node as well as a way to create more instances of the same type.
    /// </summary>
    public class NodeTemplate
    {
        /// <summary>
        /// Factory function to create a new instance of the same type of node as Instance
        /// </summary>
        public Func<NodeViewModel> Factory { get; }

        /// <summary>
        /// Example instance of the type of node created by Factory
        /// </summary>
        public NodeViewModel Instance { get; }

        public NodeTemplate(Func<NodeViewModel> factory)
        {
            Factory = factory;
            Instance = factory();
        }
    }
}