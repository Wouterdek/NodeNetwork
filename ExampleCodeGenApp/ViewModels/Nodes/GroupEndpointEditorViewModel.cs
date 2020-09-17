using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using DynamicData;
using ExampleCodeGenApp.Views;
using MoreLinq.Extensions;
using NodeNetwork.Toolkit.Group;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels.Nodes
{
    public interface IGroupEndpointEditorViewModel
    {
        public Endpoint Endpoint { get; }
        public ReactiveCommand<Unit, Unit> MoveUp { get; }
        public ReactiveCommand<Unit, Unit> MoveDown { get; }
        public ReactiveCommand<Unit, Unit> Delete { get; }
    }

    public class GroupEndpointEditorViewModel<T> : ValueEditorViewModel<T>, IGroupEndpointEditorViewModel
    {
        public Endpoint Endpoint => Parent;
        public ReactiveCommand<Unit, Unit> MoveUp { get; }
        public ReactiveCommand<Unit, Unit> MoveDown { get; }
        public ReactiveCommand<Unit, Unit> Delete { get; }

        static GroupEndpointEditorViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new GroupEndpointEditorView(), typeof(IViewFor<GroupEndpointEditorViewModel<T>>));
        }

        public GroupEndpointEditorViewModel(GroupIOBinding groupBinding)
        {
            MoveUp = ReactiveCommand.Create(() =>
            {
                bool isOnGroupNode = groupBinding.GroupNode == this.Parent.Parent;
                bool isInput = Parent is NodeInputViewModel;
                
                IEnumerable<Endpoint> endpoints;
                Endpoint toMove;

                if (isInput)
                {
                    if (isOnGroupNode)
                    {
                        endpoints = groupBinding.GroupNode.Inputs.Items;
                        toMove = Parent;
                    }
                    else
                    {
                        endpoints = groupBinding.GroupNode.Outputs.Items;
                        toMove = groupBinding.GetGroupNodeOutput((NodeInputViewModel)Parent);
                    }
                }
                else
                {
                    if (isOnGroupNode)
                    {
                        endpoints = groupBinding.GroupNode.Outputs.Items;
                        toMove = Parent;
                    }
                    else
                    {
                        endpoints = groupBinding.GroupNode.Inputs.Items;
                        toMove = groupBinding.GetGroupNodeInput((NodeOutputViewModel)Parent);
                    }
                }

                var prevElement = endpoints
                    .Where(e => e.SortIndex < toMove.SortIndex)
                    .MaxBy(e => e.SortIndex)
                    .FirstOrDefault();
                if (prevElement != null)
                {
                    var idx = prevElement.SortIndex;
                    prevElement.SortIndex = toMove.SortIndex;
                    toMove.SortIndex = idx;
                }
            });
            MoveDown = ReactiveCommand.Create(() =>
            {
                bool isOnGroupNode = groupBinding.GroupNode == this.Parent.Parent;
                bool isInput = Parent is NodeInputViewModel;

                IEnumerable<Endpoint> endpoints;
                Endpoint toMove;

                if (isInput)
                {
                    if (isOnGroupNode)
                    {
                        endpoints = groupBinding.GroupNode.Inputs.Items;
                        toMove = Parent;
                    }
                    else
                    {
                        endpoints = groupBinding.GroupNode.Outputs.Items;
                        toMove = groupBinding.GetGroupNodeOutput((NodeInputViewModel)Parent);
                    }
                }
                else
                {
                    if (isOnGroupNode)
                    {
                        endpoints = groupBinding.GroupNode.Outputs.Items;
                        toMove = Parent;
                    }
                    else
                    {
                        endpoints = groupBinding.GroupNode.Inputs.Items;
                        toMove = groupBinding.GetGroupNodeInput((NodeOutputViewModel)Parent);
                    }
                }

                var nextElement = endpoints
                    .Where(e => e.SortIndex > toMove.SortIndex)
                    .MinBy(e => e.SortIndex)
                    .FirstOrDefault();
                if (nextElement != null)
                {
                    var idx = nextElement.SortIndex;
                    nextElement.SortIndex = toMove.SortIndex;
                    toMove.SortIndex = idx;
                }
            });
            Delete = ReactiveCommand.Create(() =>
            {
                bool isOnGroupNode = groupBinding.GroupNode == this.Parent.Parent;
                bool isInput = Parent is NodeInputViewModel;

                if (isInput)
                {
                    if (isOnGroupNode)
                    {
                        Parent.Parent.Inputs.Remove((NodeInputViewModel)Parent);
                    }
                    else
                    {
                        var output = groupBinding.GetGroupNodeOutput((NodeInputViewModel) Parent);
                        groupBinding.GroupNode.Outputs.Remove(output);
                    }
                }
                else
                {
                    if (isOnGroupNode)
                    {
                        Parent.Parent.Outputs.Remove((NodeOutputViewModel)Parent);
                    }
                    else
                    {
                        var input = groupBinding.GetGroupNodeInput((NodeOutputViewModel)Parent);
                        groupBinding.GroupNode.Inputs.Remove(input);
                    }
                }
            });
        }
    }
}
