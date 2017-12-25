using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetwork.ViewModels
{
    public class NodeEndpointEditorViewModel : ReactiveObject
    {
        static NodeEndpointEditorViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeEndpointEditorView(), typeof(IViewFor<NodeEndpointEditorViewModel>));
        }

        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Parent
        private NodeInputViewModel _parent;
        public NodeInputViewModel Parent
        {
            get => _parent;
            internal set => this.RaiseAndSetIfChanged(ref _parent, value);
        }
        #endregion

        public IObservable<Unit> Changed { get; protected set; } = Observable.Empty<Unit>();
    }
}
