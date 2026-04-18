using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIBA.ComplaintSystem.Data;
using SIBA.ComplaintSystem.Models;
using System.Security.Claims;

namespace SIBA.ComplaintSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            return int.TryParse(userIdClaim, out int id) ? id : 0;
        }

        // 1. Unified Auth Page (Login & Signup)
        public IActionResult Auth()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // 2. Login Logic
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("UserId", user.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, "SibaAuth");
                await HttpContext.SignInAsync("SibaAuth", new ClaimsPrincipal(claimsIdentity));

                if (user.Role == "Admin")
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid email or password.";
            ViewBag.IsLogin = true; // Use this in view to keep Login tab active
            return View("Auth");
        }

        // 3. Sign Up Logic
        [HttpPost]
        public async Task<IActionResult> SignUp(string username, string email, string password)
        {
            // Basic check if email exists
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ViewBag.Error = "Email already registered.";
                ViewBag.IsLogin = false; // Use this in view to keep SignUp tab active
                return View("Auth");
            }

            // Enforce student domain
            if (!email.ToLower().EndsWith("@iba-suk.edu.pk"))
            {
                ViewBag.Error = "Please use a valid @iba-suk.edu.pk student domain.";
                ViewBag.IsLogin = false;
                return View("Auth");
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "All fields are required.";
                ViewBag.IsLogin = false;
                return View("Auth");
            }

            var user = new User
            {
                Username = username,
                Email = email,
                Password = password,
                Role = "Student",
                CreatedAt = DateTime.Now
            };

            _context.Add(user);
            await _context.SaveChangesAsync();

            // Automatically log them in after sign up
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.Id.ToString())
            };
            var claimsIdentity = new ClaimsIdentity(claims, "SibaAuth");
            await HttpContext.SignInAsync("SibaAuth", new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            int userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return RedirectToAction("Auth");
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSettings(string username, string email, string? currentPassword, string? newPassword, string? confirmPassword, IFormFile? profilePicture)
        {
            int userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return RedirectToAction("Auth");

            // Profile Updates
            user.Username = username;
            user.Email = email;

            // Password Reset Logic
            if (!string.IsNullOrEmpty(newPassword))
            {
                if (user.Password != currentPassword)
                {
                    TempData["Error"] = "Current password verification failed.";
                    return RedirectToAction(nameof(Settings));
                }
                if (newPassword != confirmPassword)
                {
                    TempData["Error"] = "Passwords do not match.";
                    return RedirectToAction(nameof(Settings));
                }
                user.Password = newPassword;
            }

            // Profile Picture Upload
            if (profilePicture != null && profilePicture.Length > 0)
            {
                try
                {
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                    if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                    var fileName = $"profile_{userId}_{DateTime.Now.Ticks}{Path.GetExtension(profilePicture.FileName)}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profilePicture.CopyToAsync(stream);
                    }

                    user.ProfilePicturePath = $"/uploads/profiles/{fileName}";
                }
                catch (Exception)
                {
                    TempData["Error"] = "Error while uploading profile picture.";
                }
            }

            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your account settings have been updated successfully.";
            return RedirectToAction(nameof(Settings));
        }

        // 4. Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("SibaAuth");
            return RedirectToAction(nameof(Auth));
        }
    }
}
