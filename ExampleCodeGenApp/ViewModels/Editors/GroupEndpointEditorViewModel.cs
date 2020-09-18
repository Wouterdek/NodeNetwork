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

        public GroupEndpointEditorViewModel(CodeGroupIOBinding groupBinding)
        {
            MoveUp = ReactiveCommand.Create(() =>
            {
                bool isInput = Parent is NodeInputViewModel;
                IEnumerable<Endpoint> endpoints = isInput ? (IEnumerable<Endpoint>)Parent.Parent.Inputs.Items : Parent.Parent.Outputs.Items;

                var prevElement = endpoints
                    .Where(e => e.SortIndex < Parent.SortIndex)
                    .MaxBy(e => e.SortIndex)
                    .FirstOrDefault();
                if (prevElement != null)
                {
                    var idx = prevElement.SortIndex;
                    prevElement.SortIndex = Parent.SortIndex;
                    Parent.SortIndex = idx;
                }
            });
            MoveDown = ReactiveCommand.Create(() =>
            {
                bool isInput = Parent is NodeInputViewModel;
                IEnumerable<Endpoint> endpoints = isInput ? (IEnumerable<Endpoint>)Parent.Parent.Inputs.Items : Parent.Parent.Outputs.Items;

                var nextElement = endpoints
                    .Where(e => e.SortIndex > Parent.SortIndex)
                    .MinBy(e => e.SortIndex)
                    .FirstOrDefault();
                if (nextElement != null)
                {
                    var idx = nextElement.SortIndex;
                    nextElement.SortIndex = Parent.SortIndex;
                    Parent.SortIndex = idx;
                }
            });
            Delete = ReactiveCommand.Create(() =>
            {
                groupBinding.DeleteEndpoint(Parent);
            });
        }
    }
}
