using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleShaderEditorApp.Model;
using ExampleShaderEditorApp.Views;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;

namespace ExampleShaderEditorApp.ViewModels.Editors
{
    public class FloatEditorViewModel : ValueEditorViewModel<ShaderFunc>
    {
        static FloatEditorViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new FloatEditorView(), typeof(IViewFor<FloatEditorViewModel>));
        }

        #region FloatValue
        private float _floatValue;
        public float FloatValue
        {
            get => _floatValue;
            set => this.RaiseAndSetIfChanged(ref _floatValue, value);
        }
        #endregion

        public FloatEditorViewModel()
        {
            this.WhenAnyValue(vm => vm.FloatValue)
                .Select(v => new ShaderFunc(() => v.ToString(CultureInfo.InvariantCulture)))
                .BindTo(this, vm => vm.Value);
        }
    }
}
