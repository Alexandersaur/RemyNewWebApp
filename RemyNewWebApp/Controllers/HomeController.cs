using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RemyNewWebApp.Extensions;
using RemyNewWebApp.Models;
using RemyNewWebApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RemyNewWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBTProjectService _projectService;

        public HomeController(ILogger<HomeController> logger,
                              IBTProjectService projectService)
        {
            _logger = logger;
            _projectService = projectService;
        }

        public async Task<IActionResult> Index()
        {
            List<Project> model = new();
            int companyId = User.Identity.GetCompanyId().Value;
            model = await _projectService.GetAllProjectsByCompany(companyId);
            return View(model);
        }

        public IActionResult Landing()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
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
}
