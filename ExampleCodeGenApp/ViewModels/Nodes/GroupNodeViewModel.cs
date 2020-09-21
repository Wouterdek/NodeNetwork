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
using NodeNetwork.Toolkit.Group.AddEndpointDropPanel;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels.Nodes
{
    public class GroupNodeViewModel : CodeGenNodeViewModel
    {
        static GroupNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new GroupNodeView(), typeof(IViewFor<GroupNodeViewModel>));
        }

        public NetworkViewModel Subnet { get; }

        #region IOBinding
        public CodeNodeGroupIOBinding IOBinding
        {
            get => _ioBinding;
            set
            {
                if (_ioBinding != null)
                {
                    throw new InvalidOperationException("IOBinding is already set.");
                }
                _ioBinding = value;
                AddEndpointDropPanelVM = new AddEndpointDropPanelViewModel
                {
                    NodeGroupIOBinding = IOBinding
                };
            }
        }
        private CodeNodeGroupIOBinding _ioBinding;
        #endregion

        public AddEndpointDropPanelViewModel AddEndpointDropPanelVM { get; private set; }

        public GroupNodeViewModel(NetworkViewModel subnet) : base(NodeType.Group)
        {
            this.Name = "Group";
            this.Subnet = subnet;
        }
    }
}
