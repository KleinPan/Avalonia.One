using Avalonia;

using One.Base.ExtensionMethods;
using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels.StringNodify;

public partial class CalculatorVM : BaseVM
{
    [ObservableProperty]
    private NodifyObservableCollection<OperationVM> _operations = new();

    public OperationsMenuVM OperationsMenu { get; set; }

    [ObservableProperty]
    private NodifyObservableCollection<OperationVM> _selectedOperations = new();

    public NodifyObservableCollection<ConnectionVM> Connections { get; } = new();

    public PendingConnectionVM PendingConnection { get; set; } = new();

    public INodifyCommand CreateConnectionCommand { get; }

    public CalculatorVM()
    {
        CreateConnectionCommand = new DelegateCommand<ConnectorVM>(
               _ => CreateConnection(PendingConnection.Source, PendingConnection.Target),
               _ => CanCreateConnection(PendingConnection.Source, PendingConnection.Target));

        Connections.WhenAdded(c =>
        {
            c.Input.IsConnected = true;
            c.Output.IsConnected = true;

            c.Input.Value = c.Output.Value;

            c.Output.ValueObservers.Add(c.Input);
        })
           .WhenRemoved(c =>
           {
               var ic = Connections.Count(con => con.Input == c.Input || con.Output == c.Input);
               var oc = Connections.Count(con => con.Input == c.Output || con.Output == c.Output);

               if (ic == 0)
               {
                   c.Input.IsConnected = false;
               }

               if (oc == 0)
               {
                   c.Output.IsConnected = false;
               }

               c.Output.ValueObservers.Remove(c.Input);
           });

        Operations.WhenAdded(x =>
        {
            x.Input.WhenRemoved(RemoveConnection);

            if (x is CalculatorInputOperationVM ci)
            {
                ci.Output.WhenRemoved(RemoveConnection);
            }

            void RemoveConnection(ConnectorVM i)
            {
                var c = Connections.Where(con => con.Input == i || con.Output == i).ToArray();
                c.ForEach(con => Connections.Remove(con));
            }
        })
        .WhenRemoved(x =>
        {
            foreach (var input in x.Input)
            {
                DisconnectConnector(input);
            }

            if (x.Output != null)
            {
                DisconnectConnector(x.Output);
            }
        });

        OperationsMenu = new OperationsMenuVM(this);
    }

    internal bool CanCreateConnection(ConnectorVM source, ConnectorVM? target)
           => target == null || (source != target && source.Operation != target.Operation && source.IsInput != target.IsInput);

    internal void CreateConnection(ConnectorVM source, ConnectorVM? target)
    {
        if (target == null)
        {
            PendingConnection.IsVisible = true;
            OperationsMenu.OpenAt(PendingConnection.TargetLocation);
            OperationsMenu.Closed += OnOperationsMenuClosed;
            return;
        }

        var input = source.IsInput ? source : target;
        var output = target.IsInput ? source : target;

        PendingConnection.IsVisible = false;

        DisconnectConnector(input);

        Connections.Add(new ConnectionVM
        {
            Input = input,
            Output = output
        });
    }

    private void OnOperationsMenuClosed()
    {
        PendingConnection.IsVisible = false;
        OperationsMenu.Closed -= OnOperationsMenuClosed;
    }

    private bool CanStartConnection(ConnectorVM c) => !(c.IsConnected && c.IsInput);

    [RelayCommand(CanExecute = nameof(CanStartConnection))]
    private void StartConnection(ConnectorVM connectorViewModel)
    {
        PendingConnection.IsVisible = true;
    }

    [RelayCommand]
    private void DisconnectConnector(ConnectorVM connector)
    {
        var connections = Connections.Where(c => c.Input == connector || c.Output == connector).ToList();
        connections.ForEach(c => Connections.Remove(c));
    }

    [RelayCommand]
    private void DeleteSelection()
    {
        var selected = SelectedOperations.ToList();
        selected.ForEach(o => Operations.Remove(o));
    }

    private bool CanGroupSelectedOperations() => SelectedOperations.Count > 0;

    [RelayCommand(CanExecute = nameof(CanGroupSelectedOperations))]
    private void GroupSelectedOperations()
    {
        var selected = SelectedOperations.ToList();
        var bounding = selected.GetBoundingBox(50);

        Operations.Add(new OperationGroupVM
        {
            Title = "Operations",
            Location = bounding.Position,
            GroupSize = new Size(bounding.Width, bounding.Height)
        });
    }
}