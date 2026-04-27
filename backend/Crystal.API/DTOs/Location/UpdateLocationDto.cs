namespace Crystal.API.DTOs.Location
{
    public class UpdateLocationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}