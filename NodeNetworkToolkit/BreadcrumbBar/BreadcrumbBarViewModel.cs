using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;

namespace NodeNetwork.Toolkit.BreadcrumbBar
{
    /// <summary>
    /// Viewmodel for a single element of the BreadcrumbBar.
    /// </summary>
    public class BreadcrumbViewModel : ReactiveObject
    {
        #region Name
        /// <summary>
        /// Displayed name of the crumb.
        /// </summary>
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        private string _name = "";
        #endregion
    }

    /// <summary>
    /// ViewModel for the BreadcrumbBar.
    /// This UI element displays a path as a list of path elements (crumbs), allowing navigation by selection of path elements.
    /// </summary>
    public class BreadcrumbBarViewModel : ReactiveObject
    {
        static BreadcrumbBarViewModel()
        {
            NNViewRegistrar.AddRegistration(() => new BreadcrumbBarView(), typeof(IViewFor<BreadcrumbBarViewModel>));
        }

        /// <summary>
        /// The path that is currently displayed in the bar.
        /// Add or remove elements to modify the path.
        /// </summary>
        public ISourceList<BreadcrumbViewModel> ActivePath { get; } = new SourceList<BreadcrumbViewModel>();

        #region ActiveElement
        /// <summary>
        /// The deepest element of the currect path. (Last element of ActivePath)
        /// </summary>
        public BreadcrumbViewModel ActiveItem => _activeItem.Value;
        private readonly ObservableAsPropertyHelper<BreadcrumbViewModel> _activeItem;
        #endregion

        /// <summary>
        /// Navigate to the subpath represented by the selected crumb which is passed as a parameter.
        /// Only this crumb and its ancestors are kept, the rest of the path is removed.
        /// </summary>
        public ReactiveCommand<BreadcrumbViewModel, Unit> SelectCrumb { get; }

        public BreadcrumbBarViewModel()
        {
            SelectCrumb = ReactiveCommand.Create((BreadcrumbViewModel crumb) =>
            {
                ActivePath.Edit(l =>
                {
                    int index = l.IndexOf(crumb);
                    for (int i = l.Count - 1; i > index; i--)
                    {
                        l.RemoveAt(i);
                    }
                });
            });

            ActivePath.Connect().Select(_ => ActivePath.Count > 0 ? ActivePath.Items.ElementAt(ActivePath.Count - 1) : null)
                .ToProperty(this, vm => vm.ActiveItem, out _activeItem);
        }
    }
}