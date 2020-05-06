using System;
using System.Collections.Generic;
using System.Text;

using ReactiveUI;

namespace NodeNetwork.ViewModels
{
    public class EndpointGroup : ReactiveObject
    {
        private string _name;

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public EndpointGroup Parent { get; }

        public EndpointGroup(EndpointGroup parent)
        {
            Parent = parent;
        }

        public EndpointGroup() : this(null) { }
    }
}
