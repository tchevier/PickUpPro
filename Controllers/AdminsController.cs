using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PickUpApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace PickUpApp.Controllers;

public class AdminsController : Controller
{
    private MyContext db;
        public AdminsController(MyContext context)
        {
            db = context;
        }
    //Check to see if a admin is in Session
    public Admin CheckForAdmin()
    {
        int? AID = HttpContext.Session.GetInt32("AID");
        Admin? admin = db.Admins.FirstOrDefault(admin => admin.AdminId == AID);
        if(admin != null)
        {
            return admin;
        }
        return null;
    } 
    [HttpGet("/admins/dashboard/students")]
    public IActionResult ConfirmStudentDashboard()
    {
        Admin? admin = CheckForAdmin();
        if(admin != null)
        {
            List<Student> students = db.Students.Where(student => student.isConfirmed == 0).ToList();
            admin.Students = students;
            db.SaveChanges();
            return View("ConfirmStudents", admin);
        }
        return LoginForm();
    }
    [HttpPost("/admins/dashboard/{editedStudentId}/confirm")]
    public IActionResult ConfirmStudent(int editedStudentId)
    {
        Student? student = db.Students.FirstOrDefault(student => student.StudentId == editedStudentId);
        if(student != null)
        {
            Parent? studentsParent = db.Parents.FirstOrDefault(parent => parent.ParentId == student.ParentId);
            if(studentsParent != null)
            {
                Message newMessage = new Message();
                newMessage.Content = "The request for student " + student.FirstName + " was confirmed by the admin. " + newMessage.CreatedAt;
                newMessage.ParentId = studentsParent.ParentId;
                studentsParent.AddMessage(newMessage);
                db.SaveChanges();
            }
            student.isConfirmed = 1;
            db.SaveChanges();
        }
        return RedirectToAction("ConfirmStudentDashboard", "Admins");
    }
    [HttpPost("/admins/dashboard/{editedStudentId}/delete")]
    public IActionResult DenyStudent(int editedStudentId)
    {
        Student? student = db.Students.FirstOrDefault(student => student.StudentId == editedStudentId);
        if(student != null)
        {
            Parent? studentsParent = db.Parents.FirstOrDefault(parent => parent.ParentId == student.ParentId);
            if(studentsParent != null)
            {
                Message newMessage = new Message();
                newMessage.Content = "The request for student " + student.FirstName + " was denied by the admin. Make sure the name and school number all correspond with the school's info. " + newMessage.CreatedAt;
                newMessage.ParentId = studentsParent.ParentId;
                studentsParent.AddMessage(newMessage);
                db.SaveChanges();
            }
            db.Students.Remove(student);
            db.SaveChanges();
        }
        return RedirectToAction("ConfirmStudentDashboard", "Admins");
    }
    [HttpGet("/admins/dashboard/confirmpickups")]
    public IActionResult ConfirmStudentPickupDashboard()
    {
        Admin? admin = CheckForAdmin();
        if(admin != null)
        {
            List<Student> students = db.Students.Where(student => student.isRequestedForPickup == 1).ToList();
            admin.Students = students;
            db.SaveChanges();
            return View("ConfirmPickups", admin);
        }
        return LoginForm();
    }
    [HttpPost("/admins/dashboard/confirmpickups/{confirmedStudentId}")]
    public IActionResult ConfirmStudentPickup(int confirmedStudentId)
    {
        Student? student = db.Students.FirstOrDefault(student => student.StudentId == confirmedStudentId);
        if(student != null)
        {
            student.isPickupConfirmed = 1;
            student.isRequestedForPickup = 0;
            db.SaveChanges();
        }
        return RedirectToAction("ConfirmStudentPickupDashboard", "Admins");
    }
    [HttpGet("/admins/login")]
    public IActionResult LoginForm()
    {
        //If a user is already logged in, take them to dashboard
        if(CheckForAdmin() != null){
            return RedirectToAction("Dashboard", "Admin");
        }
        return View("Login");
    }  
        //Log admin in
    [HttpPost("/admins/login")]
    public IActionResult Login(LoginAdmin returningAdmin)
    {
        if(!ModelState.IsValid)
        {
            return LoginForm();
        }
        Admin? admin = db.Admins.FirstOrDefault(admin => admin.Username == returningAdmin.LoginUsername);
        if(admin == null)
        {
            ModelState.AddModelError("LoginUsername", "not found");   
            return LoginForm();
        }
        //Clear any previous admins when switching to new one
        HttpContext.Session.Clear();
        //Check if passwords match
        PasswordHasher<LoginAdmin> hashBrowns = new PasswordHasher<LoginAdmin>();
        PasswordVerificationResult pwCompareResult = hashBrowns.VerifyHashedPassword(returningAdmin, admin.Password, returningAdmin.LoginPassword);
        if((int)pwCompareResult == 0)
        {
            ModelState.AddModelError("LoginPassword", " is incorrect");
            return LoginForm();
        }
            HttpContext.Session.SetInt32("AID", admin.AdminId);
            return RedirectToAction("Dashboard", "Admins");
    }  
     //Display Registration Form
    [HttpGet("/admins")]
    public IActionResult RegistrationForm()
    {
        //If an admin exist in the db, don't render the form, just send user to login page
        if(CheckForAdmin() != null){
            return RedirectToAction("Dashboard", "Admins");
        }
        return View("Registration");
    }    
    //Register a admin
    [HttpPost("/admins")]
    public IActionResult Register(Admin newAdmin)
    {
        //Check if model state is valid
        if(!ModelState.IsValid){
            return RegistrationForm();
        }
        //Clear any previous admins when switching to new one
        HttpContext.Session.Clear();
        //Hash Password
        PasswordHasher<Admin> hashBrowns = new PasswordHasher<Admin>();
        newAdmin.Password = hashBrowns.HashPassword(newAdmin, newAdmin.Password);
        //Add admin to db
        db.Admins.Add(newAdmin);
        db.SaveChanges();
        HttpContext.Session.SetInt32("AID", newAdmin.AdminId);

        return RedirectToAction("Dashboard", "Admins");

    }     
    //Display admin Dashboard
    [HttpGet("/admins/dashboard")]
    public IActionResult Dashboard()
    {
        Admin? admin = CheckForAdmin();
        if(admin != null)
        {
            List<Student> students = db.Students.Where(student => student.isConfirmed == 0).ToList();
            admin.Students = students;
            db.SaveChanges();
            return View("Dashboard", admin);
        }
        return LoginForm();
    } 
        //Log admin out
    [HttpPost("/admins/logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Admins");
    }
    [HttpPost("/admins/dashboard/reset")]
    public IActionResult ResetStudentPickups(int adminId)
    {
        Admin? admin = db.Admins.FirstOrDefault(admin => admin.AdminId == adminId);
        if(admin != null)
        {
            foreach(Student student in db.Students)
            {
                student.isRequestedForPickup = 0;
                student.isPickupConfirmed = 0;
                
            }
            db.SaveChanges();
            return RedirectToAction("Dashboard", "Admins");
        }
        return LoginForm();
    }
}
