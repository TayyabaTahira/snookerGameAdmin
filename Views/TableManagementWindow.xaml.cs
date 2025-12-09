using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using SnookerGameManagementSystem.Services;
using SnookerGameManagementSystem.ViewModels;

namespace SnookerGameManagementSystem.Views
{
    public partial class TableManagementWindow : Window
    {
        private TableInfoViewModel? _draggedItem;
        private System.Windows.Point _startPoint;
        private bool _isDragging = false;
        private System.Windows.Controls.Border? _lastHighlightedBorder;

        public TableManagementWindow(TableService tableService)
        {
            InitializeComponent();
            DataContext = new TableManagementViewModel(tableService);
        }

        private void TableCard_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Don't start drag if clicking on a button
            if (e.OriginalSource is System.Windows.Controls.Button)
                return;

            // Check if click is within a button
            var element = e.OriginalSource as DependencyObject;
            while (element != null && element != sender)
            {
                if (element is System.Windows.Controls.Button)
                    return;
                element = VisualTreeHelper.GetParent(element);
            }

            if (sender is System.Windows.Controls.Border border && 
                border.Tag is TableInfoViewModel tableInfo)
            {
                _draggedItem = tableInfo;
                _startPoint = e.GetPosition(null);
                _isDragging = false;
            }
        }

        private void TableCard_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _draggedItem != null && !_isDragging)
            {
                System.Windows.Point mousePos = e.GetPosition(null);
                Vector diff = _startPoint - mousePos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    _isDragging = true;
                    if (sender is System.Windows.Controls.Border border)
                    {
                        // Set cursor to indicate dragging
                        Mouse.OverrideCursor = Cursors.Hand;
                        
                        // Start drag operation
                        DragDropEffects result = DragDrop.DoDragDrop(border, _draggedItem, DragDropEffects.Move);
                        
                        // Reset cursor
                        Mouse.OverrideCursor = null;
                        
                        // Reset any highlighted borders
                        if (_lastHighlightedBorder != null)
                        {
                            _lastHighlightedBorder.Opacity = 1.0;
                            _lastHighlightedBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3a3a4e"));
                            _lastHighlightedBorder = null;
                        }
                    }
                    _isDragging = false;
                    _draggedItem = null;
                }
            }
        }

        private void TableCard_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TableInfoViewModel)))
            {
                e.Effects = DragDropEffects.Move;
                
                // Provide visual feedback - highlight the drop target
                if (sender is System.Windows.Controls.Border border)
                {
                    var draggedItem = e.Data.GetData(typeof(TableInfoViewModel)) as TableInfoViewModel;
                    var targetItem = border.Tag as TableInfoViewModel;
                    
                    // Only show effects if items are different
                    if (draggedItem != null && targetItem != null && draggedItem != targetItem)
                    {
                        // Reset previous highlight
                        if (_lastHighlightedBorder != null && _lastHighlightedBorder != border)
                        {
                            _lastHighlightedBorder.Opacity = 1.0;
                            _lastHighlightedBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3a3a4e"));
                        }
                        
                        // Highlight current drop target
                        border.Opacity = 0.7;
                        border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4caf50"));
                        _lastHighlightedBorder = border;
                    }
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void TableCard_DragLeave(object sender, DragEventArgs e)
        {
            // Reset opacity and border when drag leaves
            if (sender is System.Windows.Controls.Border border)
            {
                // Check if we're actually leaving the border (not just entering a child element)
                var position = e.GetPosition(border);
                var borderBounds = new Rect(0, 0, border.ActualWidth, border.ActualHeight);
                
                if (!borderBounds.Contains(position))
                {
                    border.Opacity = 1.0;
                    border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3a3a4e"));
                    
                    if (_lastHighlightedBorder == border)
                    {
                        _lastHighlightedBorder = null;
                    }
                }
            }
            e.Handled = true;
        }

        private async void TableCard_Drop(object sender, DragEventArgs e)
        {
            // Reset opacity and border
            if (sender is System.Windows.Controls.Border border)
            {
                border.Opacity = 1.0;
                border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3a3a4e"));
            }
            
            _lastHighlightedBorder = null;

            if (e.Data.GetDataPresent(typeof(TableInfoViewModel)) && 
                sender is System.Windows.Controls.Border dropBorder &&
                dropBorder.Tag is TableInfoViewModel targetItem)
            {
                var draggedItem = e.Data.GetData(typeof(TableInfoViewModel)) as TableInfoViewModel;
                
                if (draggedItem != null && draggedItem != targetItem)
                {
                    var viewModel = DataContext as TableManagementViewModel;
                    if (viewModel != null)
                    {
                        await viewModel.ReorderTablesAsync(draggedItem, targetItem);
                    }
                }
            }
            
            e.Handled = true;
        }
    }
}
