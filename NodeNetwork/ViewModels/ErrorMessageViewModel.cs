using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNetwork.Views;
using ReactiveUI;

namespace NodeNetwork.ViewModels
{
    public class ErrorMessageViewModel : ReactiveObject
    {
        static ErrorMessageViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new ErrorMessageView(), typeof(IViewFor<ErrorMessageViewModel>));
        }

        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public string Message { get; }

        public ErrorMessageViewModel(string message)
        {
            Message = message;
        }
    }
}
