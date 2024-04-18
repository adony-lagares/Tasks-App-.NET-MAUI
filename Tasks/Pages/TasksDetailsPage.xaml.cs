using Tasks.Constants;
using Tasks.Models;
using Tasks.Service;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace Tasks.Pages;

public partial class TasksDetailsPage : ContentPage
{
    public TaskModelMain Task { get; set; }

    private DatabaseService<TaskModelMain> _taskService;
    private DatabaseService<Comment> _commentService;
    private DatabaseService<Attachment> _attachmentService;

    public TasksDetailsPage(TaskModelMain task)
    {
        InitializeComponent();
        Task = task;
        BindingContext = this;
        _taskService = new DatabaseService<TaskModelMain>(Db.DB_PATH);
        _commentService = new DatabaseService<Comment>(Db.DB_PATH);
        _attachmentService = new DatabaseService<Attachment>(Db.DB_PATH);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LabelTitle.Text = Task.Title;
        LabelUsername.Text = Task.Username;
        LabelCreatedDate.Text = Task.CreatedDate.ToString();
        LabelUpdatedDate.Text = Task.UpdatedDate.ToString();
        LabelStatus.Text = Task.Status.ToString();
        LabelDescription.Text = Task.Description;
        UserPicker.ItemsSource = UsersService.Instance().All();

        LoadComments();
        LoadFiles();
    }

    private async void LoadFiles()
    {
        var files = await _attachmentService.Query().Where(a => a.TaskId == Task.Id && !string.IsNullOrEmpty(a.File)).ToListAsync();
        if (files.Count > 0)
        {
            FilesFrame.IsVisible = true;
            FilesCollection.ItemsSource = files;
            return;
        }

        FilesCollection.IsVisible = false;
    }

    private async void LoadComments()
    {
        CommentsCollection.ItemsSource = await _commentService.Query().Where(c => c.TaskId == Task.Id).ToListAsync();
    }

    private async void BackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void ChangeClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TasksSavePage(Task));
    }

    private async void TakePictureClicked(object sender, EventArgs e)
    {
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

                if (photo != null)
                {
                    using Stream stream = await photo.OpenReadAsync();

                    string directory = FileSystem.AppDataDirectory;
                    string filename = Path.Combine(directory, $"{DateTime.Now.ToString("ddMMyyyy_hhmmss")}.jpg");

                    using FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
                    await stream.CopyToAsync(fileStream);

                    await _attachmentService.IncludeAsync(new Attachment
                    {
                        File = filename,
                        TaskId = Task.Id,
                    });

                    LoadFiles();
                }
            }
            else
            {
                await DisplayAlert("Error", "Photo capture is not supported on this device.", "OK");
            }
        }
        catch (FeatureNotSupportedException fnsEx)
        {
            await DisplayAlert("Error", "Photo capture is not supported on this device. - " + fnsEx.Message, "OK");
        }
        catch (PermissionException pEx)
        {
            await DisplayAlert("Error", "Permission to access the camera not granted. - " + pEx.Message, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error has occurred: {ex.Message}", "OK");
        }
    }

    private async void DeleteClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Confirm", "Do you want to delete this task?", "Yes", "No");
        if (confirm)
        {
            await _taskService.DeleteAsync(Task);
            await Navigation.PopAsync();
        }
    }

    private async void AddCommentClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(NewCommentEditor.Text) || UserPicker.SelectedIndex == -1)
        {
            await DisplayAlert("Error", "Type a comment and select a user", "Close");
            return;
        }

        var user = (User)UserPicker.SelectedItem;
        await _commentService.IncludeAsync(new Comment
        {
            UserId = user.Id,
            TaskId = Task.Id,
            Text = NewCommentEditor.Text
        });

        NewCommentEditor.Text = string.Empty;
        UserPicker.SelectedItem = -1;

        LoadComments();
    }
}