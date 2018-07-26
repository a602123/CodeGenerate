using System;
using System.Collections.Generic;
using System.Text;

namespace #%namespace%#.Model
{
    public class LogBaseModel
    {
        public string Message { get; set; }

        public DateTime Time { get; set; }

        public override string ToString()
        {
            return $"{Time.ToString("yyyy-MM-dd HH:mm:ss")}:{Message}";
        }
    }

    public class LogErrorModel : LogBaseModel
    {
        public Exception Ex { get; set; }

        public override string ToString()
        {
            return $"{Time.ToString("yyyy-MM-dd HH:mm:ss")}:{Message}\r{Ex.Message}";
        }
    }

    public class LogInfoModel : LogBaseModel
    {

    }
}
