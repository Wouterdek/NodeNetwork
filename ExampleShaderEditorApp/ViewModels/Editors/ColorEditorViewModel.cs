using System.Globalization;
using System.Reactive.Linq;
using System.Windows.Media;
using ExampleShaderEditorApp.Model;
using ExampleShaderEditorApp.Views;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;

namespace ExampleShaderEditorApp.ViewModels.Editors
{
    public class ColorEditorViewModel : ValueEditorViewModel<ShaderFunc>
    {
        static ColorEditorViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new ColorEditorView(), typeof(IViewFor<ColorEditorViewModel>));
        }
        
        #region ColorValue
        private Color _colorValue;
        public Color ColorValue
        {
            get => _colorValue;
            set => this.RaiseAndSetIfChanged(ref _colorValue, value);
        }
        #endregion

        public ColorEditorViewModel()
        {
            this.WhenAnyValue(vm => vm.ColorValue)
                .Select(c => new ShaderFunc(() => $"vec3({(c.R / 255d).ToString(CultureInfo.InvariantCulture)}, {(c.G / 255d).ToString(CultureInfo.InvariantCulture)}, {(c.B / 255d).ToString(CultureInfo.InvariantCulture)})"))
                .BindTo(this, vm => vm.Value);
        }
    }
}
