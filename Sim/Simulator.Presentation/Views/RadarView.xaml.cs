using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MathNet.Spatial.Units;
using SyDLab.Usv.Simulator.Applications.Views;
using System.Windows.Forms.Integration;
using SyDLab.Usv.Simulator.Applications.ViewModels;

namespace SyDLab.Usv.Simulator.Presentation.Views
{
    /// <summary>
    /// RadarView.xaml에 대한 상호 작용 논리
    /// </summary>
    [Export(typeof(IRadarView))]
    /// 
    public partial class RadarView : IRadarView
    {
        private readonly BackgroundWorker _playWorker;

        public RadarView()
        {
            InitializeComponent();
        }

        // private void load(object sender, EventArgs e)
        // {
        //     //create Bitmap
        //     bmp = new Bitmap(width + 1, height + 1);
        //
        //     //background color
        //     //.BackColor = Color.Black;
        //
        //     //center
        //     cx = width / 2;
        //     cy = height / 2;
        //
        //     //initial degree of HAND
        //     u = 0;
        //
        //     //timer
        //     t.Interval = 5; //in millisecond
        //     t.Tick += t_Tick;
        //     t.Start();
        // }

        public void UpdateImage(Bitmap image)
        {
            Action action = () => (Radar.Child as PictureBox).Image = image;

            //load bitmap in picturebox

            if ((Radar.Child as PictureBox).InvokeRequired)
            {
                (Radar.Child as PictureBox).Invoke(action);
            }
            else
            {
                action();
            }
        }
    }
}
