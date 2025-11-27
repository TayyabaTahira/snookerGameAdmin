using System.Windows;
using SnookerGameManagementSystem.ViewModels;

namespace SnookerGameManagementSystem.Views
{
    public partial class ReportsWindow : Window
    {
        public ReportsWindow(ReportsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
