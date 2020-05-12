using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExampleShaderEditorApp.ViewModels.Nodes
{
    [DataContract]
    public enum NodeCategory
    {
        [DataMember] Vector, [DataMember] Math, [DataMember] Misc
    }
}
