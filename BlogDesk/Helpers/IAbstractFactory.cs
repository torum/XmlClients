using BlogDesk.ViewModels;
using BlogDesk.Views;

namespace BlogDesk.Helpers;

public interface IAbstractFactory<T>
{
    T Create();
}