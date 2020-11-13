using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DynamicData;
using NodeNetwork.Utilities;
using ReactiveUI;

namespace NodeNetwork.Toolkit.ContextMenu
{
    public partial class SearchableContextMenuView : IViewFor<SearchableContextMenuViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(SearchableContextMenuViewModel), typeof(SearchableContextMenuView), new PropertyMetadata(null));

        public SearchableContextMenuViewModel ViewModel
        {
            get => (SearchableContextMenuViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (SearchableContextMenuViewModel)value;
        }
        #endregion

        #region ChildrenBelowSearch
        public static readonly DependencyProperty ChildrenBelowSearchProperty =
            DependencyProperty.Register(nameof(ChildrenBelowSearch), typeof(IEnumerable), typeof(SearchableContextMenuView), new PropertyMetadata(new object[0]));

        public IEnumerable ChildrenBelowSearch
        {
            get => (IEnumerable)GetValue(ChildrenBelowSearchProperty);
            set => SetValue(ChildrenBelowSearchProperty, value);
        }
        #endregion

        #region ReferencePointElement
        public static readonly DependencyProperty ReferencePointElementProperty =
            DependencyProperty.Register(nameof(ReferencePointElement), typeof(IInputElement), typeof(SearchableContextMenuView), new PropertyMetadata(null));

        public IInputElement ReferencePointElement
        {
            get => (IInputElement)GetValue(ReferencePointElementProperty);
            set => SetValue(ReferencePointElementProperty, value);
        }
        #endregion

        #region OpenPoint
        public static readonly DependencyProperty OpenPointProperty =
            DependencyProperty.Register(nameof(OpenPoint), typeof(Point), typeof(SearchableContextMenuView), new PropertyMetadata(new Point()));

        public Point OpenPoint
        {
            get => (Point)GetValue(OpenPointProperty);
            private set => SetValue(OpenPointProperty, value);
        }
        #endregion

        public SearchableContextMenuView()
        {
            InitializeComponent();

            this.Bind(ViewModel, vm => vm.SearchQuery, v => v.SearchTextBox.Text);
            this.BindList(ViewModel, vm => vm.VisibleCommands, v => v.CollectionContainer.Collection);

            Binding myBinding = new Binding(nameof(ChildrenBelowSearch)) { Source = this };
            BindingOperations.SetBinding(ContainerBelowSearch, CollectionContainer.CollectionProperty, myBinding);

            this.Opened += (sender, args) =>
            {
                SearchTextBox.Focus();
                if (ReferencePointElement != null)
                {
                    OpenPoint = Mouse.GetPosition(ReferencePointElement);
                }
            };

            // This var is needed to ensure both key down and key up of arrow keys happened in the textbox,
            // otherwise moving into the textbox will immediately move out again.
            bool arrowWasPressedInTextBox = false;

            this.SearchTextBox.PreviewKeyDown += (sender, args) =>
            {
                if (args.Key == Key.Enter || args.Key == Key.Return)
                {
                    if (ViewModel.VisibleCommands.Count > 0)
                    {
                        var firstEntry = ViewModel.VisibleCommands.Items.First();
                        firstEntry.Command.Execute(firstEntry.CommandParameter);
                        this.IsOpen = false;
                    }
                }
                else if (args.Key == Key.Escape && SearchTextBox.Text.Length > 0)
                {
                    SearchTextBox.Text = "";
                    args.Handled = true;
                }
                else if (args.Key == Key.Up || args.Key == Key.Down)
                {
                    arrowWasPressedInTextBox = true;
                }
            };
            this.SearchTextBox.PreviewKeyUp += (sender, args) =>
            {
                if (arrowWasPressedInTextBox && (args.Key == Key.Up || args.Key == Key.Down))
                {
                    arrowWasPressedInTextBox = false;

                    var dir = args.Key == Key.Up ? FocusNavigationDirection.Previous : FocusNavigationDirection.Next;
                    var traversalRequest = new TraversalRequest(dir);
                    var focusedElem = Keyboard.FocusedElement as FrameworkElement;
                    focusedElem?.MoveFocus(traversalRequest);
                }
            };
            this.SearchMenuItem.GotKeyboardFocus += (sender, args) => { SearchTextBox.Focus(); };
        }
    }
}