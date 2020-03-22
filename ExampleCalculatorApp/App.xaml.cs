using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ExampleCalculatorApp.ViewModels;
using ExampleCalculatorApp.ViewModels.Nodes;
using ExampleCalculatorApp.Views;
using NodeNetwork.Utilities;
using NodeNetwork.Views;
using ReactiveUI;
using Splat;

namespace ExampleCalculatorApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            InitSplatContainer();

            base.OnStartup(e);
        }

        private void InitSplatContainer()
        {
            var container = Locator.CurrentMutable;
            
            // Register NodeNetwork components. 
            container.UseNodeNetwork();
            
            // Register custom components. 
            container.Register(() => new MainWindow(), typeof(IViewFor<MainViewModel>));
            container.Register(() => new IntegerValueEditorView(), typeof(IViewFor<IntegerValueEditorViewModel>));
            container.Register(() => new NodeView(), typeof(IViewFor<ConstantNodeViewModel>));
            container.Register(() => new NodeView(), typeof(IViewFor<DivisionNodeViewModel>));
            container.Register(() => new NodeView(), typeof(IViewFor<OutputNodeViewModel>));
            container.Register(() => new NodeView(), typeof(IViewFor<ProductNodeViewModel>));
            container.Register(() => new NodeView(), typeof(IViewFor<SubtractionNodeViewModel>));
            container.Register(() => new NodeView(), typeof(IViewFor<SumNodeViewModel>));
        }
    }
}
