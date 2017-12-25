using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeNetwork.ViewModels
{
    public interface IEndpoint
    {
        NodeViewModel Parent { get; }
    }
}
