using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels
{
    [DataContract]
    public enum PortType
    {
        [DataMember] Execution, [DataMember] Integer, [DataMember] String
    }

    [DataContract]
    public class CodeGenPortViewModel : PortViewModel
    {
        static CodeGenPortViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new Views.CodeGenPortView(), typeof(IViewFor<CodeGenPortViewModel>));
        }

        #region PortType
        [DataMember]
        public PortType PortType
        {
            get => _portType;
            set => this.RaiseAndSetIfChanged(ref _portType, value);
        }
        private PortType _portType;
        #endregion
    }
}
