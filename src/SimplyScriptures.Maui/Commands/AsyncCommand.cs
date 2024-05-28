using System.Windows.Input;

namespace SimplyScriptures.Commands;
public class AsyncCommand(Func<Task> action) : ICommand
{
    public event EventHandler? CanExecuteChanged;    

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("AsyncUsage", "AsyncFixer03:Fire-and-forget async-void methods or delegates", Justification = "<Pending>")]
    public async void Execute(object? parameter)
    {
        try
        {
            await action();
        }        
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
