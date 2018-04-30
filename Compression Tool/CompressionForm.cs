//DNA Compression tool
//Copyright(C) 2018 DNAtix Ltd.

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.




using System;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Compression_Tool
{
    public partial class CompressionForm : Form
    {

        #region constants

        // DNAtix compressed format extension
        private static readonly string DNATIX_EXTENSION = ".dtix";
        
        // About text
        private static readonly string ABOUT =
            "DNAtix - DNA Compression Tool - v0.1b\n\n" +
            "(c)2018 DNAtix Ltd. - All rights reserved. - www.dnatix.com\n\n" +
            "This tool is a free tool developed by DNAtix for compressing DNA sequence files.\n\n" +
            "Developed by the DNAtix Development Team for the DNAtix Genetics Eco-system.\n\n" +
            "Contact us at: support@dnatix.com";

        #endregion







        #region properties

        private string inputFilePath = "";      // fastra file path
        private string outputFilePath = "";     // compressed file path

        #endregion

        // Code for windows resizing
        ResizeClass resizeClass;


        /// <summary>
        /// Form Constructor
        /// </summary>
        public CompressionForm()
        {
            InitializeComponent();

            // Resizing
            resizeClass = new ResizeClass(this);

            // Make background images resize in unity with their controlers
            compressButton.BackgroundImageLayout = ImageLayout.Stretch;
            browseButton.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage; 
        }







        #region Handlers



        /// <summary>
        /// Open a dialog for the user to choose a file to compress
        /// </summary>
        private void browseButton_Click(object sender, EventArgs e)
        {
            // Create a Dialog
            OpenFileDialog browseFileDialog = new OpenFileDialog();
 

            // Check if the user choose a file
            if (browseFileDialog.ShowDialog() == DialogResult.OK)
            {
                // update input file path and update the file information in the GUI
                inputFilePath = browseFileDialog.FileName;
                filePathTextBox.Text = inputFilePath;
                filePathTextBox.SelectionStart = filePathTextBox.Text.Length;
            }

        }




        /// <summary>
        /// 
        /// </summary>
        private void compressButton_Click(object sender, EventArgs e)
        {
            // Check if compression path is empty
            if (inputFilePath.Equals(""))
            {
                MessageBox.Show(
                    "Please select a file to compress first",
                    "No file selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }


            if (inputFilePath.EndsWith(DNATIX_EXTENSION))
            {
                DialogResult res = MessageBox.Show(
                    "You are trying to Compress a file in a compressed format (" + DNATIX_EXTENSION + ") " + 
                    "Are you sure you want to compress?",
                    "Confirmation",
                    MessageBoxButtons.YesNoCancel);

                if (res != DialogResult.Yes)
                    return;
            }



            // Create a dialog for the user to choose the output (compressed) file
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "DNAtix Compression Format | *" + DNATIX_EXTENSION;

            // Show dialog
            DialogResult result = saveFileDialog1.ShowDialog();




            // Check if user choose a valid path
            if (result == DialogResult.OK)
            {
                // set outputFilePath with the path the user choose
                outputFilePath = saveFileDialog1.FileName;
                // Compress the input file
                CompressFile();
            }
        }





        /// <summary>
        /// Show About Message
        /// </summary>
        private void aboutButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(ABOUT, "About");
        }


        #endregion




        /// <summary>
        /// This function handles compression
        /// </summary>
        private async void CompressFile()
        {
            // Disable compression buttons and start progress bar
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.MarqueeAnimationSpeed = 30;
            compressButton.Enabled = false;
            decompressButton.Enabled = false;



            // Compress file
            int ret = await Task.Run(() => Compress.CompressFile(inputFilePath, outputFilePath));



            // Enable compression buttons and stop porgress bar
            compressButton.Enabled = true;
            decompressButton.Enabled = true;
            progressBar1.Style = ProgressBarStyle.Blocks;

            

            // Show a MessageBox with information about the status of the compression
            showCompressionResult(ret, true);
        }





        /// <summary>
        ///  Show a MessageBox according to the result of the compression
        /// </summary>
        /// <param name="ret">Compression result</param>
        private void showCompressionResult(int ret, bool compress)
        {
            switch (ret)
            {
                // Success 
                case 0:
                    MessageBox.Show(
                    String.Format("Succsesfuly {0} file", compress ? "compress" : "decompress"),
                    String.Format("Succsesfuly {0} file", compress ? "compress" : "decompress"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                    break;



                // File contains invalid characters (not A, C, T or G)
                case -1:
                    MessageBox.Show(String.Format(
                    "{0} contains invalid characters", inputFilePath),
                    String.Format("Failed to {0} file", compress ? "compress" : "decompress"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                    break;



                // IO exception caused by the input or output files
                case -2:
                    MessageBox.Show(
                    String.Format("Failed to open input or output file", inputFilePath),
                    "Failed to compress file",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                    break;
            }

        }




       
        
        /// <summary>
        /// Resize the window
        /// </summary>
        private void ResizeFormWindow(object sender, EventArgs e)
        {
            // Resize components
            resizeClass.Resize();   
        }


        
        /// <summary>
        /// 
        /// </summary>
        private void decompressButton_Click(object sender, EventArgs e)
        {
            // Check if decompression path is empty
            if (inputFilePath.Equals(""))
            {
                MessageBox.Show(
                    "Please select a file to decompress first",
                    "No file selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }


            if (!inputFilePath.EndsWith(DNATIX_EXTENSION))
            {
                DialogResult res = MessageBox.Show(
                    "You are trying to Decompress a file in a different format then " + DNATIX_EXTENSION + "\n" +
                    "Are you sure you want to decompress?",
                    "Confirmation",
                    MessageBoxButtons.YesNoCancel);

                if (res != DialogResult.Yes)
                    return;
            }


            // Create a dialog for the user to choose the output (compressed) file
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Fasta | *.fa";

            // Show dialog
            DialogResult result = saveFileDialog1.ShowDialog();

            // Check if user choose a valid path
            if (result == DialogResult.OK)
            {
                // set outputFilePath with the path the user choose
                outputFilePath = saveFileDialog1.FileName;
                // Decompress the input file
                DecompressFile();
            }
        }




        /// <summary>
        /// This function handles compression
        /// </summary>
        private async void DecompressFile()
        {
            // Disable compression buttons and start progress bar
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.MarqueeAnimationSpeed = 30;
            compressButton.Enabled = false;
            decompressButton.Enabled = false;



            // Decompress file
            int ret = await Task.Run(() => Compress.DecompressFile(inputFilePath, outputFilePath));



            // Enable decompression buttons and stop porgress bar
            compressButton.Enabled = true;
            decompressButton.Enabled = true;

            progressBar1.Style = ProgressBarStyle.Blocks;



            // Show a MessageBox with information about the status of the compression
            showCompressionResult(ret, false);
        }
    }
}
