using System;

namespace PhotoScavengerHunt.Features.Photos
{
    public class AddCommentRequest
    {
        public int UserId { get; set; }
        public string Text { get; set; } = "";
    }
}
