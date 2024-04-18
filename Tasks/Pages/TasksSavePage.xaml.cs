using Tasks.Constants;
using Tasks.Enums;
using Tasks.Models;
using Tasks.Service;

namespace Tasks.Pages;

public partial class TasksSavePage : ContentPage
{
    private DatabaseService<TaskModelMain> _taskService;

    public TaskModelMain SelectedTask { get; set; }

    public TasksSavePage(TaskModelMain task)
    {
        InitializeComponent();

        _taskService = new DatabaseService<TaskModelMain>(Db.DB_PATH);

        var status = task.Status;
        var user = task.User;

        SelectedTask = task;
        BindingContext = task;

        StatusPicker.ItemsSource = Enum.GetValues(typeof(Status)).Cast<Status>().ToList();
        UserPicker.ItemsSource = UsersService.Instance().All();

        StatusPicker.SelectedItem = status;
        UserPicker.SelectedItem = user;

        this.Appearing += OnPageAppearing;
    }

    private async void OnPageAppearing(object sender, EventArgs e)
    {
        await Task.Delay(100);
        TitleEntry.Focus();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(TitleEntry.Text))
        {
            await DisplayAlert("Error", "The Name is required", "Ok");
            TitleEntry.Focus();
            return;
        }

        SelectedTask.Title = TitleEntry.Text;
        SelectedTask.Description = DescriptionEditor.Text;

        if (StatusPicker.SelectedItem != null)
            SelectedTask.Status = (Status)StatusPicker.SelectedItem;
        else
            SelectedTask.Status = Status.Backlog;

        if (UserPicker.SelectedItem != null)
            SelectedTask.UserId = ((User)UserPicker.SelectedItem).Id;

        if (SelectedTask.Id == 0)
            await _taskService.IncludeAsync(SelectedTask);
        else
        {
            SelectedTask.UpdatedDate = DateTime.Now;
            await _taskService.ChangeAsync(SelectedTask);
        }
        await Navigation.PopAsync();
    }

    private async void BackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}