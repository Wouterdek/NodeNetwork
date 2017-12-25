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
    public class Vec3EditorViewModel : ValueEditorViewModel<ShaderFunc>
    {
        static Vec3EditorViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new Vec3EditorView(), typeof(ReactiveUI.IViewFor<Vec3EditorViewModel>));
        }
        
        #region Vec3Value
        private Vec3 _vec3Value;
        public Vec3 Vec3Value
        {
            get => _vec3Value;
            set => this.RaiseAndSetIfChanged(ref _vec3Value, value);
        }
        #endregion

        public Vec3EditorViewModel()
        {
            this.WhenAnyValue(vm => vm.Vec3Value)
                .Select(v => v == null ? null : new ShaderFunc(() => $"vec3(({v.X.ToString(CultureInfo.InvariantCulture)}), ({v.Y.ToString(CultureInfo.InvariantCulture)}), ({v.Z.ToString(CultureInfo.InvariantCulture)}))"))
                .BindTo(this, vm => vm.Value);
        }
    }
}
