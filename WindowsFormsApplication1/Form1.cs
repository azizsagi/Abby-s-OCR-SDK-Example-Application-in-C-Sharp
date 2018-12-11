using OCR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // See implementation of the RestServiceClient class below
            RestServiceClient restClient = new RestServiceClient();
            restClient.Proxy.Credentials = CredentialCache.DefaultCredentials;

            restClient.Password = "TEXKvMJxh7EiFrlhd1xNqfLX";
            restClient.ApplicationId = "CSR-RBS-APP";
            string sourcePath = @"c:\app\myFile.jpg";
            string targetPath = "output.txt";

            OCR.Task task = restClient.ProcessImage(sourcePath);

            string taskId = task.Id;

            while (true)
            {
                task = restClient.GetTaskStatus(taskId);
                if (!global::OCR.Task.IsTaskActive(task.Status))
                    break;
                System.Threading.Thread.Sleep(1000);
            }

            if (task.Status == "Completed")
            {
                restClient.DownloadResult(task, targetPath);
          
   
            }
            else
            {
                // Error while processing the task.
            }
        }
    }
}
