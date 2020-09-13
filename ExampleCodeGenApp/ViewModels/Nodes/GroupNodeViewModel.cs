using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using ExampleCodeGenApp.Model;
using ExampleCodeGenApp.Model.Compiler;
using ExampleCodeGenApp.ViewModels.Editors;
using ExampleCodeGenApp.Views;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels.Nodes
{
    public class GroupNodeViewModel : CodeGenNodeViewModel
    {
        static GroupNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new CodeGenNodeView(), typeof(IViewFor<GroupNodeViewModel>));
        }

        public NetworkViewModel Subnet { get; }

        public CodeGroupIOBinding IOBinding { get; set; }

        public GroupNodeViewModel(NetworkViewModel subnet) : base(NodeType.Group)
        {
            this.Name = "Group";
            this.Subnet = subnet;
        }
    }
}
