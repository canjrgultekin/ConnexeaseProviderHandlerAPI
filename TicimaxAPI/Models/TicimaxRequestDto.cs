namespace TicimaxAPI.Models
{
    public class TicimaxRequestDto
    {
        public string Provider { get; set; }
        public string ProjectName { get; set; }
        public string SessionId { get; set; }
        public string AuthToken { get; set; }
        public string CustomerId { get; set; }
        public string ActionType { get; set; }
    }
}
