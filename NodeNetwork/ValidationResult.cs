using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeNetwork
{
    /// <summary>
    /// A class that represents a generic validation result.
    /// </summary>
    public abstract class ValidationResult
    {
        /// <summary>
        /// True if the subject is valid
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// A viewmodel of the message that is to be displayed explaining this validation result.
        /// </summary>
        public object MessageViewModel { get; }

        protected ValidationResult(bool isValid, object messageViewModel)
        {
            this.IsValid = isValid;
            this.MessageViewModel = messageViewModel;
        }
    }

    /// <summary>
    /// A validation of the node network.
    /// </summary>
    public class NetworkValidationResult : ValidationResult
    {
        /// <summary>
        /// If false, the network is in a state where trying to parse it (by walking from node to node) can cause problems.
        /// For example, this property is false if the network contains loops since parsing it could then result in infinite loops.
        /// </summary>
        public bool NetworkIsTraversable { get; }

        public NetworkValidationResult(bool isValid, bool isTraversable, object messageViewModel) : base(isValid, messageViewModel)
        {
            NetworkIsTraversable = isTraversable;
        }
    }

    /// <summary>
    /// A validation of a connection between nodes.
    /// </summary>
    public class ConnectionValidationResult : ValidationResult
    {
        public ConnectionValidationResult(bool isValid, object messageViewModel) : base(isValid, messageViewModel)
        {
        }
    }
}
