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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Compression_Tool
{
    class ResizeClass
    {
        public Dictionary<Control, Tuple<float, float>> ControlSizeRatioDictionary { get; set; }
        public Dictionary<Control, Tuple<float, float>> ControlLocationRatioDictionary { get; set; }
        public Dictionary<Control, float> ControlFontRatioDictionary { get; set; }

        private Form _Form;


        public ResizeClass(Form form)
        {
            ControlSizeRatioDictionary = new Dictionary<Control, Tuple<float, float>>();
            ControlLocationRatioDictionary = new Dictionary<Control, Tuple<float, float>>();
            ControlFontRatioDictionary = new Dictionary<Control, float>();

            _Form = form;
            createInititalValues();
        }



        private void createInititalValues()
        {
            foreach (Control control in _Form.Controls)
            {
                ControlSizeRatioDictionary.Add(
                    control,
                    new Tuple<float, float>(
                        (((float)control.Width) / _Form.Width),
                        (((float)control.Height) / _Form.Height))
                    );


                ControlLocationRatioDictionary.Add(
                 control,
                 new Tuple<float, float>(
                     (((float)control.Left) / _Form.Width),
                     (((float)control.Top) / _Form.Height))
                 );

                ControlFontRatioDictionary.Add(
                    control,
                    control.Font.Size / _Form.Height
                );
            }
        }


        public void Resize()
        {
            foreach (Control control in _Form.Controls)
            {
                Tuple<float, float> tuple = ControlSizeRatioDictionary[control];
                control.Width = (int)(tuple.Item1 * _Form.Width);
                control.Height = (int)(tuple.Item2 * _Form.Height);

                tuple = ControlLocationRatioDictionary[control];
                control.Left = (int)(tuple.Item1 * _Form.Width);
                control.Top = (int)(tuple.Item2 * _Form.Height);

                control.Font = new Font(control.Font.FontFamily, _Form.Height * ControlFontRatioDictionary[control]);
            }
        }
    }
}
