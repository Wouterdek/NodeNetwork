using System;
using System.Runtime.Serialization;
using ReactiveUI;

namespace NodeNetwork.ViewModels
{
    [DataContract]
    public class EndpointGroup : ReactiveObject, IHaveId
    {
        #region Serialisation Properties
        [DataMember]
        public string Id { get; set; }

        [DataMember] public string ParentId => Parent?.Id ?? string.Empty;
        #endregion

        #region Parent
        [DataMember]
        public EndpointGroup Parent
        {
            get => _parent;
            set => this.RaiseAndSetIfChanged(ref _parent, value);
        }
        [IgnoreDataMember] private EndpointGroup _parent;
        #endregion

        #region Name
        [DataMember]
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        [IgnoreDataMember] private string _name;
        #endregion

        public EndpointGroup(EndpointGroup parent)
        {
            Id = Guid.NewGuid().ToString();
            Parent = parent;
        }

        public EndpointGroup() : this(null) { }
    }
}