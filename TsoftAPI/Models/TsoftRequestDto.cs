namespace TsoftAPI.Models
{
    public class TsoftRequestDto
    {
        public string Provider { get; set; }
        public string ProjectName { get; set; }
        public string SessionId { get; set; }
        public string AuthToken { get; set; }
        public string CustomerId { get; set; }
        public string ActionType { get; set; }
    }
}
