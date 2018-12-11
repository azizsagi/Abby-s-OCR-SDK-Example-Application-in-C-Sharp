using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace OCR
{
    public class RestServiceClient
    {
        public RestServiceClient()
        {
            ServerUrl = "http://cloud.ocrsdk.com/";
            Proxy = WebRequest.DefaultWebProxy;
        }

        public string ServerUrl { get; set; }
        public string ApplicationId { get; set; }
        public string Password { get; set; }

        public IWebProxy Proxy { get; set; }

        // Extracts barcode values from entire image.
        public Task ProcessImage(string filePath)
        {
            // Use barcodeRecognition profile to extract barcode values.
            // Save results in XML (you can use any other available output format).
            // For details, see API Reference for processImage method.
            string url = String.Format(
              "{0}/processImage?profile=textExtraction&exportFormat=txt",
              ServerUrl);

            // Build post request.
            WebRequest request = WebRequest.Create(url);
            setupPostRequest(url, request);
            writeFileToRequest(filePath, request);

            XDocument response = performRequest(request);

            Task task = new Task();
            task.Id = response.Root.Element("task").Attribute("id").Value;
            task.Status = response.Root.Element("task").Attribute("status").Value;
            return task;
        }

        // Performs recognition of a barcode field.
        public Task ProcessBarcodeField(string filePath)
        {
            // Specify the region of a barcode (by default, the whole image is recognized), 
            // barcode type, and other parameters.
            // For details, see API Reference for processBarcodeField method.
            string url = String.Format(
              "{0}/processBarcodeField?region=0,0,100,100&barcodeType=pdf417",
              ServerUrl);

            // Build post request.
            WebRequest request = WebRequest.Create(url);
            setupPostRequest(url, request);
            writeFileToRequest(filePath, request);

            XDocument response = performRequest(request);
            Task task = new Task();
            task.Id = response.Root.Element("task").Attribute("id").Value;
            task.Status = response.Root.Element("task").Attribute("status").Value;
            return task;
        }

        private void setupPostRequest(string serverUrl, WebRequest request)
        {
            setupRequest(serverUrl, request);
            request.Method = "POST";
            request.ContentType = "application/octet-stream";
        }
        private void setupRequest(string serverUrl, WebRequest request)
        {
            if (Proxy != null)
                request.Proxy = Proxy;

            // Support authentication in case url is ABBYY SDK.
            if (serverUrl.StartsWith(ServerUrl, StringComparison.InvariantCultureIgnoreCase))
            {
                request.Credentials = new NetworkCredential(ApplicationId, Password);
            }
        }

        private void writeFileToRequest(string filePath, WebRequest request)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(filePath)))
            {
                request.ContentLength = reader.BaseStream.Length;
                using (Stream stream = request.GetRequestStream())
                {
                    byte[] buf = new byte[reader.BaseStream.Length];
                    while (true)
                    {
                        int bytesRead = reader.Read(buf, 0, buf.Length);
                        if (bytesRead == 0)
                        {
                            break;
                        }
                        stream.Write(buf, 0, bytesRead);
                    }
                }
            }
        }
        private XDocument performRequest(WebRequest request)
        {
            using (HttpWebResponse result = (HttpWebResponse)request.GetResponse())
            {
                using (Stream stream = result.GetResponseStream())
                {
                    return XDocument.Load(new XmlTextReader(stream));
                }
            }
        }
        public Task GetTaskStatus(string task)
        {
            string url = String.Format("{0}/getTaskStatus?taskId={1}",
            ServerUrl, Uri.EscapeDataString(task));

            WebRequest request = WebRequest.Create(url);
            setupGetRequest(url, request);
            XDocument response = performRequest(request);
            Task serverTask = new Task();
            serverTask.Id = response.Root.Element("task").Attribute("id").Value;
            serverTask.Status = response.Root.Element("task").Attribute("status").Value;
            if (response.Root.Element("task").Attribute("resultUrl") != null)
            {
                serverTask.DownloadUrl = response.Root.Element("task").Attribute("resultUrl").Value;
            }
            return serverTask;
        }
        private void setupGetRequest(string serverUrl, WebRequest request)
        {
            setupRequest(serverUrl, request);
            request.Method = "GET";
        }

        // Download the resulting file and save it to given location.
        public void DownloadResult(Task task, string outputFile)
        {
            string url = task.DownloadUrl;
            WebRequest request = WebRequest.Create(url);
            setupGetRequest(url, request);
            using (HttpWebResponse result = (HttpWebResponse)request.GetResponse())
            {
                using (Stream stream = result.GetResponseStream())
                {
                    // Write result directly to file.
                    using (Stream file = File.OpenWrite(outputFile))
                    {
                        copyStream(stream, file);
                    }
                }
            }
        }

        private static void copyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }
    }
}
