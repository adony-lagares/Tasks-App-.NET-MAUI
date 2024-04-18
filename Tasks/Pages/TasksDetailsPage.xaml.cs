using System.Linq.Expressions;
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
        LoadLocations();
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

        FilesFrame.IsVisible = false;
    }

    private async void LoadLocations()
    {
        var locations = await _attachmentService.Query().Where(a => a.TaskId == Task.Id && string.IsNullOrEmpty(a.File)).ToListAsync();
        if (locations.Count > 0)
        {
            LocationFrame.IsVisible = true;
            LocationCollection.ItemsSource = locations;
            return;
        }

        LocationFrame.IsVisible = false;
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

    private async void LabelLinkGoogleMaps_Tapped(object sender, EventArgs e)
    {
        var label = sender as Label;
        if (label != null)
        {
            var url = label.Text.Split('-')[1].Trim();
            if (!string.IsNullOrWhiteSpace(url))
            {
                await Launcher.OpenAsync(new Uri(url));
            }
        }
    }

    private async void GPSClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Location", $"Confirm the capture of your location?", "Locate", "Cancel");
        if (confirm)
        {
            LocationButton.Text = "Loading...";
            LocationButton.IsEnabled = false;

            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    if (status != PermissionStatus.Granted)
                    {
                        await DisplayAlert("Location Permission", "Location access permission not granted.", "OK");
                        LocationButton.Text = "Locate";
                        LocationButton.IsEnabled = true;
                        return;
                    }
                }

                var request = new GeolocationRequest(GeolocationAccuracy.Medium);
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    await _attachmentService.IncludeAsync(new Attachment
                    {
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        TaskId = Task.Id,
                    });

                    LoadLocations();

                    await DisplayAlert("Location", $"Latitude: {location.Latitude}, Longitude: {location.Longitude}", "Close");
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                await DisplayAlert("Error", "GPS not supported on that device - " + fnsEx.Message, "OK");
            }
            catch (FeatureNotEnabledException fneEx)
            {
                await DisplayAlert("Error", "GPS not enabled. - " + fneEx.Message, "OK");
            }
            catch (PermissionException pEx)
            {
                await DisplayAlert("Error", "GPS permission denied. - " + pEx.Message, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Unable to get location. - " + ex.Message, "OK");
            }
            finally
            {
                LocationButton.Text = "Locate";
                LocationButton.IsEnabled = true;
            }
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