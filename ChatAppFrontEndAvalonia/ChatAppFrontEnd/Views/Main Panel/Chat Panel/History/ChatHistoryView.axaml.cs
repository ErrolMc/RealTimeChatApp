using System;
using System.Collections.Specialized;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ChatAppFrontEnd.ViewModels;
using ReactiveUI;

namespace ChatAppFrontEnd.Views;

public partial class ChatHistoryView : UserControl
{
    public ChatHistoryView()
    {
        InitializeComponent();
        DataContextChanged += ChatHistoryView_DataContextChanged;
        
        ScrollToBottom();
    }

    private void ChatHistoryView_DataContextChanged(object sender, EventArgs e)
    {
        if (DataContext is ChatHistoryViewModel vm)
        {
            // Detach the old event handler if the DataContext changes
            if (vm.Messages is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= Messages_CollectionChanged;
            }

            // Attach the new event handler
            vm.Messages.CollectionChanged += Messages_CollectionChanged;
        }
    }

    private void Messages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    { 
        ScrollToBottom();
    }

    private async void ScrollToBottom()
    {
        await Task.Delay(10);

        try
        {
            Dispatcher.UIThread.Post(() =>
            {
                var scrollViewer = this.FindControl<ScrollViewer>("Scroller");
                scrollViewer?.ScrollToEnd();
            });
        }
        catch (Exception e)
        {
            
        }

    }
}