using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RemyNewWebApp.Extensions;
using RemyNewWebApp.Models;
using RemyNewWebApp.Models.ViewModels;
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
        private readonly IBTRolesService _rolesService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyInfoService _companyInfoService;

        public HomeController(ILogger<HomeController> logger,
                              IBTProjectService projectService,
                              IBTRolesService rolesService,
                              UserManager<BTUser> userManager,
                              IBTCompanyInfoService companyInfoService)
        {
            _logger = logger;
            _projectService = projectService;
            _rolesService = rolesService;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
        }

        public async Task<IActionResult> Index()
        {
            //List<Project> model = new();
            //int companyId = User.Identity.GetCompanyId().Value;
            //model = await _projectService.GetAllProjectsByCompany(companyId);
            //return View(model);

            List<BTUserViewModel> model = new();
            int companyId = User.Identity.GetCompanyId().Value;
            List<BTUser> members = await _companyInfoService.GetAllMembersAsync(companyId);

            foreach (BTUser member in members)
            {
                BTUserViewModel viewModel = new();
                viewModel.BTUser = member;
                viewModel.Roles = (await _rolesService.GetUserRolesAsync(member)).ToList();
                model.Add(viewModel);
            }

            return View(model);
        }

        public IActionResult Landing()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            List<BTUserViewModel> model = new();
            int companyId = User.Identity.GetCompanyId().Value;
            List<BTUser> members = await _companyInfoService.GetAllMembersAsync(companyId);

            foreach (BTUser member in members)
            {
                BTUserViewModel viewModel = new();
                viewModel.BTUser = member;
                viewModel.Roles = (await _rolesService.GetUserRolesAsync(member)).ToList();
                model.Add(viewModel);
            }

            return View(model);
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
