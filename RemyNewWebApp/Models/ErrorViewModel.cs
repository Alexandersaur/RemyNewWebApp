using System;

namespace RemyNewWebApp.Models
{
    public class ErrorViewModel
    {
        // -- Foreign Keys -- //

        public string RequestId { get; set; }

        // -- Properties -- //

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
