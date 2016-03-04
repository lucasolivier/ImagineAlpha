using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Cuda;

namespace ImagineAlpha
{
    public partial class HistogramForm : Form
    {
        public HistogramForm()
        {
            InitializeComponent();
            histogramBox1.Size = Screen.PrimaryScreen.WorkingArea.Size;
        }

        public void AddHist(Mat hist, string title, Color color)
        {
            histogramBox1.AddHistogram(title, color, hist, 256, new float[] { 0, 255 });
        }

        public void Show(string title)
        {
            label1.Text = title;
            histogramBox1.Refresh();
            base.Show();
        }

        /*public void SetHistogram(IImage image, int numberOfbins, string title)
        {
            
            histogramBox1.GenerateHistograms(image, numberOfbins);
            label1.Text = title;
            histogramBox1.Refresh();
        }*/

        /*public void ShowHist(Mat hist, string title)
        {
            histogramBox1.Size = Screen.PrimaryScreen.WorkingArea.Size;
            histogramBox1.AddHistogram(title, Color.Blue, hist, 256, new float[] { 0, 255 });
            label1.Text = title;
            histogramBox1.Refresh();
        }*/
    }
}
