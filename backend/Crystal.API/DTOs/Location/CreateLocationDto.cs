namespace Crystal.API.DTOs.Location
{
    public class CreateLocationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}