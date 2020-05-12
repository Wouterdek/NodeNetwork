﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ExampleShaderEditorApp.Views;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;

namespace ExampleShaderEditorApp.ViewModels
{
    [DataContract]
    public class EnumEditorViewModel : ValueEditorViewModel<object>
    {
        static EnumEditorViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new EnumEditorView(), typeof(IViewFor<EnumEditorViewModel>));
        }

        [IgnoreDataMember] public object[] Options { get; }
        [IgnoreDataMember] public string[] OptionLabels { get; }

        #region SelectedOptionIndex
        [IgnoreDataMember] private int _selectedOptionIndex;
        [DataMember]
        public int SelectedOptionIndex
        {
            get => _selectedOptionIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedOptionIndex, value);
        }
        #endregion
        
       
        public EnumEditorViewModel(Type enumType)
        {
            if (enumType == null)
            {
                return;
            }

            if (!enumType.IsEnum)
            {
                throw new ArgumentException(enumType.Name + " is not an enum type");
            }
            Options = Enum.GetValues(enumType).Cast<object>().ToArray();
            OptionLabels = Options.Select(c => Enum.GetName(enumType, c)).ToArray();

            this.WhenAnyValue(vm => vm.SelectedOptionIndex)
                .Select(i => i == -1 ? null : Options[i])
                .BindTo(this, vm => vm.Value);
        }
    }
}
