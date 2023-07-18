using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Media;
using SyDLab.Usv.Simulator.Applications.Views;
using SyDLab.Usv.Simulator.Domain.Models;

namespace SyDLab.Usv.Simulator.Presentation.Views
{
	[Export(typeof(IGraphView))]
	public partial class GraphView : IGraphView
	{
		public GraphView()
		{
			InitializeComponent();
		}

        private void DataGrid_OnAddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            Output output = e.NewItem as Output;

            int rowIndex = Grid.Items.Count - 1;
            int columeIndex = Grid.Columns.Count - 1;

            DataGridRow row = Grid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            var cell = Grid.Columns[columeIndex].GetCellContent(row).Parent as DataGridCell;
            //set background

            cell.Background = Brushes.AliceBlue;
        }
        private void DataGrid_OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            //            if (e.Column.Header.ToString() == "Color")
            //                e.Column.Header = "MPM (Upper)";
            //            else if (e.Column.Header.ToString() == "MpmLower")
            //                e.Column.Header = "MPM (Lower)";
            //            else if (e.Column.Header.ToString() == "MpmInterval")
            //                e.Column.Header = "MPM (Interval)";
        }
    }
}