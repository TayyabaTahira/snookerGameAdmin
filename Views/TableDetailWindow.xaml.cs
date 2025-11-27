using System.Windows;
using System.Windows.Threading;
using SnookerGameManagementSystem.ViewModels;

namespace SnookerGameManagementSystem.Views
{
    public partial class TableDetailWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private readonly TableDetailViewModel _viewModel;

        public TableDetailWindow(TableDetailViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;

            // Setup timer for real-time updates
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            // Trigger property change to update elapsed time display
            _viewModel.OnPropertyChanged(nameof(_viewModel.ElapsedTimeDisplay));
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer.Stop();
            base.OnClosed(e);
        }
    }
}
