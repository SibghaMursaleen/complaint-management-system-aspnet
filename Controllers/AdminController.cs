using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIBA.ComplaintSystem.Data;
using SIBA.ComplaintSystem.Models;

namespace SIBA.ComplaintSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Admin Dashboard: See all complaints
        public async Task<IActionResult> Index()
        {
            var complaints = await _context.Complaints
                .Include(c => c.User)
                .Where(c => c.Status != "Resolved")
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(complaints);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var complaint = await _context.Complaints
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (complaint == null) return NotFound();

            var oldStatus = complaint.Status;
            complaint.Status = status;

            // If moving to Resolved, create a permanent archive
            if (status == "Resolved" && oldStatus != "Resolved")
            {
                var messages = await _context.ComplaintMessages
                    .Include(m => m.Sender)
                    .Where(m => m.ComplaintId == id)
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();

                var transcript = new System.Text.StringBuilder();
                transcript.AppendLine($"--- COMPLAINT RESOLUTION LOG ---");
                transcript.AppendLine($"Ticket ID: #{complaint.Id}");
                transcript.AppendLine($"Title: {complaint.Title}");
                transcript.AppendLine($"Category: {complaint.Category}");
                transcript.AppendLine($"Student: {(complaint.IsAnonymous ? "Anonymous Student" : complaint.User?.Username)}");
                transcript.AppendLine($"Date Filed: {complaint.CreatedAt:MMM dd, yyyy HH:mm}");
                transcript.AppendLine($"Resolved On: {DateTime.Now:MMM dd, yyyy HH:mm}");
                transcript.AppendLine($"Resolved By: {User.Identity?.Name}");
                transcript.AppendLine();
                transcript.AppendLine("--- ORIGINAL DESCRIPTION ---");
                transcript.AppendLine(complaint.Description);
                transcript.AppendLine();
                transcript.AppendLine("--- DISCUSSION THREAD ---");

                if (!messages.Any())
                {
                    transcript.AppendLine("[No messages recorded in discussion]");
                }
                else
                {
                    foreach (var msg in messages)
                    {
                        var senderName = msg.Sender?.Role == "Admin" ? "OFFICIAL STAFF" : (complaint.IsAnonymous ? "STUDENT (ANONYMOUS)" : msg.Sender?.Username);
                        transcript.AppendLine($"[{msg.SentAt:yyyy-MM-dd HH:mm}] {senderName}: {msg.Content}");
                    }
                }

                var archive = new ComplaintArchive
                {
                    ComplaintId = complaint.Id,
                    ComplaintTitle = complaint.Title,
                    StudentName = complaint.IsAnonymous ? "Anonymous Student" : (complaint.User?.Username ?? "Unknown"),
                    FullTranscript = transcript.ToString(),
                    ResolvedAt = DateTime.Now,
                    ResolvedBy = User.Identity?.Name ?? "Admin"
                };

                _context.ComplaintArchives.Add(archive);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Action to see all resolved/archived complaints
        public async Task<IActionResult> Resolved()
        {
            var archives = await _context.ComplaintArchives
                .OrderByDescending(a => a.ResolvedAt)
                .ToListAsync();
            return View(archives);
        }

        // Action to view the specific "Proof"
        public async Task<IActionResult> ViewProof(int id)
        {
            var archive = await _context.ComplaintArchives.FindAsync(id);
            if (archive == null) return NotFound();
            return View(archive);
        }

        // Action to delete a complaint
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint != null)
            {
                _context.Complaints.Remove(complaint);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/CreateAdmin
        public IActionResult CreateAdmin()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            if (userEmail != "senior@admin.siba.edu.pk")
            {
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        // POST: Admin/CreateAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(string username, string email, string password)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            if (userEmail != "senior@admin.siba.edu.pk")
            {
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "All fields are required.";
                return View();
            }

            // Enforce domain for new admins
            if (!email.ToLower().EndsWith("@admin.siba.edu.pk"))
            {
                ViewBag.Error = "Admin email must use the @admin.siba.edu.pk domain.";
                return View();
            }

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ViewBag.Error = "Email already registered.";
                return View();
            }

            var admin = new User
            {
                Username = username,
                Email = email,
                Password = password,
                Role = "Admin",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Admin user created successfully!";
            return View();
        }
        // GET: Admin/ManageStaff
        public async Task<IActionResult> ManageStaff()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            if (userEmail != "senior@admin.siba.edu.pk")
            {
                return RedirectToAction(nameof(Index));
            }

            var staff = await _context.Users
                .Where(u => u.Role == "Admin" && u.Email != "senior@admin.siba.edu.pk")
                .ToListAsync();

            return View(staff);
        }

        // POST: Admin/DeleteStaff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            if (userEmail != "senior@admin.siba.edu.pk")
            {
                return RedirectToAction(nameof(Index));
            }

            var staff = await _context.Users.FindAsync(id);
            if (staff != null && staff.Role == "Admin" && staff.Email != "senior@admin.siba.edu.pk")
            {
                _context.Users.Remove(staff);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ManageStaff));
        }

        public async Task<IActionResult> Details(int id)
        {
            var complaint = await _context.Complaints
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

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
            
            // Check if resolved - Admin cannot reply to already resolved tickets to keep proof immutable
            var complaint = await _context.Complaints.FindAsync(complaintId);
            if (complaint == null || complaint.Status == "Resolved") return RedirectToAction(nameof(Details), new { id = complaintId });

            // Get Admin ID from claims
            var userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var message = new ComplaintMessage
            {
                ComplaintId = complaintId,
                SenderId = userId,
                Content = content,
                SentAt = DateTime.Now
            };

            _context.ComplaintMessages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = complaintId });
        }
    }
}
