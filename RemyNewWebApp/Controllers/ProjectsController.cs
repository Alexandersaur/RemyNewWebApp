using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RemyNewWebApp.Data;
using RemyNewWebApp.Extensions;
using RemyNewWebApp.Models;
using RemyNewWebApp.Models.Enums;
using RemyNewWebApp.Models.ViewModels;
using RemyNewWebApp.Services.Interfaces;

namespace RemyNewWebApp.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTFileService _fileService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTLookupService _lookupService;

        public ProjectsController(ApplicationDbContext context,
                                  IBTCompanyInfoService companyInfoService,
                                  IBTRolesService rolesService,
                                  IBTFileService fileService,
                                  UserManager<BTUser> userManager,
                                  IBTProjectService projectService,
                                  IBTLookupService lookupService)
        {
            _context = context;
            _companyInfoService = companyInfoService;
            _rolesService = rolesService;
            _fileService = fileService;
            _userManager = userManager;
            _projectService = projectService;
            _lookupService = lookupService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Projects
                                               .Include(p => p.Company)
                                               .Include(p => p.ProjectPriority);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: MY Projects
        public async Task<IActionResult> MyProjects()
        {
            List<ProjectViewModel> model = new();
            string userId = _userManager.GetUserId(User);
            List<Project> projects = await _projectService.GetUserProjectsAsync(userId);
            foreach (Project project in projects)
            {
                ProjectViewModel viewModel = new();
                viewModel.Project = project;
                viewModel.ProjectManager = await _projectService.GetProjectManagerAsync(project.Id);
                model.Add(viewModel);
            }
            return View(model);
        }

        // GET: ALL Projects
        public async Task<IActionResult> AllProjects()
        {
            List<ProjectViewModel> model = new();
            int companyId = User.Identity.GetCompanyId().Value;

            if (User.IsInRole(nameof(Roles.Admin)) || User.IsInRole(nameof(Roles.ProjectManager)))
            {
                List<Project> projects = await _companyInfoService.GetAllProjectsAsync(companyId);
                foreach (Project project in projects)
                {
                    ProjectViewModel viewModel = new();
                    viewModel.Project = project;
                    viewModel.ProjectManager = await _projectService.GetProjectManagerAsync(project.Id);
                    model.Add(viewModel);
                }
            }
            else
            {
                List<Project> projects = await _projectService.GetAllProjectsByCompany(companyId);
                foreach (Project project in projects)
                {
                    ProjectViewModel viewModel = new();
                    viewModel.Project = project;
                    viewModel.ProjectManager = await _projectService.GetProjectManagerAsync(project.Id);
                    model.Add(viewModel);
                }
            }
            return View(model);
        }

        // GET: Archived Projects
        public async Task<IActionResult> ArchivedProjects()
        {
            List<ProjectViewModel> model = new();
            int companyId = User.Identity.GetCompanyId().Value;
            List<Project> projects = await _projectService.GetArchivedProjectsByCompany(companyId);
            foreach (Project project in projects)
            {
                ProjectViewModel viewModel = new();
                viewModel.Project = project;
                viewModel.ProjectManager = await _projectService.GetProjectManagerAsync(project.Id);
                model.Add(viewModel);
            }
            return View(model);
        }

        // GET: Assign PM index
        public async Task<IActionResult> AssignPMIndex()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            List<Project> projects = await _projectService.GetUnassignedProjectsAsync(companyId);
            return View(projects);
        }

        [HttpGet]
        public async Task<IActionResult> AssignPM(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            //create new Viewomdel for Project and list of Project Managers
            AssignPMViewModel model = new();
            model.Project = await _projectService.GetProjectByIdAsync(id, companyId);
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPM(AssignPMViewModel model)
        {
            //Set parameter for AddProjectManagerViewModel
            if (!string.IsNullOrEmpty(model.PMId))
            {
                await _projectService.AddProjectManagerAsync(model.PMId, model.Project.Id);
                return RedirectToAction("Details", "Projects", new { id = model.Project.Id });
            }
            return RedirectToAction(nameof(AssignPM), new { projectId = model.Project.Id });
        }


        [HttpGet]
        public async Task<IActionResult> AssignMembers(int id)
        {
            ProjectMembersViewModel model = new();
            int companyId = User.Identity.GetCompanyId().Value;
            // All company projects based on "id" parameter
            List<Project> projects = await _projectService.GetAllProjectsByCompany(companyId);
            Project project = projects.FirstOrDefault(p => p.Id == id);
            model.Project = project;
            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(Roles.Developer.ToString(), companyId);
            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(Roles.Submitter.ToString(), companyId);
            List<BTUser> users = developers.Concat(submitters).ToList();
            List<string> members = project.Members.Select(m => m.Id).ToList();
            model.Users = new MultiSelectList(users, "Id", "FullName", members);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMembers(ProjectMembersViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SelectedUsers != null)
                {
                    List<string> memberIds = (await _projectService.GetAllProjectMembersExceptPMAsync(model.Project.Id))
                                                                   .Select(m => m.Id).ToList();
                    foreach (string item in memberIds)
                    {
                        await _projectService.RemoveUserFromProjectAsync(item, model.Project.Id);
                    }
                    foreach (string item in model.SelectedUsers)
                    {
                        await _projectService.AddUserToProjectAsync(item, model.Project.Id);
                    }
                    return RedirectToAction("Details", "Projects", new { id = model.Project.Id });
                }
            }
            return RedirectToAction("AssignMembers", new { id = model.Project.Id });
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            int companyId = User.Identity.GetCompanyId().Value;
            Project project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            //Add Viewmodel instance "AddProjectWithPMViewModel"
            AddProjectWithPMViewModel model = new();
            //Load SelectLists with data ie. PMList & PriorityList
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName");
            model.PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name");
            //Return View with viewmodel instance as the model
            return View(model);
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddProjectWithPMViewModel model)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            //Test if model is null (aka if data has been captured from the form)
            if (model != null)
            {
                try
                {
                    //Capture the image if one has been selected
                    if (model.Project.ImageFormFile != null)
                    {
                        model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                        model.Project.ImageFileContentType = model.Project.ImageFormFile.ContentType;
                    }
                    //Set companyId of the new project
                    model.Project.CompanyId = companyId;
                    //Use service to add new project to database
                    await _projectService.AddNewProjectAsync(model.Project);
                    //Then if a PM was selected in the form
                    if (!string.IsNullOrEmpty(model.PMId))
                    {
                        //Add the PM to the project with service call
                        await _projectService.AddProjectManagerAsync(model.PMId, model.Project.Id);
                    }
                    return RedirectToAction("AllProjects");
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return RedirectToAction("Create");
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            {
                int companyId = User.Identity.GetCompanyId().Value;
                AddProjectWithPMViewModel model = new();
                model.Project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
                model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName");
                model.PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name");
                return View(model);
            }
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AddProjectWithPMViewModel model)
        {
            if (model != null)
            {
                try
                {
                    if (model.Project.ImageFormFile != null)
                    {
                        model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                        model.Project.ImageFileContentType = model.Project.ImageFormFile.ContentType;
                    }
                    await _projectService.UpdateProjectAsync(model.Project);
                    if (!string.IsNullOrEmpty(model.PMId))
                    {
                        await _projectService.AddProjectManagerAsync(model.PMId, model.Project.Id);
                    }
                    return RedirectToAction("Details", "Projects", new { id = model.Project.Id });
                }
                catch (Exception) 
                {
                    throw;
                }
            }

            return RedirectToAction("Edit");
        }

        // GET: Projects/UnassignedProjects
        public async Task<IActionResult> UnassignedProjects()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            List<Project> projects = new();
            projects = await _projectService.GetUnassignedProjectsAsync(companyId);
            return View(projects);
        }

        // GET: Projects/Archive/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            } 
            int companyId = User.Identity.GetCompanyId().Value;
            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            var project = await _projectService.GetProjectByIdAsync(id, companyId);
            await _projectService.ArchiveProjectAsync(project);
            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Restore/5
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            int companyId = User.Identity.GetCompanyId().Value;
            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            var project = await _projectService.GetProjectByIdAsync(id, companyId);
            await _projectService.RestoreProjectAsync(project);
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
