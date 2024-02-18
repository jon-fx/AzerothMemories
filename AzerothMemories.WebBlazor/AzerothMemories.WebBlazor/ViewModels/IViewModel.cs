namespace AzerothMemories.WebBlazor.ViewModels;

public interface IViewModel<out TViewModel>
{
    public static abstract TViewModel CreateViewModel(IMoaServices services, Action onViewModelChanged);
}