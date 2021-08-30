using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RemyNewWebApp.Models;
using RemyNewWebApp.Extensions;
using RemyNewWebApp.Models.ViewModels;
using RemyNewWebApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemyNewWebApp.Controllers
{
    public class UserRolesController : Controller
    {
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTRolesService _rolesService;
        private readonly int _companyId;

        public UserRolesController(IBTCompanyInfoService companyInfoService, IBTRolesService rolesService, IHttpContextAccessor contextAccessor)
        {
            _companyInfoService = companyInfoService;
            _rolesService = rolesService;

            if (contextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                _companyId = contextAccessor.HttpContext.User.Identity.GetCompanyId().Value;
            }
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            List<ManageUserRolesViewModel> model = new();
            List<BTUser> users = await _companyInfoService.GetAllMembersAsync(_companyId);
            foreach (BTUser user in users)
            {
                ManageUserRolesViewModel viewModel = new();
                viewModel.BTUser = user;
                var selectedRoles = await _rolesService.GetUserRolesAsync(user);
                viewModel.Roles = new MultiSelectList(await _rolesService.GetRolesAsync(), "Name", "Name", selectedRoles);
                model.Add(viewModel);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel member)
        {
            BTUser user = (await _companyInfoService.GetAllMembersAsync(_companyId)).FirstOrDefault(u => u.Id == member.BTUser.Id);
            IEnumerable<string> roles = await _rolesService.GetUserRolesAsync(user);
            await _rolesService.RemoveUserFromRolesAsync(user, roles);
            string userRole = member.SelectedRoles.FirstOrDefault();
            if (!string.IsNullOrEmpty(userRole))
            {
                await _rolesService.AddUserToRoleAsync(user, userRole);
            }
            return RedirectToAction("ManageUserRoles");
        }
    }
}
