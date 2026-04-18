using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIBA.ComplaintSystem.Data;
using SIBA.ComplaintSystem.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace SIBA.ComplaintSystem.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public HomeController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    private int GetCurrentUserId() => int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

    public async Task<IActionResult> Index()
    {
        int userId = GetCurrentUserId();
        
        // If Admin, they should really be in AdminController, but let's handle it
        if (User.IsInRole("Admin")) return RedirectToAction("Index", "Admin");

        ViewBag.Total = await _context.Complaints.CountAsync(c => c.UserId == userId);
        ViewBag.Pending = await _context.Complaints.CountAsync(c => c.Status == "Pending" && c.UserId == userId);
        ViewBag.InProgress = await _context.Complaints.CountAsync(c => c.Status == "In Progress" && c.UserId == userId);
        ViewBag.Resolved = await _context.Complaints.CountAsync(c => c.Status == "Resolved" && c.UserId == userId);

        return View();
    }

    public IActionResult Create()
    {
        ViewBag.UserName = User.Identity?.Name;
        ViewBag.UserEmail = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Complaint complaint, IFormFile? attachment)
    {
        if (ModelState.IsValid)
        {
            if (attachment != null && attachment.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "complaints");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(attachment.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await attachment.CopyToAsync(stream);
                }
                complaint.AttachmentPath = "/uploads/complaints/" + uniqueFileName;
            }

            complaint.Status = "Pending";
            complaint.CreatedAt = DateTime.Now;
            complaint.UserId = GetCurrentUserId(); // Link to logged in user

            _context.Add(complaint);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Your complaint has been submitted successfully!";
            return RedirectToAction(nameof(List));
        }

        ViewBag.UserName = User.Identity?.Name;
        ViewBag.UserEmail = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
        return View(complaint);
    }

    public async Task<IActionResult> List()
    {
        int userId = GetCurrentUserId();
        var complaints = await _context.Complaints
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return View(complaints);
    }

    public async Task<IActionResult> Details(int id)
    {
        int userId = GetCurrentUserId();
        var complaint = await _context.Complaints
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (complaint == null) return NotFound();

        ViewBag.Messages = await _context.ComplaintMessages
            .Include(m => m.Sender)
            .Where(m => m.ComplaintId == id)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        return View(complaint);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMessage(int complaintId, string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return RedirectToAction(nameof(Details), new { id = complaintId });

        // Lock discussion if resolved
        var complaint = await _context.Complaints.FindAsync(complaintId);
        if (complaint == null || complaint.Status == "Resolved") return RedirectToAction(nameof(Details), new { id = complaintId });

        var message = new ComplaintMessage
        {
            ComplaintId = complaintId,
            SenderId = GetCurrentUserId(),
            Content = content,
            SentAt = DateTime.Now
        };

        _context.ComplaintMessages.Add(message);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = complaintId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        int userId = GetCurrentUserId();
        var complaint = await _context.Complaints
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (complaint == null) return NotFound();

        // Students can only delete resolved tickets for dashboard cleanup
        if (complaint.Status != "Resolved")
        {
            ViewBag.Error = "Only resolved complaints can be removed.";
            return RedirectToAction(nameof(List));
        }

        // Remove associated messages
        var messages = await _context.ComplaintMessages.Where(m => m.ComplaintId == id).ToListAsync();
        _context.ComplaintMessages.RemoveRange(messages);

        _context.Complaints.Remove(complaint);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Resolved complaint archived to history and removed from list.";
        return RedirectToAction(nameof(List));
    }
    public async Task<IActionResult> ViewProof(int id)
    {
        int userId = GetCurrentUserId();

        // 1. Verify complaint ownership
        var complaint = await _context.Complaints
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (complaint == null) return NotFound();

        // 2. Fetch the corresponding archive
        var archive = await _context.ComplaintArchives
            .FirstOrDefaultAsync(a => a.ComplaintId == id);

        if (archive == null) return NotFound();

        // 3. Reuse the existing professional proof view
        return View("~/Views/Admin/ViewProof.cshtml", archive);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
