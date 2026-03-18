using CommunityToolkit.Mvvm.Messaging;

using One.Toolbox.Helpers;
using One.Toolbox.Messenger;
using One.Toolbox.ViewModels.Base;

using System.Collections.ObjectModel;
using System.Text.Json;

namespace One.Toolbox.ViewModels.Todo;

public partial class TodoPageVM : BasePageVM
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _dataFilePath = Path.Combine(PathHelper.dataPath, "todo-checklist.json");

    public ObservableCollection<TodoDayVM> Days { get; set; } = [];

    [ObservableProperty]
    private TodoDayVM? selectedDay;

    [ObservableProperty]
    private string newTodoTitle = string.Empty;

    [ObservableProperty]
    private string progressText = "0%";

    [ObservableProperty]
    private double progressValue;

    public TodoPageVM()
    {
        WeakReferenceMessenger.Default.Register<CloseMessage>(this, (_, _) => SaveSetting());

        InitData();
    }

    public override void UpdateTitle()
    {
        Title = "Todo";
    }

    private void InitData()
    {
        LoadSetting();
        SelectedDay = EnsureToday();
        UpdateProgress();
    }

    public override void OnNavigatedLeave()
    {
        base.OnNavigatedLeave();
        SaveSetting();
    }

    [RelayCommand]
    private void AddDay()
    {
        var date = Days.Count == 0 ? DateTime.Today : Days.Max(x => x.Date).AddDays(1);
        var day = new TodoDayVM(date);
        Days.Add(day);
        SelectedDay = day;

        SortDays();
        SaveSetting();
    }

    [RelayCommand]
    private void GoToday()
    {
        SelectedDay = EnsureToday();
        UpdateProgress();
    }

    [RelayCommand]
    private void AddTodo()
    {
        if (SelectedDay is null)
        {
            return;
        }

        var title = NewTodoTitle?.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        SelectedDay.Items.Add(new TodoItemVM
        {
            Title = title,
            IsDone = false,
        });

        NewTodoTitle = string.Empty;
        UpdateProgress();
        SaveSetting();
    }

    [RelayCommand]
    private void DeleteTodo(TodoItemVM? item)
    {
        if (item is null || SelectedDay is null)
        {
            return;
        }

        SelectedDay.Items.Remove(item);
        UpdateProgress();
        SaveSetting();
    }

    [RelayCommand]
    private void ToggleTodo(TodoItemVM? _)
    {
        UpdateProgress();
        SaveSetting();
    }

    [RelayCommand]
    private void SyncToToday()
    {
        if (SelectedDay is null)
        {
            return;
        }

        var today = EnsureToday();
        if (today == SelectedDay)
        {
            return;
        }

        var existing = today.Items.Select(x => x.Title).ToHashSet();
        foreach (var item in SelectedDay.Items.Where(x => !x.IsDone))
        {
            if (existing.Add(item.Title))
            {
                today.Items.Add(new TodoItemVM { Title = item.Title, IsDone = false });
            }
        }

        SelectedDay = today;
        UpdateProgress();
        SaveSetting();
    }

    partial void OnSelectedDayChanged(TodoDayVM? value)
    {
        UpdateProgress();
    }

    public void SaveSetting()
    {
        var data = new TodoStorage
        {
            Days = Days.Select(x => new TodoDayStorage
            {
                Date = x.Date,
                Items = x.Items.Select(i => new TodoItemStorage { Title = i.Title, IsDone = i.IsDone }).ToList(),
            }).ToList(),
        };

        File.WriteAllText(_dataFilePath, JsonSerializer.Serialize(data, JsonOptions));
    }

    public void LoadSetting()
    {
        Days.Clear();

        if (!File.Exists(_dataFilePath))
        {
            return;
        }

        var json = File.ReadAllText(_dataFilePath);
        var storage = JsonSerializer.Deserialize<TodoStorage>(json);

        if (storage?.Days is null)
        {
            return;
        }

        foreach (var day in storage.Days)
        {
            Days.Add(new TodoDayVM(day.Date)
            {
                Items = new ObservableCollection<TodoItemVM>(day.Items.Select(x => new TodoItemVM
                {
                    Title = x.Title,
                    IsDone = x.IsDone,
                })),
            });
        }

        SortDays();
    }

    private TodoDayVM EnsureToday()
    {
        var today = Days.FirstOrDefault(x => x.Date == DateTime.Today);
        if (today is not null)
        {
            return today;
        }

        today = new TodoDayVM(DateTime.Today);
        Days.Insert(0, today);
        SortDays();
        return today;
    }

    private void SortDays()
    {
        var ordered = Days.OrderByDescending(x => x.Date).ToList();
        Days.Clear();
        foreach (var day in ordered)
        {
            Days.Add(day);
        }
    }

    private void UpdateProgress()
    {
        if (SelectedDay is null || SelectedDay.Items.Count == 0)
        {
            ProgressValue = 0;
            ProgressText = "0%";
            return;
        }

        var doneCount = SelectedDay.Items.Count(x => x.IsDone);
        var progress = doneCount * 100.0 / SelectedDay.Items.Count;

        ProgressValue = progress;
        ProgressText = $"{Math.Round(progress)}% ({doneCount}/{SelectedDay.Items.Count})";
    }
}

public partial class TodoDayVM : ObservableObject
{
    public TodoDayVM(DateTime date)
    {
        Date = date.Date;
    }

    [ObservableProperty]
    private DateTime date;

    [ObservableProperty]
    private ObservableCollection<TodoItemVM> items = [];

    public string DateLabel => Date.ToString("MM-dd");
    public string WeekLabel => Date.ToString("ddd");
}

public partial class TodoItemVM : ObservableObject
{
    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private bool isDone;
}

public class TodoStorage
{
    public List<TodoDayStorage> Days { get; set; } = [];
}

public class TodoDayStorage
{
    public DateTime Date { get; set; }
    public List<TodoItemStorage> Items { get; set; } = [];
}

public class TodoItemStorage
{
    public string Title { get; set; } = string.Empty;
    public bool IsDone { get; set; }
}
