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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression_Tool
{
    /// <summary>
    /// Compression Class
    /// </summary>
    class Compress
    {
        // Size of the binary buffer
        private static readonly int BUFFER_SIZE = 1024 * 1024; // 1 MB

        // Buffers
        private static byte[] binaryBuffer;
        private static char[] stringBuffer;

        // Dictionary of DNA letters to 2 bit values and 2 bit values to DNA letters
        public static Dictionary<char, byte> LettersToDigit;
        public static Dictionary<byte, char> DigitToLetter;






        /// <summary>
        /// Compress constructor
        /// </summary>
        static Compress()
        {
            // Initialize buffers
            binaryBuffer = new byte[BUFFER_SIZE];
            stringBuffer = new char[4 * BUFFER_SIZE];


            // Creates dictionary of DNA letters to 2 bits values
            LettersToDigit = new Dictionary<char, byte>();
            LettersToDigit.Add('A', 0);
            LettersToDigit.Add('C', 1);
            LettersToDigit.Add('T', 2);
            LettersToDigit.Add('G', 3);



            // Creates dictionary of 2 bits values to DNA letters
            DigitToLetter = new Dictionary<byte, char>();
            DigitToLetter.Add(0, 'A');
            DigitToLetter.Add(1, 'C');
            DigitToLetter.Add(2, 'T');
            DigitToLetter.Add(3, 'G');
        }




        #region Compression functions




        /// <summary>
        /// Puts the input compressed in binaryBuffer (without a header)
        /// 
        /// --------------Compression Algorithm-------------------
        /// 1) Divide the input to blocks of 4 bytes
        /// 2) Compress every block to a single byte by substituding each character with its 2 bit value
        /// </summary>
        /// 
        /// <param name="input">A char[] containing the letters: A,C,T and G</param>
        /// <param name="size">Length of data in the "input" parameter</param>
        /// <returns>Index of the first unwritten cell in the binaryBuffer</returns>
        private static int CompressInput(char[] input, int size)
        {
            // Check if the input is too big for the binaryBuffer
            if (input.Length > 4 * BUFFER_SIZE)
            {
                // Throw Expception if the input buffer is too big
                throw new Exception(String.Format("Input too big for buffer. \nMake sure the input is at most 4*BUFFER_SIZE ({1}) characters long", BUFFER_SIZE, 4 * BUFFER_SIZE));
            }




            byte compressedValue; // value of compressed block
            byte length; // Size of block

            // Index of the first unwritten cell in the binaryBuffer
            // Initialized to 0 because the binaryBuffer is empty
            int index = 0;




            // Iterate over blocks of 4 bytes in the input array
            for (int i = 0; i < size; i += 4)
            {
                // Compressed value of the current block
                // Initialized to 0 because no characters were read
                compressedValue = 0;

                // Find the size of the next block
                // 4 by default. Shorter if reached the end of the input.
                length = (byte)Math.Min(4, size - i);



                // Iterate over the next block
                for (int j = 0; j < length; j++)
                {
                    // Shift value 2 bits left to make room for the next character in the input
                    compressedValue *= 4;

                    // Return an error if the input contain invalid characters
                    if (!LettersToDigit.ContainsKey(input[i + j]))
                        return -1;

                    // Add the 2 bit value of the current character
                    compressedValue += LettersToDigit[input[i + j]];
                }



                // Shift left the value when the byte is not full
                compressedValue *= (byte)Math.Pow(4, 4 - length);

                // adds byte to binaryBuffer
                binaryBuffer[index] = compressedValue;

                // Increment last unwritten index in the binaryBuffer.
                index++;
            }

            return index;
        }



        /// <summary>
        /// Compress file
        /// 
        /// ---------------------Format---------------------
        /// byte :    0                   1                ...
        /// value: |HEADER|letter1,letter2,letter3,letter4|...
        /// HEADER = number of letters in last byte
        /// </summary>
        /// <param name="inputFilePath">Path to the input file</param>
        /// <param name="outputFilePath">Path to the output file</param>
        /// <returns>Success code: 0 = Success; -1 = Invalid input; -2 = IOException;</returns>
        public static int CompressFile(string inputFilePath, string outputFilePath)
        {
            // Header of the compressed file
            // Contains the number of letters in the last byte
            byte header = 0;


            int size; // Number of bytes read from the input
            int index; // Number of bytes in the output buffer
            //char[] stringBuffer = new char[4 * BUFFER_SIZE]; // Input buffer

            BinaryWriter binaryWriter = null; // Output binary stream
            StreamReader streamReader = null; // Input stream

            try
            {
                // Open input and output streams
                binaryWriter = new BinaryWriter(new FileStream(outputFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None));
                streamReader = new StreamReader(inputFilePath);

                // Read BUFFER_SIZE bytes from the input file into stringBuffer
                size = streamReader.Read(stringBuffer, 0, stringBuffer.Length);

                // Write placeholder value for the header in the first byte of the output
                binaryWriter.Write((byte)0);

                // Compress while the input is not empty
                while (size != 0)
                {
                    // Compresses stringBuffer
                    index = CompressInput(stringBuffer, size);

                    // Return error code if the compression failed
                    if (index < 0)
                        return -1;

                    // Write compressed buffer to the output file
                    binaryWriter.Write(binaryBuffer, 0, index);

                    // Calculate header value
                    header = (byte)(size % 4);
                    if (header == 0)
                        header = 4;

                    // Read BUFFER_SIZE bytes from the input file into stringBuffer
                    size = streamReader.Read(stringBuffer, 0, stringBuffer.Length);
                }

                // Seek to start of the output file
                binaryWriter.Seek(0, SeekOrigin.Begin);
                // Write header to output file
                binaryWriter.Write(header);
            }

            // Catches IO Exceptions that were thrown during the compression process
            catch (IOException)
            {
                // IOException error code
                return -2;
            }

            // Close input and output streams
            finally
            {
                // Close binaryWriter stream
                if (binaryWriter != null)
                    binaryWriter.Close();

                // Close streamReader stream
                if (streamReader != null)
                    streamReader.Close();
            }

            // Success code
            return 0;
        }




        #endregion




        #region Decompression functions


        /// <summary>
        /// This function decompresses the input parameter
        /// </summary>
        /// <param name="input">A buffer contaning compressed DNA data</param>
        /// <param name="size">number of bytes in the input buffer</param>
        private static void DecompressInput(byte[] input, int size)
        {
            char letter;

            // iterate over the input
            for (int i = 0; i < size; i++)
            {
                // decompress a single byte (4 letters)
                for (int j = 0; j < 4; j++)
                {
                    // convert the 2 LSB in the decompressed cell to the letter they represent
                    letter = DigitToLetter[(byte)(input[i] % 4)];
                    
                    // Shift the data in the cell two bits to the left (get rid of already decompressed data)
                    input[i] /= 4;

                    // Add the new letter to the ouput buffer
                    stringBuffer[4 * i + (3 - j)] = letter;
                }
            }
        }



        /// <summary>
        /// Decompress file (.ditx)
        /// </summary>
        /// <param name="inputFilePath">Path to the input file (compressed .ditx)</param>
        /// <param name="outputFilePath">Path to the output file (fasta .fa)</param>
        /// <returns></returns>
        public static int DecompressFile(string inputFilePath, string outputFilePath)
        {
            // Header of the compressed file
            // Contains the number of letters in the last byte
            byte header;

            // Number of bytes read from the input
            int size; 

            // Number of bytes to write to the output
            int writeSize;

            BinaryReader binaryReader = null; // Input Binary stream
            StreamWriter streamWriter = null; // Output stream

            try
            {
                // Open stream to the input file
                FileStream fs = new FileStream(inputFilePath, FileMode.Open);
                
                // If the file stream is null (can't open the input file) return -2 (IO error).
                if (fs == null)
                    return -2;

                // Find the file size
                fs.Seek(0, SeekOrigin.End);
                long fileLength = fs.Position;
                fs.Seek(0, SeekOrigin.Begin);

                // Open input and output streams
                streamWriter = new StreamWriter(new FileStream(outputFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None));
                binaryReader = new BinaryReader(fs);

                // Read the header from the compressed file
                header = binaryReader.ReadByte();

                // Read BUFFER_SIZE bytes from the input file into stringBuffer
                size = binaryReader.Read(binaryBuffer, 0, binaryBuffer.Length);

                // decompress while the input is not empty
                while (size != 0)
                {
                    // decompress binaryBuffer
                    DecompressInput(binaryBuffer, size);

                    // Set number of characters to write to the ouput file
                    writeSize = 4*size;
                    if (fs.Position == fileLength)
                        writeSize -= 4 - header;

                    // Write decompressed buffer to the output file
                    streamWriter.Write(stringBuffer, 0, writeSize);

                    // Read BUFFER_SIZE bytes from the input file into binaryBuffer
                    size = binaryReader.Read(binaryBuffer, 0, binaryBuffer.Length);
                }
            }
            // Catches IO Exceptions that were thrown during the decompression process
            catch (IOException)
            {
                // IOException error code
                return -2;
            }

            // Close input and output streams
            finally
            {
                // Close binaryReader stream
                if (binaryReader != null)
                    binaryReader.Close();

                // Close streamReader stream
                if (streamWriter != null)
                    streamWriter.Close();
            }

            // Success code
            return 0;
        }
    }




    #endregion
}
