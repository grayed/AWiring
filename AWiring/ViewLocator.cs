using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AWiring.ViewModels;

namespace AWiring;

public class ViewLocator : IDataTemplate {
    public IControl Build(object? data) {
        var typeName = data!.GetType().FullName!;
        if (typeName.EndsWith("ViewModel"))
            typeName.Remove(typeName.Length - "ViewModel".Length);
        typeName += "View";
        var type = Type.GetType(typeName);
        if (type != null)
            return (Control)Activator.CreateInstance(type)!;
        return new TextBlock { Text = "Could not find view for " + data.GetType() };
    }

    public bool Match(object? data) {
        return data is ViewModelBase;
    }
}