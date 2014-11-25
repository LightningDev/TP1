using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace imageSerial
{
    public partial class optionMenu : Form
    {
        private FolderBrowserDialog openFileDialog1 = new FolderBrowserDialog();
        public string openFolder;
        public string defaultFolder;

        public optionMenu()
        {
            InitializeComponent();
            button1.DialogResult = DialogResult.OK;
            button2.DialogResult = DialogResult.Cancel;
        }

        private void optionMenu_Load(object sender, EventArgs e)
        {
            textBox1.Text = defaultFolder;
            openFolder = defaultFolder;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult saveDialog = openFileDialog1.ShowDialog();
            if (saveDialog == DialogResult.OK)
            {
                openFolder = openFileDialog1.SelectedPath;
                textBox1.Text = openFolder;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = defaultFolder;
        }

    }
}
