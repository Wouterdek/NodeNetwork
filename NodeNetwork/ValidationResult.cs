using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeNetwork
{
    public abstract class ValidationResult
    {
        public bool IsValid { get; }
        public object MessageViewModel { get; }

        protected ValidationResult(bool isValid, object messageViewModel)
        {
            this.IsValid = isValid;
            this.MessageViewModel = messageViewModel;
        }
    }

    public class NetworkValidationResult : ValidationResult
    {
        public bool NetworkIsTraversable { get; }

        public NetworkValidationResult(bool isValid, bool isTraversable, object messageViewModel) : base(isValid, messageViewModel)
        {
            NetworkIsTraversable = isTraversable;
        }
    }

    public class ConnectionValidationResult : ValidationResult
    {
        public ConnectionValidationResult(bool isValid, object messageViewModel) : base(isValid, messageViewModel)
        {
        }
    }
}
