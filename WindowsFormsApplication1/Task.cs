using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR
{
    public class Task
    {
        public string Id;
        public string Status;

        // Url to download processed tasks.
        public string DownloadUrl = null;

        public Task()
        {
            Status = "<unknown>";
            Id = "<unknown>";
        }

        public Task(string id, string status)
        {
            Id = id;
            Status = status;
        }

        public bool IsTaskActive()
        {
            return IsTaskActive(Status);
        }

        // Task is submitted or is processing.
        public static bool IsTaskActive(string status)
        {
            if (status == "Submitted" ||
            status == "Queued" ||
            status == "InProgress")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
