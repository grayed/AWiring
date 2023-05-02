using Avalonia.Controls;
using AWiring.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWiring.Views;
internal class ViewModelView<ViewModelT, BaseT>
    where ViewModelT : ViewModelBase
    {
    private object? DataContext { get; set; }
    ViewModelT? ViewModel { get => (ViewModelT?)DataContext; set => DataContext = value; }
}
