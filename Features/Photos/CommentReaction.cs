using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PhotoScavengerHunt.Features.Photos
{
    public class CommentReaction
    {
        public int Id { get; set; }
        
        public int CommentId { get; set; }
        
        public int UserId { get; set; }
        
        [Required]
        [MaxLength(10)]
        public string Emoji { get; set; } = "";
        
        public DateTime CreatedAt { get; set; }

        [JsonIgnore]
        public Comment? Comment { get; set; }

        [NotMapped]
        public string UserName { get; set; } = "";
    }
}
