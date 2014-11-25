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
using TeamProject;

namespace imageSerial
{
    public partial class Form1 : Form
    {

        #region Variables
        private int PB_MOVE = 1;
        private int END_BYTE = 1;
        private int MAX_WEIGHT = 200;
        private int MAX_HEIGHT = 160;
        private int length;
        private int tempSize = 0;
        private Image currentImage;
        List<Image> myImageList = new List<Image>();
        List<byte> myByte = new List<byte>();
        private int[] bits = { 300, 600, 1200, 1800, 2400, 4800, 7200, 9600, 14400, 19200, 38400, 57600, 115200, 230400, 460800, 921620 };
        private int[] dataBits = { 7, 8 };
        private string[] par = { "Even", "Odd", "None", "Mark", "Space" };
        private string[] stop = { "1", "2" };
        private string[] hshake = { "XOnXOff", "None" };
        private string filePath;
        private byte[] tempBuffer;
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
            progressBar1.Enabled = false;
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
                    //startButton.Enabled = false;
                    saveButton.Enabled = true;
                    portConfig();
                    bpsComboBox.Enabled = false;
                    dbComboBox.Enabled = false;
                    parityComboBox.Enabled = false;
                    sbComboBox.Enabled = false;
                    fcComboBox.Enabled = false;
                    portComboBox.Enabled = false;
                    progressBar1.Enabled = true;
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
                tempSize = 0;
                bpsComboBox.Enabled = true;
                dbComboBox.Enabled = true;
                parityComboBox.Enabled = true;
                sbComboBox.Enabled = true;
                fcComboBox.Enabled = true;
                portComboBox.Enabled = true;
                progressBar1.Enabled = false;
                pictureBox1.Image = null;
                progressBar1.Value = 0;
                startButton.Text = "Start";
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
                listBox1.Items.Add(filePath + "\\Picture " + length + 1 + ".jpg");
            }
            else if(checkStorage() == false)
                MessageBox.Show("The storage is FULL or Image folder not set up yet", "Error",
                   MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        // Event Handler for Trackbar
        private void brightnessTrackbar_Scroll(object sender, EventArgs e)
        {
            brightnessBox.Value = brightnessTrackBar.Value;
            if (currentImage != null)
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
            // Variables
            Boolean check = false;
            int tempPos = 1;
            byte[] buffer = new byte[serialPort1.BytesToRead];

            // Read byte into buffer
            serialPort1.Read(buffer, 0, buffer.Length);

            // Check the end of picture, END_BYTE = 1
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == END_BYTE)
                {
                    check = true;
                    tempPos = i;
                    break;
                }
            }

            // If this buffer reach the end byte, cut the remainder after byte 1
            if (check == true)
            {
                tempBuffer = new byte[buffer.Length - tempPos - 1];
                for (int i = 1; i <=tempBuffer.Length; i++)
                {
                    tempBuffer[i-1] = buffer[tempPos + i];
                }
                Array.Resize(ref buffer, tempPos);
            }

            // Insert the buffer into List of bytes
            myByte.InsertRange(tempSize, buffer);
            tempSize += buffer.Length;
            Console.WriteLine("Size : " + tempSize);            

            // Check if List of bytes reach MAX, means 1 image then convert it into Image
            if (tempSize == (MAX_WEIGHT*MAX_HEIGHT))
            {
                this.Invoke(new EventHandler(convertImage));
                currentImage = pictureBox1.Image;
            }

            PB_MOVE = buffer.Length;

            // After received 1 image, clear the Array and put the remainder into it
            if (check == true)
            {
                myByte.Clear();
                myByte.InsertRange(0, tempBuffer);
                tempSize = tempBuffer.Length;
                PB_MOVE += tempBuffer.Length;
            }

            this.Invoke(new EventHandler(updatePB));                       
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
            if (listBox1.Items.Count > 0 && listBox1.SelectedIndex >=0)
            {
                string path = listBox1.GetItemText(listBox1.SelectedItem);
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        System.IO.File.Delete(path);
                        storageLabel.Text = listBox1.Items.Count + "/10 Images";
                    }
                    catch (System.IO.IOException ex)
                    {
                        Console.WriteLine(ex.Message);

                    }
                }
            }           
        }

        // Event Handler for load Raw Image
        private void loadRAWToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] anotherByte = new byte[1];
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "RAW FILE SELECTION";
            theDialog.Filter = "RAW files|*.raw";
            theDialog.InitialDirectory = filePath;
            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                anotherByte = File.ReadAllBytes(theDialog.FileName);
            }

            convertImage1(anotherByte);
        }

        // Event Handler for Exit
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
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
            if (filePath != null)
            {
                DirectoryInfo di = new DirectoryInfo(filePath);
                length = di.GetFiles("*.jpg", SearchOption.AllDirectories).Length;
                if (length < 10)
                    return true;
                else return false;
                /*if (myImageList.Count < 10)
                    return true;
                else return false;*/
            }
            else return false;
        }

        // Update the GUI for everytime open the Application and select another save folder
        public void refreshGUI(string _filePath)
        {
            listBox1.Items.Clear();
            if (filePath != null)
            {
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
        }

        // Convert the image from byte array, this is Event Handler
        public void convertImage(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(MAX_WEIGHT, MAX_HEIGHT, PixelFormat.Format8bppIndexed);

            // Change the Palette of 8 bit color to 8 bit grayscale
            ColorPalette _palette = bmp.Palette;
            Color[] _entries = _palette.Entries;
            for (int i = 0; i < 256; i++)
            {
                Color b = new Color();
                b = Color.FromArgb((byte)i, (byte)i, (byte)i);
                _entries[i] = b;
            }
            bmp.Palette = _palette;

            // Lock bits of Bitmap
            BitmapData bmpData = bmp.LockBits(
            new Rectangle(0, 0, bmp.Width, bmp.Height),
            ImageLockMode.ReadWrite, bmp.PixelFormat);

            // Copy the raw byte array to Bitmap
            Marshal.Copy(myByte.ToArray(), 0, bmpData.Scan0, myByte.Count);

            // Unlock bits of bitmap
            bmp.UnlockBits(bmpData);

            // Return image to PictureBox
            pictureBox1.Image = bmp;
        }

        // Convert the image from byte array, this is function to call
        public void convertImage1(byte[] myByte1)
        {
            Bitmap bmp = new Bitmap(320, 240, PixelFormat.Format8bppIndexed);
            ColorPalette _palette = bmp.Palette;
            Color[] _entries = _palette.Entries;
            for (int i = 0; i < 256; i++)
            {
                Color b = new Color();
                b = Color.FromArgb((byte)i, (byte)i, (byte)i);
                _entries[i] = b;
            }
            bmp.Palette = _palette;
            BitmapData bmpData = bmp.LockBits(
            new Rectangle(0, 0, bmp.Width, bmp.Height),
            ImageLockMode.ReadWrite, bmp.PixelFormat);
            Marshal.Copy(myByte1, 0, bmpData.Scan0, myByte1.Length);
            bmp.UnlockBits(bmpData);
            pictureBox3.Image = bmp;
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
            Image _currentImage = currentImage;
            Graphics _graphic = default(Graphics);
            Bitmap finalBitmap = new Bitmap(_currentImage.Width, _currentImage.Height);
            _graphic = Graphics.FromImage(finalBitmap);
            _graphic.DrawImage(_currentImage, new Rectangle(0, 0, finalBitmap.Width + 1, finalBitmap.Height + 1), 
                0, 0, finalBitmap.Width + 1, finalBitmap.Height + 1, GraphicsUnit.Pixel, imageAttributes);
            pictureBox1.Image = finalBitmap;
        }

        // Update progressbar
        public void updatePB(object sender, EventArgs e)
        {
            if (tempSize == 32000) progressBar1.Value = 0;
            if (progressBar1.Value < progressBar1.Maximum)
                progressBar1.Increment(PB_MOVE);
            else progressBar1.Value = progressBar1.Minimum;
        }

        // Multiply Matrix
        public Bitmap Convolution3x3(Bitmap b, ConvolutionMatrix m)
        {
            Bitmap newImg = (Bitmap)b.Clone();
            Color[,] pixelColor = new Color[3, 3];
            int A, R, G, B;

            for (int y = 0; y < b.Height - 2; y++)
            {
                for (int x = 0; x < b.Width - 2; x++)
                {
                    pixelColor[0, 0] = b.GetPixel(x, y);
                    pixelColor[0, 1] = b.GetPixel(x, y + 1);
                    pixelColor[0, 2] = b.GetPixel(x, y + 2);
                    pixelColor[1, 0] = b.GetPixel(x + 1, y);
                    pixelColor[1, 1] = b.GetPixel(x + 1, y + 1);
                    pixelColor[1, 2] = b.GetPixel(x + 1, y + 2);
                    pixelColor[2, 0] = b.GetPixel(x + 2, y);
                    pixelColor[2, 1] = b.GetPixel(x + 2, y + 1);
                    pixelColor[2, 2] = b.GetPixel(x + 2, y + 2);

                    A = pixelColor[1, 1].A;

                    R = (int)((((pixelColor[0, 0].R * m.Matrix[0, 0]) + (pixelColor[1, 0].R * m.Matrix[1, 0]) + (pixelColor[2, 0].R * m.Matrix[2, 0]) +
                                 (pixelColor[0, 1].R * m.Matrix[0, 1]) + (pixelColor[1, 1].R * m.Matrix[1, 1]) + (pixelColor[2, 1].R * m.Matrix[2, 1]) +
                                 (pixelColor[0, 2].R * m.Matrix[0, 2]) + (pixelColor[1, 2].R * m.Matrix[1, 2]) + (pixelColor[2, 2].R * m.Matrix[2, 2]))
                                        / m.Factor) + m.Offset);

                    if (R < 0)
                    {
                        R = 0;
                    }
                    else if (R > 255)
                    {
                        R = 255;
                    }

                    G = (int)((((pixelColor[0, 0].G * m.Matrix[0, 0]) + (pixelColor[1, 0].G * m.Matrix[1, 0]) + (pixelColor[2, 0].G * m.Matrix[2, 0]) +
                                 (pixelColor[0, 1].G * m.Matrix[0, 1]) + (pixelColor[1, 1].G * m.Matrix[1, 1]) + (pixelColor[2, 1].G * m.Matrix[2, 1]) +
                                 (pixelColor[0, 2].G * m.Matrix[0, 2]) + (pixelColor[1, 2].G * m.Matrix[1, 2]) + (pixelColor[2, 2].G * m.Matrix[2, 2]))
                                        / m.Factor) + m.Offset);

                    if (G < 0)
                    {
                        G = 0;
                    }
                    else if (G > 255)
                    {
                        G = 255;
                    }

                    B = (int)((((pixelColor[0, 0].B * m.Matrix[0, 0]) +
                                 (pixelColor[1, 0].B * m.Matrix[1, 0]) + (pixelColor[2, 0].B * m.Matrix[2, 0]) + (pixelColor[0, 1].B * m.Matrix[0, 1]) +
                                 (pixelColor[1, 1].B * m.Matrix[1, 1]) + (pixelColor[2, 1].B * m.Matrix[2, 1]) + (pixelColor[0, 2].B * m.Matrix[0, 2]) +
                                 (pixelColor[1, 2].B * m.Matrix[1, 2]) + (pixelColor[2, 2].B * m.Matrix[2, 2]))  / m.Factor) + m.Offset);

                    if (B < 0)
                    {
                        B = 0;
                    }
                    else if (B > 255)
                    {
                        B = 255;
                    }
                    newImg.SetPixel(x + 1, y + 1, Color.FromArgb(A, R, G, B));
                }
            }
            return newImg;
        }

        // Apply Blur Image
        public void ApplySmooth(double weight)
        {
            ConvolutionMatrix matrix = new ConvolutionMatrix(30);
            matrix.defaultValue(1);
            matrix.Matrix[1, 1] = weight;
            matrix.Factor = weight + 8;
            pictureBox3.Image = Convolution3x3((Bitmap)pictureBox3.Image, matrix);
        }


        #endregion

       
    }
}
