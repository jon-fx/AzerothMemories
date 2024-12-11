using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components.Web;

namespace AzerothMemories.WebBlazor.Components;

public abstract class PersistentStateViewModel : ViewModelBase
{
    private readonly List<PersistentStateWrapper> _persistentStateWrappers = [];

    private PersistingComponentStateSubscription _componentStateSubscription;

    protected PersistentStateViewModel(IMoaServices services, Action onViewModelChanged) : base(services, onViewModelChanged)
    {
    }

    public override async Task OnInitialized()
    {
        await base.OnInitialized();

        _componentStateSubscription = Services.ClientServices.PersistentComponentState.RegisterOnPersisting(PersistComponentState, RenderMode.InteractiveWebAssembly);

        await TryLoadPersistentState();
    }

    private Task PersistComponentState()
    {
        foreach (var test in _persistentStateWrappers)
        {
            test.OnPersistComponentState();
        }

        return Task.CompletedTask;
    }

    protected void AddPersistentState<TState>(Func<TState?> getFunc, Action<TState?> setAction, Func<Task<TState?>> createStateFunction, [CallerArgumentExpression("getFunc")] string? message = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new NotImplementedException();
        }

        _persistentStateWrappers.Add(new PersistentStateWrapperGen<TState>(this, message, getFunc, setAction, createStateFunction));
    }

    private async Task TryLoadPersistentState()
    {
        foreach (var test in _persistentStateWrappers)
        {
            await test.TryLoadPersistentState();
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        _componentStateSubscription.Dispose();
    }
}