using Tasks.Service;
using Tasks.Models;
using Tasks.Constants;
using System.Windows.Input;

namespace Tasks.Pages;

public partial class MainPage : ContentPage
{
    private DatabaseService<TaskModelMain> _taskService;

    public ICommand NavigateToDetailCommand { get; private set; }

    public MainPage()
    {
        InitializeComponent();

        _taskService = new DatabaseService<TaskModelMain>(Db.DB_PATH);

        NavigateToDetailCommand = new Command<TaskModelMain>(async (task) => await NavigateToDetail(task));

        LoadTasks();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadTasks();
    }

    private async Task NavigateToDetail(TaskModelMain task)
    {
        await Navigation.PushAsync(new TasksDetailsPage(task));
    }

    private async void LoadTasks()
    {
        CardBacklog.ItemsSource = await _taskService.Query().Where(t => t.Status == Enums.Status.Backlog).ToListAsync();
        CardReview.ItemsSource = await _taskService.Query().Where(t => t.Status == Enums.Status.Review).ToListAsync();
        CardToDo.ItemsSource = await _taskService.Query().Where(t => t.Status == Enums.Status.ToDo).ToListAsync();
        CardDevelopment.ItemsSource = await _taskService.Query().Where(t => t.Status == Enums.Status.Development).ToListAsync();
        CardDone.ItemsSource = await _taskService.Query().Where(t => t.Status == Enums.Status.Done).ToListAsync();
    }

    private async void NewClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (sender != null)
        {
            var status = (Enums.Status)button.CommandParameter;
            await Navigation.PushAsync(new TasksSavePage(new TaskModelMain { Status = status }));
        }
    }
}