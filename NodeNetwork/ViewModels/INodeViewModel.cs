using DynamicData;
using Splat;
using System.Windows;

namespace NodeNetwork.ViewModels
{
    public interface INodeViewModel: IHaveId
    {
        bool CanBeRemovedByUser { get; set; }
        IBitmap HeaderIcon { get; set; }
        ISourceList<NodeInputViewModel> Inputs { get; set; }
        bool IsCollapsed { get; set; }
        bool IsSelected { get; set; }
        string Name { get; set; }
        ISourceList<NodeOutputViewModel> Outputs { get; set; }
        NetworkViewModel Parent { get; set; }
        string ParentId { get; }
        Point Position { get; set; }
        Size Size { get; set; }
        IObservableList<NodeInputViewModel> VisibleInputs { get; }
        IObservableList<NodeOutputViewModel> VisibleOutputs { get; }

        void Rebuild(NetworkViewModel parent);
    }
}