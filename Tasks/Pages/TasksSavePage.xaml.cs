using Tasks.Constants;
using Tasks.Enums;
using Tasks.Models;
using Tasks.Service;

namespace Tasks.Pages;

public partial class TasksSavePage : ContentPage
{
    private DatabaseService<TaskModelMain> _taskService;

    public TaskModelMain Task { get; set; }

    public TasksSavePage(TaskModelMain task)
    {
        InitializeComponent();

        _taskService = new DatabaseService<TaskModelMain>(Db.DB_PATH);

        var status = task.Status;
        var user = task.User;

        Task = task;
        BindingContext = task;

        StatusPicker.ItemsSource = Enum.GetValues(typeof(Status)).Cast<Status>().ToList();
        UserPicker.ItemsSource = UsersService.Instance().All();

        StatusPicker.SelectedItem = status;
        UserPicker.SelectedItem = user;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(TitleEntry.Text))
        {
            await DisplayAlert("Error", "The Name is required", "Ok");
            TitleEntry.Focus();
            return;
        }

        Task.Title = TitleEntry.Text;
        Task.Description = DescriptionEditor.Text;

        if (StatusPicker.SelectedItem != null)
            Task.Status = (Status)StatusPicker.SelectedItem;
        else
            Task.Status = Status.Backlog;

        if (UserPicker.SelectedItem != null)
            Task.UserId = ((User)UserPicker.SelectedItem).Id;

        if (Task.Id == 0)
            await _taskService.IncludeAsync(Task);
        else
        {
            Task.UpdatedDate = DateTime.Now;
            await _taskService.ChangeAsync(Task);
        }
        await Navigation.PopAsync();
    }

    private async void BackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}