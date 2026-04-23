using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WebApp.Models;

public class Photo
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public IdentityUser User { get; set; } = null!;

    [Required]
    public string ImagePath { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Like> Likes { get; set; } = new List<Like>();
}

public class Like
{
    public int Id { get; set; }

    [Required]
    public int PhotoId { get; set; }

    [ForeignKey("PhotoId")]
    public Photo Photo { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public IdentityUser User { get; set; } = null!;
}