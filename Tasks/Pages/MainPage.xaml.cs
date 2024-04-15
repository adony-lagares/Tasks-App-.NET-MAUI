using Tasks.Service;
using Tasks.Models;
using Tasks.Constants;
using System.Windows.Input;

namespace Tasks.Pages;

public partial class MainPage : ContentPage
{
    DatabaseService<TaskModelMain> _taskService;

    public ICommand NavigateToDetailCommand { get; private set; }
    public ICommand DeleteCommand { get; private set; }
    public ICommand NavigateToChangeCommand { get; private set; }

    public MainPage()
    {
        InitializeComponent();

        _taskService = new DatabaseService<TaskModelMain>(Db.DB_PATH);

        NavigateToDetailCommand = new Command<TaskModelMain>(async (task) => await NavigateToDetail(task));

        NavigateToChangeCommand = new Command<TaskModelMain>(async (task) => await NavigateToChange(task));

        DeleteCommand = new Command<TaskModelMain>(ExecuteDeleteCommand);

        //TasksCollectionTable.BindingContext = this;


        LoadTasks();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadTasks();
    }

    private async void ExecuteDeleteCommand(TaskModelMain task)
    {
        bool confirm = await DisplayAlert("Confirm", "Do you want to delete this task?", "Yes", "No");
        if (confirm)
        {
            await _taskService.DeleteAsync(task);
            LoadTasks();
        }
    }


    private async Task NavigateToChange(TaskModelMain task)
    {
        await Navigation.PushAsync(new TasksSavePage(task));
    }


    private async Task NavigateToDetail(TaskModelMain task)
    {
        await Navigation.PushAsync(new TasksDetailsPage(task));
    }

    private async void LoadTasks()
    {
        var tasks = await _taskService.AllAsync();
        //TasksCollectionView.ItemsSource = tasks;
        //TasksCollectionTable.ItemsSource = tasks;
    }

    private async void NewClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TasksSavePage());
    }
}

