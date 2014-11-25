using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace imageSerial
{
    public partial class Form1 : Form
    {

        #region Variables

        private int length;
        private int tempSize = 0;
        List<Image> myImageList = new List<Image>();
        private int[] bits = { 300, 600, 1200, 1800, 2400, 4800, 7200, 9600, 14400, 19200, 38400, 57600, 115200, 230400, 460800, 921620 };
        private int[] dataBits = { 7, 8 };
        private string[] par = { "Even", "Odd", "None", "Mark", "Space" };
        private string[] stop = { "1", "2" };
        private string[] hshake = { "XOnXOff", "None" };
        private byte[] tempBuffer;
        private string filePath = "E:\\Project_Image";
        List<byte> myByte = new List<byte>();

        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        #region Event Handler for Control in Windows Form

        // Event Handler for loading Form
        private void Form1_Load(object sender, EventArgs e)
        {
            bpsComboBox.DataSource = bits;
            dbComboBox.DataSource = dataBits;
            parityComboBox.DataSource = par;
            sbComboBox.DataSource = stop;
            fcComboBox.DataSource = hshake;
            portComboBox.DataSource = SerialPort.GetPortNames();
            bpsComboBox.SelectedIndex = 7;
            dbComboBox.SelectedIndex = 1;
            parityComboBox.SelectedIndex = 2;
            sbComboBox.SelectedIndex = 0;
            fcComboBox.SelectedIndex = 1;
            brightnessBox.Value = brightnessTrackBar.Value;
            saveButton.Enabled = false;
            pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            refreshGUI(filePath);
        }

        // Event Handler for Start and stop button
        private void startButton_Click(object sender, EventArgs e)
        {
            if (startButton.Text == "Start")
            {
                if (portComboBox.Items.Count > 0)
                {
                    startButton.Enabled = false;
                    saveButton.Enabled = true;
                    portConfig();
                    bpsComboBox.Enabled = false;
                    dbComboBox.Enabled = false;
                    parityComboBox.Enabled = false;
                    sbComboBox.Enabled = false;
                    fcComboBox.Enabled = false;
                    startButton.Text = "Stop";
                }
                else
                {
                    MessageBox.Show("No Serial Port connection", "Error",
                       MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                serialPort1.Close();
                myByte.Clear();
                saveButton.Enabled = false;
                startButton.Enabled = true;
                bpsComboBox.Enabled = true;
                dbComboBox.Enabled = true;
                parityComboBox.Enabled = true;
                sbComboBox.Enabled = true;
                fcComboBox.Enabled = true;
            }

        }

        // Event Handler for Save button
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (checkStorage())
            {
                PictureBox pictureBox2 = new PictureBox();
                pictureBox1.Image.Save(filePath + "\\Picture " + length + 1 + ".jpg");
                storageLabel.Text = (length + 1).ToString() + "/10 Images";
            }
            else
                MessageBox.Show("The storage is FULL", "Error",
                   MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        // Event Handler for Trackbar
        private void brightnessTrackbar_Scroll(object sender, EventArgs e)
        {
            brightnessBox.Value = brightnessTrackBar.Value;
            if (pictureBox1.Image != null)
                brightnessAdjustment();
        }
  
        // Event Handler for DropMenu
        private void optionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            optionMenu optMenu = new optionMenu();
            if (optMenu.ShowDialog() == DialogResult.OK)
            {
                filePath = optMenu.openFolder;
                refreshGUI(filePath);
            }
            else
            {
                filePath = optMenu.defaultFolder;
            }
        }

        // Event Handler for default value of Brightness
        private void resetButton_Click(object sender, EventArgs e)
        {
            brightnessTrackBar.Value = 0;
            brightnessBox.Value = brightnessTrackBar.Value;
            if (pictureBox1.Image != null)
                brightnessAdjustment();
        }

        // Event Handler for received data from serial port
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Boolean check = false;
            int tempPos = 1;
            byte[] buffer = new byte[serialPort1.BytesToRead];
            serialPort1.Read(buffer, 0, serialPort1.BytesToRead);
            Console.WriteLine("Size buffer : " + buffer.Length);


            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == 1)
                {
                    check = true;
                    tempPos = i + 1;
                }
            }

            if (check == true)
            {
                tempBuffer = new byte[buffer.Length - tempPos];
                for (int i = 0; i < tempBuffer.Length; i++)
                {
                    tempBuffer[i] = buffer[tempPos + i];
                }
                Array.Resize(ref buffer, tempPos - 1);
            }

            Console.WriteLine("Size after delete : " + buffer.Length);
            myByte.InsertRange(tempSize, buffer);
            tempSize += buffer.Length;
            Console.WriteLine("Size : " + myByte.Count);
            if (myByte.Count > 32000)
                myByte.RemoveRange(32000, myByte.Count - 32000);
            this.Invoke(new EventHandler(convertImage));
            if (check == true)
            {
                Console.WriteLine("aaaa");
                //myByte.Clear();
                myByte.InsertRange(0, tempBuffer);
                tempSize = tempBuffer.Length;
            }
        }

        // Event Handler for BrightnessBox
        private void brightnessBox_ValueChanged(object sender, EventArgs e)
        {
            brightnessTrackBar.Value = Convert.ToInt16(brightnessBox.Value.ToString());
        }

        // Event Handler for ListBox
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string path = listBox1.GetItemText(listBox1.SelectedItem);
            if (path != "")
            {
                Image im = GetCopyImage(path);
                pictureBox2.Image = im;
            }
        }

        // Event Handler for Delete the selected image from ListBox
        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count > 0)
            {
                string path = listBox1.GetItemText(listBox1.SelectedItem);
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        System.IO.File.Delete(path);
                        storageLabel.Text = (length - 1).ToString() + "/10 Images";
                    }
                    catch (System.IO.IOException ex)
                    {
                        Console.WriteLine(ex.Message);

                    }
                }
            }
        }

        #endregion

        #region Additional Function Support Loading, Saving, Delete and adjust Brightness the images

        // Set up Port
        private void portConfig()
        {
            serialPort1.PortName = portComboBox.SelectedItem.ToString();
            serialPort1.BaudRate = Convert.ToInt32(bpsComboBox.SelectedValue);
            serialPort1.Parity = (Parity)Enum.Parse(typeof(Parity), parityComboBox.SelectedItem.ToString());
            serialPort1.DataBits = Convert.ToInt32(dbComboBox.SelectedValue);
            serialPort1.StopBits = (StopBits)Enum.Parse(typeof(StopBits), sbComboBox.SelectedItem.ToString());
            serialPort1.Handshake = (Handshake)Enum.Parse(typeof(Handshake), fcComboBox.SelectedItem.ToString());
            serialPort1.Open();
        }

        // Check image's storage
        private bool checkStorage()
        {
            Console.Out.WriteLine(length);
            DirectoryInfo di = new DirectoryInfo(filePath);
            length = di.GetFiles("*.jpg", SearchOption.AllDirectories).Length;
            if (length < 10)
                return true;
            else return false;
            /*if (myImageList.Count < 10)
                return true;
            else return false;*/
        }

        // Update the GUI for everytime open the Application and select another save folder
        public void refreshGUI(string _filePath)
        {
            listBox1.Items.Clear();
            DirectoryInfo di = new DirectoryInfo(filePath);
            length = di.GetFiles("*.jpg", SearchOption.AllDirectories).Length;
            storageLabel.Text = length.ToString() + "/10 Images";
            var filters = new[] { ".jpg" };
            List<FileInfo> files = di.GetFiles("*.*", SearchOption.AllDirectories).Where(file => filters.Contains(file.Extension, StringComparer.OrdinalIgnoreCase)).ToList();
            foreach (FileInfo temp in files)
            {
                listBox1.Items.Add(temp.FullName);
            }
        }

        // Convert the image from byte array
        public void convertImage(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(200, 160, PixelFormat.Format8bppIndexed);
            BitmapData bmpData = bmp.LockBits(
            new Rectangle(0, 0, bmp.Width, bmp.Height),
            ImageLockMode.ReadWrite, bmp.PixelFormat);
            Marshal.Copy(myByte.ToArray(), 0, bmpData.Scan0, myByte.Count);
            bmp.UnlockBits(bmpData);
            pictureBox1.Image = bmp;
        }

        // Function to Copy an Image, prevent the other process hold resources when delete.
        private Image GetCopyImage(string path)
        {
            using (Image im = Image.FromFile(path))
            {
                Bitmap bm = new Bitmap(im);
                return bm;
            }
        }

        // Adjust the brightness
        public void brightnessAdjustment()
        {
            float value = brightnessTrackBar.Value * 0.01f;

            float[][] colorMatrixElements = 
            {
                new float[] {1,0,0,0,0},
                new float[] {0,1,0,0,0},
                new float[] {0,0,1,0,0},
                new float[] {0,0,0,1,0},
                new float[] {value,value,value,0,1}
            };

            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            Image _currentImage = pictureBox1.Image;
            Graphics _graphic = default(Graphics);
            Bitmap finalBitmap = new Bitmap(_currentImage.Width, _currentImage.Height);
            _graphic = Graphics.FromImage(finalBitmap);
            _graphic.DrawImage(_currentImage, new Rectangle(0, 0, finalBitmap.Width + 1, finalBitmap.Height + 1), 0, 0, finalBitmap.Width + 1, finalBitmap.Height + 1, GraphicsUnit.Pixel, imageAttributes);
            pictureBox1.Image = finalBitmap;
        }

        #endregion
    }
}
