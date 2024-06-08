using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ChatAppFrontEnd.ViewModels;

namespace ChatAppFrontEnd.Views
{
    public partial class ChatView : UserControl
    {
        public ChatView()
        {
            InitializeComponent();
        }

        private void TextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var viewModel = DataContext as ChatViewModel;
                viewModel?.SendMessage();
            }
        }
    }   
}