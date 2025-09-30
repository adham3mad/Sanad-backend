namespace Sanad.Models
{
    public class AiApiResponse
    {
        public int status { get; set; }
        public string? query { get; set; }
        public string? response { get; set; }
        public string? answer { get; set; }
        public string? source_docs { get; set; }
    }

}
