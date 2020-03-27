﻿using ExampleCodeGenApp.Views.Editors;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels.Editors
{
    public class IntegerValueEditorViewModel : ValueEditorViewModel<int?>
    {
        public IntegerValueEditorViewModel()
        {
            Value = 0;
        }
    }
}
