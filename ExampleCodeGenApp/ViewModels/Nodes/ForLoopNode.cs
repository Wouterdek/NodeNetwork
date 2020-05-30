using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using DynamicData;
using ExampleCodeGenApp.Model;
using ExampleCodeGenApp.Model.Compiler;
using ExampleCodeGenApp.Views;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels.Nodes
{
    [DataContract]
    public class ForLoopNode : CodeGenNodeViewModel
    {
        static ForLoopNode()
        {
            Splat.Locator.CurrentMutable.Register(() => new CodeGenNodeView(), typeof(IViewFor<ForLoopNode>));
        }

        [DataMember] public ValueNodeOutputViewModel<IStatement> FlowIn { get; }

        [DataMember] public ValueListNodeInputViewModel<IStatement> LoopBodyFlow { get; }
        [DataMember] public ValueListNodeInputViewModel<IStatement> LoopEndFlow { get; }

        [DataMember] public ValueNodeInputViewModel<ITypedExpression<int>> FirstIndex { get; }
        [DataMember] public ValueNodeInputViewModel<ITypedExpression<int>> LastIndex { get; }

        [DataMember] public ValueNodeOutputViewModel<ITypedExpression<int>> CurrentIndex { get; }

        public ForLoopNode() : base(NodeType.FlowControl)
        {
            var mainGroup = new EndpointGroup
            {
                Name = "Main Group"
            };

            var nestedGroup1 = new EndpointGroup(mainGroup)
            {
                Name = "Nested Group 1"
            };

            var nestedGroup2 = new EndpointGroup(mainGroup)
            {
                Name = "Nested Group 2"
            };

            this.Name = "For Loop";

            LoopBodyFlow = new CodeGenListInputViewModel<IStatement>(PortType.Execution)
            {
                Name = "Loop Body",
                Group = nestedGroup2
            };
            this.Inputs.Add(LoopBodyFlow);

            LoopEndFlow = new CodeGenListInputViewModel<IStatement>(PortType.Execution)
            {
                Name = "Loop End",
                Group = nestedGroup2
            };
            this.Inputs.Add(LoopEndFlow);

            FirstIndex = new CodeGenInputViewModel<ITypedExpression<int>>(PortType.Integer)
            {
                Name = "First Index",
                Group = nestedGroup1
            };
            this.Inputs.Add(FirstIndex);

            LastIndex = new CodeGenInputViewModel<ITypedExpression<int>>(PortType.Integer)
            {
                Name = "Last Index",
                Group = mainGroup
            };
            this.Inputs.Add(LastIndex);

            ForLoop value = new ForLoop();

            var loopBodyChanged = LoopBodyFlow.Values.Connect().Select(_ => Unit.Default).StartWith(Unit.Default);
            var loopEndChanged = LoopEndFlow.Values.Connect().Select(_ => Unit.Default).StartWith(Unit.Default);
            FlowIn = new CodeGenOutputViewModel<IStatement>(PortType.Execution)
            {
                Name = "",
                Value = Observable.CombineLatest(loopBodyChanged, loopEndChanged, FirstIndex.ValueChanged, LastIndex.ValueChanged,
                        (bodyChange, endChange, firstI, lastI) => (BodyChange: bodyChange, EndChange: endChange, FirstI: firstI, LastI: lastI))
                    .Select(v =>
                    {
                        value.LoopBody = new StatementSequence(LoopBodyFlow.Values.Items);
                        value.LoopEnd = new StatementSequence(LoopEndFlow.Values.Items);
                        value.LowerBound = v.FirstI ?? new IntLiteral { Value = 0 };
                        value.UpperBound = v.LastI ?? new IntLiteral { Value = 1 };
                        return value;
                    })
            };
            this.Outputs.Add(FlowIn);

            CurrentIndex = new CodeGenOutputViewModel<ITypedExpression<int>>(PortType.Integer)
            {
                Name = "Current Index",
                Value = Observable.Return(new VariableReference<int> { LocalVariable = value.CurrentIndex }),
                Group = mainGroup
            };
            this.Outputs.Add(CurrentIndex);
        }
    }
}
