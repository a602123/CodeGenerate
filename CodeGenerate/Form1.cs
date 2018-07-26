using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace CodeGenerate
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != null)
            {
                CodeGeneratorHelper.ReplaceGenerate(textBox1.Text);
                System.Diagnostics.Process.Start("Explorer.exe","ResultFile");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var i=CodeGeneratorHelper.Read(@"..\..\Template\clas1.txt");

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
