using System.Windows;
using SnookerGameManagementSystem.Models;
using SnookerGameManagementSystem.Services;
using SnookerGameManagementSystem.ViewModels;

namespace SnookerGameManagementSystem.Views
{
    public partial class EditTableDialog : Window
    {
        public EditTableDialog(Table? table, TableService tableService)
        {
            InitializeComponent();
            DataContext = new EditTableViewModel(table, tableService);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
