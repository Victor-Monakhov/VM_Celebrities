namespace VM_Celebrities_Back.Models
{
    public class Celebrity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Movie { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Info { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string[] Roles { get; set; } = [];
    }
}