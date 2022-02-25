namespace AzerothMemories.WebBlazor.Components;

public abstract class PersistentStateWrapper
{
    public abstract string Key { get; }

    public abstract void OnPersistComponentState();

    public abstract Task TryLoadPersistentState();
}