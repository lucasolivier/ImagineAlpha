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
        }

        public void SetHistogram(IImage image, int numberOfbins, string title)
        {
            histogramBox1.Size = Screen.PrimaryScreen.WorkingArea.Size;
            histogramBox1.GenerateHistograms(image, 256);
            label1.Text = title;
            histogramBox1.Refresh();
        }
    }
}
