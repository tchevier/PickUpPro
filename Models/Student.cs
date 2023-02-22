#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PickUpApp.Models;


public class Student
{
    [Key]
    public int StudentId { get; set; }
    [Required]
    public int ParentId { get; set; }
    [Required]
    public string ParentFullName { get; set; }

    [Required(ErrorMessage = "is required")]
    [MinLength(2, ErrorMessage = "must be at least 2 characters.")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }
    [Required(ErrorMessage = "is required")]
    [MinLength(2, ErrorMessage = "must be at least 2 characters.")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }
    [Required(ErrorMessage = "is required")]
    public int StudentNumber { get; set; }

    public byte isConfirmed { get; set; } = 0;
    public byte isRequestedForPickup { get; set; } = 0;
    public byte isPickupConfirmed { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    //Note to think about same first and last names 
    public string FullName()
    {
        return FirstName + " " + LastName;
    }
}
