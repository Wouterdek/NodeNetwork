using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetwork.ViewModels
{
    /// <summary>
    /// A viewmodel for a simple error message.
    /// </summary>
    [DataContract]
    public class ErrorMessageViewModel : ReactiveObject
    {
        static ErrorMessageViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new ErrorMessageView(), typeof(IViewFor<ErrorMessageViewModel>));
        }

        #region Logger
        [IgnoreDataMember] private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        /// <summary>
        /// The text to be displayed that explains the error.
        /// </summary>
        [DataMember] public string Message { get; }

        public ErrorMessageViewModel(string message)
        {
            Message = message;
        }
    }
}
