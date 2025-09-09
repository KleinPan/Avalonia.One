using Avalonia;

using Nodify;

using One.Base.ExtensionMethods;
using One.Toolbox.ViewModels.Base;

using Org.BouncyCastle.Asn1.X509;

using System.Diagnostics;

namespace One.Toolbox.ViewModels.StringNodify;

public partial class CalculatorVM : BaseVM
{
    /// <summary>一个一个算子</summary>
    [ObservableProperty]
    private NodifyObservableCollection<OperationVM> nodes = new();

    public OperationsMenuVM OperationsMenu { get; set; }

    [ObservableProperty]
    private NodifyObservableCollection<OperationVM> _selectedOperations = new();

    public NodifyObservableCollection<ConnectionVM> Connections { get; } = new();

    public PendingConnectionVM PendingConnection { get; set; } = new();

    public INodifyCommand PendingConnectionCompletedCommand { get; }
    public INodifyCommand ConnectionCompletedCommand { get; }

    public CalculatorVM()
    {
        PendingConnectionCompletedCommand = new DelegateCommand<ConnectorVM>(
               _ => PendingConnectionCompleted(PendingConnection.Source, PendingConnection.Target),
               _ => CanPendingConnectionCompleted(PendingConnection.Source, PendingConnection.Target));

        ConnectionCompletedCommand = new DelegateCommand<ConnectorVM>(
               _ => ConnectionCompleted(PendingConnection.Source, PendingConnection.Target));

        Connections.WhenAdded(c =>
        {
            c.Input.IsConnected = true;
            c.Output.IsConnected = true;

            c.Input.Value = c.Output.Value;

            c.Output.ValueObservers.Add(c.Input);
        }).WhenRemoved(c =>
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

        Nodes.WhenAdded(x =>
        {
            x.Input.WhenRemoved(RemoveConnection);
            //目前先不做套娃
            void RemoveConnection(ConnectorVM i)
            {
                var c = Connections.Where(con => con.Input == i || con.Output == i).ToArray();
                c.ForEach(con => Connections.Remove(con));
            }
        }).WhenRemoved(x =>
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

    private void OnOperationsMenuClosed()
    {
        PendingConnection.IsVisible = false;
        OperationsMenu.Closed -= OnOperationsMenuClosed;
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
        selected.ForEach(o => Nodes.Remove(o));
    }

    private bool CanGroupSelectedOperations() => SelectedOperations.Count > 0;

    [RelayCommand(CanExecute = nameof(CanGroupSelectedOperations))]
    private void GroupSelectedOperations()
    {
        var selected = SelectedOperations.ToList();
        var bounding = selected.GetBoundingBox(50);

        Nodes.Add(new OperationGroupVM
        {
            Title = "Operations",
            Location = bounding.Position,
            GroupSize = new Size(bounding.Width, bounding.Height)
        });
    }

    [RelayCommand]
    private void ConnectionStarted(ConnectorVM source)
    {
        Debug.WriteLine("ConnectionStarted");
        Debug.WriteLine(source);
    }

    /// <summary>第二个参数是空的</summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    private void ConnectionCompleted(ConnectorVM source, ConnectorVM? target)
    {
        Debug.WriteLine("ConnectionCompleted");

        Debug.WriteLine(source);
        if (target != null)
        {
            Debug.WriteLine(target);
        }
    }

    private bool CanStartConnection(ConnectorVM c) =>
        !(c.IsConnected && c.IsInput);

    [RelayCommand(CanExecute = nameof(CanStartConnection))]
    private void PendingConnectionStarted(ConnectorVM connectorViewModel)
    {
        Debug.WriteLine("PendingConnectionStarted");
        PendingConnection.IsVisible = true;
    }

    internal bool CanPendingConnectionCompleted(ConnectorVM source, ConnectorVM? target)
         => target == null || (source != target && source.Operation != target.Operation && source.IsInput != target.IsInput);

    internal void PendingConnectionCompleted(ConnectorVM source, ConnectorVM? target)
    {
        Debug.WriteLine("PendingConnectionCompleted");
        Debug.WriteLine(source);
        if (target != null)
        {
            Debug.WriteLine(target);
        }

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

        //断开其他的输入
        DisconnectConnector(input);

        Connections.Add(new ConnectionVM
        {
            Input = input,
            Output = output
        });
    }
}