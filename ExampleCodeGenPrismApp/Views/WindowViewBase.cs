using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;
using System.Windows;
using ReactiveUI;

namespace ExampleCodeGenPrismApp.Views
{
    public class WindowViewBase<TViewModel> : ReactiveWindow<TViewModel> where TViewModel : class
    {
        protected CompositeDisposable CompositeDisposable { get; }

        public WindowViewBase()
        {
            CompositeDisposable = new CompositeDisposable();

            this.WhenAnyValue(v => v.DataContext)
                .Subscribe(x => ViewModel = x as TViewModel)
                .DisposeWith(CompositeDisposable);

            this.Events().Unloaded
                .Subscribe((_) => CompositeDisposable.Dispose())
                .DisposeWith(CompositeDisposable);
        }
    }
}
