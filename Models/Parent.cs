#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PickUpApp.Models;


public class Parent
{
    [Key]
    public int ParentId { get; set; }

    [Required(ErrorMessage = "is required")]
    [MinLength(2, ErrorMessage = "must be at least 2 characters.")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "is required")]
    [MinLength(2, ErrorMessage = "must be at least 2 characters.")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "is required")]
    [EmailAddress]
    public string Email { get; set; }

    [Required(ErrorMessage = "is required")]
    [MinLength(8, ErrorMessage = "must be at least 8 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [NotMapped]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "doesn't match password.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set;}

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public List<Student> Students = new List<Student>();
    public List<Message> Messages { get; set; } = new List<Message>();

    public void AddMessage(Message message)
    {
        Messages.Add(message);
    }
    public string FullName()
    {
        return FirstName + " " + LastName;
    }
}
