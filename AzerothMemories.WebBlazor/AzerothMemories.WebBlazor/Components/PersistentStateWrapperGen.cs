namespace AzerothMemories.WebBlazor.Components;

public sealed class PersistentStateWrapperGen<TState> : PersistentStateWrapper
{
    private readonly string _key;
    private readonly PersistentStateViewModel _viewModel;

    private readonly Func<TState> _getFunc;
    private readonly Action<TState> _setAction;
    private readonly Func<Task<TState>> _createStateFunction;

    public PersistentStateWrapperGen(PersistentStateViewModel viewModel, string key, Func<TState> getFunc, Action<TState> setAction, Func<Task<TState>> createStateFunction)
    {
        _key = key;
        _viewModel = viewModel;
        _getFunc = getFunc;
        _setAction = setAction;
        _createStateFunction = createStateFunction;
    }

    public override string Key => _key;

    public override void OnPersistComponentState()
    {
        var fieldSate = _getFunc();

        _viewModel.Services.ClientServices.PersistentComponentState.PersistAsJson(Key, fieldSate);
    }

    public override async Task TryLoadPersistentState()
    {
        if (!_viewModel.Services.ClientServices.PersistentComponentState.TryTakeFromJson(Key, out TState state))
        {
            state = await _createStateFunction();
        }

        _setAction(state);
    }
}