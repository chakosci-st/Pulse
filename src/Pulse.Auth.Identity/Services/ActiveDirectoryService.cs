using Pulse.Auth.Identity.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Auth.Identity.Services
{
    public class ActiveDirectoryService
    {

        private readonly string _domain;
        private readonly string _domaincontainer;
        private readonly string _domainweb;
        private readonly string _ldapserver;
        private readonly string _ldapcontainer;
        private readonly string _adgroupextemail;




        public ActiveDirectoryService(string domain, string domaincontainer, string domainweb, string ldapserver, string ldapcontainer, string adgroupextemail)
        {
            _domain = domain;
            _domainweb = domainweb;
            _domaincontainer = domaincontainer;
            _ldapserver = ldapserver;
            _ldapcontainer = ldapcontainer;
            _adgroupextemail = adgroupextemail;
        }


        public UserProfile GetUserProfile(string username)
        {
            using (var context = new PrincipalContext(ContextType.Domain))
            {
                var user = UserPrincipal.FindByIdentity(context, username);
                if (user == null) return null;

                // Example: You may need to use DirectoryEntry for custom attributes
                var directoryEntry = user.GetUnderlyingObject() as System.DirectoryServices.DirectoryEntry;

                return new UserProfile
                {
                    EmployeeId = directoryEntry.Properties["employeeID"]?.Value?.ToString(),
                    Email = user.EmailAddress,
                    FirstName = user.GivenName,
                    LastName = user.Surname,
                    Department = directoryEntry.Properties["department"]?.Value?.ToString(),
                    Division = directoryEntry.Properties["division"]?.Value?.ToString(),
                    ManagerUsername = directoryEntry.Properties["manager"]?.Value?.ToString(),
                    // ... map other properties as needed
                };
            }
        }

        public IDictionary<string, string> GetUserClaims(string key)
        {
            var claims = new Dictionary<string, string>();

            // Use your SearchUsersFull logic to find the user
            var users = SearchUsersFull(key);

            // Take the first user found (or handle as needed)
            var user = users.Count > 0 ? users[0] : null;
            if (user == null)
                return claims;
 
            // Map ADUser properties to CustomClaimTypes
            claims[CustomClaimTypes.EmployeeId] = (user.ContainsKey("employeenumber") ? user["employeenumber"].ToString() : "") ?? "";
            claims[CustomClaimTypes.Email] = (user.ContainsKey("mail") ? user["mail"].ToString() : "") ?? "";
            claims[CustomClaimTypes.FirstName] = (user.ContainsKey("givenname") ? user["givenname"].ToString() : "") ?? "";
            claims[CustomClaimTypes.LastName] = (user.ContainsKey("sn") ? user["sn"].ToString() : "") ?? "";
            claims[CustomClaimTypes.STJobFunctionDescription] = (user.ContainsKey("st-jobfunctiondescription") ? user["st-jobfunctiondescription"].ToString() : "") ?? "";
            claims[CustomClaimTypes.Department] = (user.ContainsKey("department") ? user["department"].ToString() : "") ?? "";
            claims[CustomClaimTypes.Division] = (user.ContainsKey("division") ? user["division"].ToString() : "") ?? "";
            claims[CustomClaimTypes.Photo] = (user.ContainsKey("thumbnailphoto") ? Convert.ToBase64String((byte[])user["thumbnailphoto"]) : "");
            claims[CustomClaimTypes.ManagerUsername] = (user.ContainsKey("manager") ? user["manager"].ToString() : "") ?? "";
            claims[CustomClaimTypes.CostCenter] = (user.ContainsKey("st-costcenter") ? user["st-costcenter"].ToString() : "") ?? "";
            claims[CustomClaimTypes.CostCenterDescription] = (user.ContainsKey("st-costcenterdescription") ? user["st-costcenterdescription"].ToString() : "") ?? "";
 

            return claims;
        }

        public IList<IDictionary<string, object>> SearchUsersFull(string searchTerm)
        {
            var oSearcher = new DirectorySearcher();
            SearchResultCollection oResults = null;

            var ResultFields = new string[]  {
                "givenname", "sn", "employeenumber", "samaccountname", "mail", "cn",
                "department", "division", "employeeid", "st-jobfunctiondescription",
                "thumbnailphoto", "st-costcenter", "st-costcenterdescription",
                "telephonenumber", "st-region", "st-regiondescription", "co", "l",
                "company", "manager", "uid"
            };

            IList<IDictionary<string, object>> usersList = new List<IDictionary<string, object>>();

            try
            {
                oSearcher.SearchRoot = new DirectoryEntry($"LDAP://{_domainweb}");
                oSearcher.PropertiesToLoad.AddRange(ResultFields);

                if (!string.IsNullOrEmpty(searchTerm))
                    oSearcher.Filter = $"(|(cn={searchTerm.ToLower()}*)(sn={searchTerm.ToLower()}*)(mail={searchTerm.ToLower()}*)(uid={searchTerm.ToLower()}*)(employeeNumber={searchTerm}))";
                else
                    oSearcher.Filter = "";

                oResults = oSearcher.FindAll();
                if (oResults.Count > 0)
                {
                    foreach (SearchResult result in oResults)
                    {
                        var userDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                        foreach (string property in result.Properties.PropertyNames)
                        {
                            var propertyValues = result.Properties[property];
                            // If only one value, store as single object, else as array
                            userDict["custom:" + property] = propertyValues.Count == 1 ? propertyValues[0] : propertyValues;
                        }

                        // Special handling for manager username extraction
                        if (userDict.ContainsKey("custom:manager") && userDict["custom:manager"] != null)
                        {
                            string managerDn = userDict["custom:manager"].ToString();
                            string managerUsername = "";
                            foreach (var component in managerDn.Split(','))
                            {
                                if (component.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
                                {
                                    managerUsername = component.Substring(3);
                                    break;
                                }
                            }
                            userDict["custom:managerusername"] = managerUsername;
                        }

                        usersList.Add(userDict);
                    }
                }
            }
            catch
            {
                // Handle/log exception as needed
            }

            return usersList;
        }
        private string GetProperty(SearchResult result, string propertyName)
        {
            if (result.Properties.Contains(propertyName) && result.Properties[propertyName].Count > 0)
                return result.Properties[propertyName][0].ToString();
            return null;
        }

 

        public IList<string> GetUserGroups(string username)
        {
            // Replace with actual AD group retrieval
            return new List<string> { "DomainUsers", "HR" };
        }
    }
}
