using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NodeNetwork.ViewModels;

namespace ExampleShaderEditorApp.ViewModels.Nodes
{
    [DataContract]
    public class ShaderNodeViewModel : NodeViewModel
    {
        [DataMember] public NodeCategory Category { get; set; }
    }
}
