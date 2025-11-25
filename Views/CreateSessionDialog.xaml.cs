using System.Windows;
using SnookerGameManagementSystem.ViewModels;

namespace SnookerGameManagementSystem.Views
{
    public partial class CreateSessionDialog : Window
    {
        public CreateSessionDialog(CreateSessionViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void CreateTable_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
