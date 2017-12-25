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
    public class Vec2EditorViewModel : ValueEditorViewModel<ShaderFunc>
    {
        static Vec2EditorViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new Vec2EditorView(), typeof(ReactiveUI.IViewFor<Vec2EditorViewModel>));
        }

        #region Vec2Value
        private Vec2 _vec2Value;
        public Vec2 Vec2Value
        {
            get => _vec2Value;
            set => this.RaiseAndSetIfChanged(ref _vec2Value, value);
        }
        #endregion

        public Vec2EditorViewModel()
        {
            this.WhenAnyValue(vm => vm.Vec2Value)
                .Select(v => v == null ? null : new ShaderFunc(() => $"vec2(({v.X.ToString(CultureInfo.InvariantCulture)}), ({v.Y.ToString(CultureInfo.InvariantCulture)}))"))
                .BindTo(this, vm => vm.Value);
        }
    }
}
