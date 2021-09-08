using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using RemyNewWebApp.Models;
using RemyNewWebApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RemyNewWebApp.Services.Factories
{
    public class BTUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<BTUser, IdentityRole>
    {
        private readonly IBTCompanyInfoService _companyInfoService;

        public BTUserClaimsPrincipalFactory(UserManager<BTUser> userManager,
                                            RoleManager<IdentityRole> roleManager,
                                            IOptions<IdentityOptions> optionsAccessor,
                                            IBTCompanyInfoService companyInfoService) 
        : base(userManager, roleManager, optionsAccessor)
        {
            _companyInfoService = companyInfoService;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(BTUser user)
        {
            ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("CompanyId", user.CompanyId.ToString()));
            Company company = await _companyInfoService.GetCompanyInfoByIdAsync(user.CompanyId);
            identity.AddClaim(new Claim("CompanyName", company.Name));

            return identity;
        }
    }
}
