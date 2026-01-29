using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace One.Base.Collections;


[Serializable]
public class ManualObservableCollection<T> : ObservableCollection<T>
{
    private const string CountString = "Count";

    private const string IndexerName = "Item[]";

    private int _oldCount;

    private bool _canNotify = true;

    public bool CanNotify
    {
        get
        {
            return _canNotify;
        }
        set
        {
            _canNotify = value;
            if (value)
            {
                if (_oldCount != base.Count)
                {
                    OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                }

                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            else
            {
                _oldCount = base.Count;
            }
        }
    }

    public ManualObservableCollection()
    {
    }

    public ManualObservableCollection(List<T> list)
        : base((list != null) ? new List<T>(list.Count) : list)
    {
        CopyFrom(list);
    }

    public ManualObservableCollection(IEnumerable<T> collection)
    {
        if (collection == null)
        {
            throw new ArgumentNullException("collection");
        }

        CopyFrom(collection);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (CanNotify)
        {
            base.OnPropertyChanged(e);
        }
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (CanNotify)
        {
            base.OnCollectionChanged(e);
        }
    }

    private void CopyFrom(IEnumerable<T> collection)
    {
        IList<T> items = base.Items;
        if (collection == null)
        {
            return;
        }

        foreach (T item in collection)
        {
            items.Add(item);
        }
    }
}