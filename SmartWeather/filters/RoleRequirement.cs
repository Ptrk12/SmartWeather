using Microsoft.AspNetCore.Authorization;

namespace SmartWeather.filters
{
    public class RoleRequirement : IAuthorizationRequirement
    {
        public string[] RequirementRoles { get; }

        public RoleRequirement(params string[] requirementRoles)
        {
            RequirementRoles = requirementRoles;
        }
    }
}
