using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IWebHostEnvironment _hostEnvironment;

    public ProfileController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment hostEnvironment)
    {
        _context = context;
        _userManager = userManager;
        _hostEnvironment = hostEnvironment;
    }

    // Показати профіль користувача
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            userId = _userManager.GetUserId(User);
        }

        if (userId == null)
            return Redirect("/Identity/Account/Login");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var photos = await _context.Photos
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        ViewBag.User = user;
        ViewBag.CurrentUserId = _userManager.GetUserId(User);
        ViewBag.IsOwner = userId == _userManager.GetUserId(User);

        return View(photos);
    }

    // Сторінка завантаження фото
    public IActionResult UploadPhoto()
    {
        return View();
    }

    // Завантажити фото
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPhoto(IFormFile? photo, string? description)
    {
        var userId = _userManager.GetUserId(User);

        if (photo == null || photo.Length == 0)
        {
            ModelState.AddModelError("", "Будь ласка, оберіть файл");
            return View();
        }

        // Перевіримо тип файлу
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var fileExtension = Path.GetExtension(photo.FileName).ToLower();
        if (!allowedExtensions.Contains(fileExtension))
        {
            ModelState.AddModelError("", "Дозволені тільки наступні формати: jpg, jpeg, png, gif");
            return View();
        }

        try
        {
            var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{userId}_{DateTime.Now.Ticks}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(fileStream);
            }

            var photoModel = new Photo
            {
                UserId = userId ?? string.Empty,
                ImagePath = $"/uploads/{fileName}",
                Description = description ?? string.Empty,
                CreatedAt = DateTime.Now
            };

            _context.Photos.Add(photoModel);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { userId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Помилка при завантаженні: {ex.Message}");
            return View();
        }
    }

    // Видалити фото
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        var photo = await _context.Photos.FindAsync(id);
        if (photo == null)
            return NotFound();

        var currentUserId = _userManager.GetUserId(User);
        if (photo.UserId != currentUserId)
            return Unauthorized();

        var filePath = Path.Combine(_hostEnvironment.WebRootPath, photo.ImagePath.TrimStart('/'));
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);

        _context.Photos.Remove(photo);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", new { userId = currentUserId });
    }

    // Лайкнути фото
    [HttpPost]
    public async Task<IActionResult> LikePhoto(int photoId)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (currentUserId == null)
            return Unauthorized();

        var photo = await _context.Photos
            .Include(p => p.Likes)
            .FirstOrDefaultAsync(p => p.Id == photoId);

        if (photo == null)
            return NotFound();

        var existingLike = await _context.Likes
            .FirstOrDefaultAsync(l => l.PhotoId == photoId && l.UserId == currentUserId);

        if (existingLike != null)
        {
            // Вже лайковано, видаляємо лайк
            _context.Likes.Remove(existingLike);
        }
        else
        {
            // Додаємо новий лайк
            var like = new Like
            {
                PhotoId = photoId,
                UserId = currentUserId
            };
            _context.Likes.Add(like);
        }

        await _context.SaveChangesAsync();

        return Json(new { success = true, likeCount = photo.Likes.Count });
    }
}
