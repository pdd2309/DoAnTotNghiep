using Microsoft.AspNetCore.Mvc;
using DoAnTotNghiep.Services;

namespace DoAnTotNghiep.Controllers;

public class ShopController : Controller
{
    private readonly IDanhMucService _danhMucService;

    public ShopController(IDanhMucService danhMucService)
    {
        _danhMucService = danhMucService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var categories = await _danhMucService.GetAllDanhMucsAsync();
        return View(categories);
    }

    [HttpGet]
    public async Task<IActionResult> Grid(int? categoryId)
    {
        var categories = await _danhMucService.GetAllDanhMucsAsync();
        ViewBag.Categories = categories;
        ViewBag.SelectedCategoryId = categoryId;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var categories = await _danhMucService.GetAllDanhMucsAsync();
        ViewBag.Categories = categories;
        return View();
    }
}