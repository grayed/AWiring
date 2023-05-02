using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using AWiring.ViewModels;

namespace AWiring.Views;
public class ElementView : TemplatedControl, IViewModelView<Element> {
    public ElementView() {
        (this as IViewModelView<Element>).ViewModel = new();
    }
}
