//namespace wellcare.Models
//{
//   public class ErrorViewModel
//    {
//        public string? RequestId { get; set; }
//        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
//    }
//}

namespace wellcare.Models
{
    public class ApiErrorResponse
    {
        public string Message { get; set; } = "An unexpected error occurred.";

        public string? RequestId { get; set; }

        public string? Details { get; set; } // optional: for dev/debug
    }
}

