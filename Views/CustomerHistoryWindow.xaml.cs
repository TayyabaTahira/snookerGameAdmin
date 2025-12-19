using System.Windows;
using SnookerGameManagementSystem.ViewModels;

namespace SnookerGameManagementSystem.Views
{
    public partial class CustomerHistoryWindow : Window
    {
        private readonly CustomerHistoryViewModel _viewModel;

        public CustomerHistoryWindow(CustomerHistoryViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
            
            // Refresh history when window is activated (comes into focus)
            Activated += async (s, e) => await _viewModel.RefreshHistoryAsync();
        }
    }
}
