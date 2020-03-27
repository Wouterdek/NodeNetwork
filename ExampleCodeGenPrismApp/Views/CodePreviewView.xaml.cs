﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExampleCodeGenApp.ViewModels;
using ReactiveUI;

namespace ExampleCodeGenApp.Views
{
    public partial class CodePreviewView : IViewFor<CodePreviewViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(CodePreviewViewModel), typeof(CodePreviewView), new PropertyMetadata(null));

        public CodePreviewViewModel ViewModel
        {
            get => (CodePreviewViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (CodePreviewViewModel)value;
        }
        #endregion

        public CodePreviewView()
        {
            InitializeComponent();

            this.WhenActivated(d => { 
                this.OneWayBind(ViewModel, vm => vm.CompiledCode, v => v.codeTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CompilerError, v => v.errorTextBlock.Text).DisposeWith(d);
            });
        }
    }
}
