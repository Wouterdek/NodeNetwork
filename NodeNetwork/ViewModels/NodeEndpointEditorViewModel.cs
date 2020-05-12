using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetwork.ViewModels
{
    /// <summary>
    /// The viewmodel for the editor component that is displayed next to a node endpoint.
    /// </summary>
    [DataContract]
    public class NodeEndpointEditorViewModel : ReactiveObject, IHaveId
    {
        static NodeEndpointEditorViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeEndpointEditorView(), typeof(IViewFor<NodeEndpointEditorViewModel>));
        }

        #region Logger
        [IgnoreDataMember] private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Parent
        /// <summary>
        /// The endpoint that has this object as its editor.
        /// </summary>
        public Endpoint Parent
        {
            get => _parent;
            internal set => this.RaiseAndSetIfChanged(ref _parent, value);
        }
        [IgnoreDataMember] private Endpoint _parent;
        #endregion
        #region Serialisation Properties
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// Gets the parent identifier.
        /// </summary>
        /// <value>
        /// The parent identifier.
        /// </value>
        [DataMember] public string ParentId => Parent?.Id ?? string.Empty;
        #endregion
    }
}
