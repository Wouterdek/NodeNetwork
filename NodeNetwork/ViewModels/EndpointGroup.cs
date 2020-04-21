using System;
using System.Collections.Generic;
using System.Text;

using ReactiveUI;

namespace NodeNetwork.ViewModels
{
    public class EndpointGroup : ReactiveObject
    {
        public static EndpointGroup NoGroup { get; } = new EndpointGroup { Name = "No Group" };

        private string _name;

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
    }
}
