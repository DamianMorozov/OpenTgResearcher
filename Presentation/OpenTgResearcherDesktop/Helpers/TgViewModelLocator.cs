// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Helpers;

/// <summary> ViewModel locator </summary>
public sealed class TgViewModelLocator
{
    private readonly ConcurrentDictionary<Type, object> _viewModelsByType = new();
    private readonly ConcurrentDictionary<string, object> _viewModelsByName = new();

    public TgViewModelLocator()
    {
        var assembly = typeof(TgViewModelLocator).Assembly;

        var viewModelTypes = assembly.GetTypes()
            .Where(t => t.Name.EndsWith("ViewModel") && t.IsClass && !t.IsAbstract);

        foreach (var type in viewModelTypes)
        {
            try
            {
                var instance = App.GetService(type);
                _viewModelsByType[type] = instance;
                _viewModelsByName[type.Name] = instance;
            }
            catch (Exception ex)
            {
#if DEBUG
                TgLogUtils.WriteException(ex);
#endif
            }
        }
    }

    /// <summary> Get ViewModel by type </summary>
    public T Get<T>() where T : class =>
        _viewModelsByType.TryGetValue(typeof(T), out var vm)
            ? vm as T ?? throw new InvalidCastException($"Cannot cast {vm?.GetType()} to {typeof(T)}")
            : throw new KeyNotFoundException($"ViewModel of type {typeof(T)} not found.");

    /// <summary> Get ViewModel by type's name </summary>
    public object Get(string name) =>
        _viewModelsByName.TryGetValue(name, out var vm)
            ? vm
            : throw new KeyNotFoundException($"ViewModel named '{name}' not found.");
}
