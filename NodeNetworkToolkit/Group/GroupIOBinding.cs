using NodeNetwork.ViewModels;

namespace NodeNetwork.Toolkit.Group
{
    public abstract class GroupIOBinding
    {
        public NodeViewModel GroupNode { get; }
        public NodeViewModel EntranceNode { get; }
        public NodeViewModel ExitNode { get; }

        public GroupIOBinding(NodeViewModel groupNode, NodeViewModel entranceNode, NodeViewModel exitNode)
        {
            GroupNode = groupNode;
            EntranceNode = entranceNode;
            ExitNode = exitNode;
        }

        public abstract NodeInputViewModel GetEntranceInput(NodeOutputViewModel entranceOutput);
        public abstract NodeOutputViewModel GetEntranceOutput(NodeInputViewModel entranceInput);

        public abstract NodeInputViewModel GetExitInput(NodeOutputViewModel exitOutput);
        public abstract NodeOutputViewModel GetExitOutput(NodeInputViewModel exitInput);

        public abstract NodeInputViewModel AddNewEntranceInput(NodeOutputViewModel candidateOutput);
        public abstract NodeOutputViewModel AddNewEntranceOutput(NodeInputViewModel candidateInput);
        public abstract NodeOutputViewModel AddNewExitOutput(NodeInputViewModel candidateInput);
        public abstract NodeInputViewModel AddNewExitInput(NodeOutputViewModel candidateOutput);
    }
}