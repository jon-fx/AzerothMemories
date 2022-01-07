namespace AzerothMemories.WebBlazor.Components;

public sealed class AutoCompleteComponent<T> : MudAutocomplete<T>
{
    public AutoCompleteComponent()
    {
        ResetValueOnEmptyText = true;
        SelectValueOnTab = true;
        Margin = Margin.Dense;
        MinCharacters = 2;
        Dense = true;
        //CoerceText = true;
        //CoerceValue = true;
        MaxItems = null;
        InputMode = InputMode.search;
        DebounceInterval = 500;
    }
}