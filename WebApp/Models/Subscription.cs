using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WebApp.Models;

public class Subscription
{
    public int Id { get; set; }

    [Required]
    public string FollowerId { get; set; } = string.Empty;
    [ForeignKey("FollowerId")]
    public IdentityUser Follower { get; set; } = null!;

    [Required]
    public string FolloweeId { get; set; } = string.Empty;
    [ForeignKey("FolloweeId")]
    public IdentityUser Followee { get; set; } = null!;
}
