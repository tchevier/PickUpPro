using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PickUpApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace PickUpApp.Controllers;

public class ParentsController : Controller
{
    private MyContext db;
        public ParentsController(MyContext context)
        {
            db = context;
        }
    //Check to see if a parent is in Session
    public Parent CheckForParent()
    {
        int? PID = HttpContext.Session.GetInt32("PID");
        Parent? parent = db.Parents.FirstOrDefault(parent => parent.ParentId == PID);
        if(parent != null)
        {
            return parent;
        }
        return null;
    } 
    [HttpPost("/parents/dashboard/{requestedStudentId}")]
    public IActionResult RequestPickup(int requestedStudentId)
    {
        Student? student = db.Students.FirstOrDefault(student => student.StudentId == requestedStudentId);
        if(student != null)
        {
            student.isRequestedForPickup = 1;
            db.SaveChanges();
        }
        return RedirectToAction("Dashboard", "Parents");
    }
    [HttpGet("/parents/messages")]
    public IActionResult RenderMessages()
    {
        //If a user is already logged in, take them to dashboard
        
        Parent? parent = CheckForParent();
        if(parent != null)
        {
            List<Message> messages = db.Messages.Where(messages => messages.ParentId == parent.ParentId).ToList();
            parent.Messages = messages;
            db.SaveChanges();
            return View("Messages", parent);
        }
        return View("Login");
    }
    //Display Login Form or send parent to dashboard
    [HttpGet("/parents/login")]
    public IActionResult LoginForm()
    {
        //If a user is already logged in, take them to dashboard
        if(CheckForParent() != null){
            return RedirectToAction("Dashboard", "Parents");
        }
        return View("Login");
    }  
    //Log Parent in
    [HttpPost("/parents/login")]
    public IActionResult Login(LoginParent returningParent)
    {
        if(!ModelState.IsValid)
        {
            return LoginForm();
        }
        Parent? parent = db.Parents.FirstOrDefault(parent => parent.Email == returningParent.LoginEmail);
        Console.WriteLine(parent + "This is parent");
        if(parent == null)
        {
            ModelState.AddModelError("LoginEmail", "not found");   
            return LoginForm();
        }
        //Clear any previous admins when switching to new one
        HttpContext.Session.Clear();
        //Check if passwords match
        PasswordHasher<LoginParent> hashBrowns = new PasswordHasher<LoginParent>();
        PasswordVerificationResult pwCompareResult = hashBrowns.VerifyHashedPassword(returningParent, parent.Password, returningParent.LoginPassword);
        if((int)pwCompareResult == 0)
        {
            ModelState.AddModelError("LoginPassword", " is incorrect");
            return LoginForm();
        }
            HttpContext.Session.SetInt32("PID", parent.ParentId);
            return RedirectToAction("Dashboard", "Parents");    
    }    
    //Display Registration Form
    [HttpGet("/parents")]
    public IActionResult RegistrationForm()
    {
        //If a user is already logged in, take them to dashboard
        if(CheckForParent() != null){
            return RedirectToAction("Dashboard", "Parents");
        }
        return View("Registration");
    }    
    //Register a Parent
    [HttpPost("/parents")]
    public IActionResult Register(Parent newParent)
    {
        //Check if model state is valid
        if(!ModelState.IsValid){
            return RegistrationForm();
        }
        //Check if Parent exist in db
        if(db.Parents.Any(Parent => Parent.Email == newParent.Email)){
            ModelState.AddModelError("Email", "already exist");
        }
        //Clear any previous Parents when switching to new one
        HttpContext.Session.Clear();
        //Hash Password
        PasswordHasher<Parent> hashBrowns = new PasswordHasher<Parent>();
        newParent.Password = hashBrowns.HashPassword(newParent, newParent.Password);
        //Add Parent to db
        db.Parents.Add(newParent);
        db.SaveChanges();
        HttpContext.Session.SetInt32("PID", newParent.ParentId);

        return RedirectToAction("Dashboard", "Parents");

    }  
    //Log Parent out
    [HttpPost("/parents/logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Parents");
    }
    //Display Parent Dashboard
    [HttpGet("/parents/dashboard")]
    public IActionResult Dashboard()
    {
        Parent? parent = CheckForParent();
        if(parent != null)
        {
            List<Student> students = db.Students.Where(students => students.ParentId == parent.ParentId).ToList();
            parent.Students = students;
            db.SaveChanges();
            return View("Dashboard", parent);
        }
        return LoginForm();
    } 
    //Display Form to request a student
    [HttpGet("/parents/students")]
    public IActionResult StudentForm()
    {
        Parent? parent = CheckForParent();
        if(parent != null)
        {
            ViewBag.ParentId = parent.ParentId;
            ViewBag.ParentFullName = parent.FullName();
            return View("StudentForm");
        }
        return LoginForm();
    } 
    [HttpPost("/parents/students")]
    public IActionResult RequestStudent(Student newStudent)
    {
        if(!ModelState.IsValid){
            return StudentForm();
        }
        //Check if Student exist in db
        if(db.Students.Any(student => student.FirstName == newStudent.FirstName && student.LastName == newStudent.LastName)){
            ModelState.AddModelError("FirstName", " - this student is already being requested");
            ModelState.AddModelError("LastName", " - this student is already being requested");
            return StudentForm();
        }
        //Check if Student Number exist in db
        if(db.Students.Any(student => student.StudentNumber == newStudent.StudentNumber)){
            ModelState.AddModelError("StudentNumber", " - the student with this number is already being requested");
            return StudentForm();
        }
        //Save student to request database
        db.Students.Add(newStudent);
        db.SaveChanges();

        return RedirectToAction("Dashboard", "Parents");
    } 
    [HttpPost("/parents/students/{deletedStudentId}/delete")]
    public IActionResult DeleteStudent(int deletedStudentId)
    {
        Student? student = db.Students.FirstOrDefault(student => student.StudentId == deletedStudentId);
        if(student != null)
        {
            db.Students.Remove(student);
            db.SaveChanges();
        }
        else {
            // Add alert if the student doesnt exist
        }
        return RedirectToAction("Dashboard", "Parents");
    }
}
