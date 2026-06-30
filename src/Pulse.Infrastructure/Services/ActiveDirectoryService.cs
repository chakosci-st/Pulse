using Pulse.Core.Entities;
using Pulse.Core.Enums;
using Pulse.Core.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Task = System.Threading.Tasks.Task;
using Serilog;
namespace Pulse.Infrastructure.Services
{
    public class ActiveDirectoryService : IActiveDirectoryService
    {
        private readonly string _domain;
        private readonly string _domainContainer;
        private readonly string _domainWeb;
        private readonly string _ldapServer;
        private readonly string _ldapContainer;
        private readonly string _adGroupExtEmail;

        public ActiveDirectoryService(
            string domain,
            string domainContainer,
            string domainWeb,
            string ldapServer,
            string ldapContainer,
            string adGroupExtEmail)
        {
            _domain = domain ?? throw new ArgumentNullException(nameof(domain));
            _domainContainer = domainContainer ?? throw new ArgumentNullException(nameof(domainContainer));
            _domainWeb = domainWeb ?? throw new ArgumentNullException(nameof(domainWeb));
            _ldapServer = ldapServer ?? throw new ArgumentNullException(nameof(ldapServer));
            _ldapContainer = ldapContainer ?? throw new ArgumentNullException(nameof(ldapContainer));
            _adGroupExtEmail = adGroupExtEmail ?? throw new ArgumentNullException(nameof(adGroupExtEmail));
        }

        /// <summary>
        /// Searches for users in Active Directory based on a search term.
        /// </summary>
        /// <param name="searchTerm">The search term (e.g., username, email).</param>
        /// <returns>A collection of ADUser objects.</returns>
        public ICollection<ActiveDirectoryUser> SearchUsers(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                Log.Warning("Search term is null or empty.");
                return new Collection<ActiveDirectoryUser>();
            }

            try
            {
                using (var searcher = CreateDirectorySearcher(_ldapServer, _ldapContainer))
                {
                    searcher.Filter = $"(|(cn={searchTerm.ToLower()}*)(sn={searchTerm.ToLower()}*)(mail={searchTerm.ToLower()}*)(uid={searchTerm.ToLower()}*))";
                    searcher.PropertiesToLoad.AddRange(GetDefaultResultFields());

                    var results = searcher.FindAll();
                    Log.Information("SearchUsers: Found {Count} users for search term '{SearchTerm}'.", results.Count, searchTerm);
                    return MapSearchResultsToAdUsers(results);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while searching for users with search term '{SearchTerm}'.", searchTerm);
                return new Collection<ActiveDirectoryUser>();
            }
        }

        /// <summary>
        /// Finds a user in Active Directory by a specific key and key type.
        /// </summary>
        /// <param name="key">The key to search for (e.g., username, email).</param>
        /// <param name="type">The type of key (e.g., Username, Email, EmployeeId).</param>
        /// <returns>An ADUser object if found; otherwise, null.</returns>
        public ActiveDirectoryUser FindUser(string key, ActiveDirectoryKeyType type)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Log.Warning("Key is null or empty.");
                return null;
            }

            try
            {
                using (var searcher = CreateDirectorySearcher(_ldapServer, _ldapContainer))
                {
                    string filter;
                    switch (type)
                    {
                        case ActiveDirectoryKeyType.Username:
                            filter = $"(uid={key.ToLower()})";
                            break;
                        case ActiveDirectoryKeyType.Email:
                            filter = $"(mail={key.ToLower()})";
                            break;
                        case ActiveDirectoryKeyType.EmployeeId:
                            filter = $"(employeeNumber={key})";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), "Invalid ADKeyType.");
                    }

                    searcher.Filter = filter;

                    searcher.PropertiesToLoad.AddRange(GetDefaultResultFields());
                    var result = searcher.FindOne();
                    if (result != null)
                    {
                        Log.Information("FindUser: User found for key '{Key}' and type '{Type}'.", key, type);
                        return MapSearchResultToAdUser(result);
                    }
                    else
                    {
                        Log.Warning("FindUser: No user found for key '{Key}' and type '{Type}'.", key, type);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while finding user with key '{Key}' and type '{Type}'.", key, type);
                return null;
            }
        }

        /// <summary>
        /// Searches for users in Active Directory with full details based on a search term.
        /// </summary>
        /// <param name="searchTerm">The search term (e.g., username, email).</param>
        /// <returns>A collection of ActiveDirectoryUser objects.</returns>
        public ICollection<ActiveDirectoryUser> SearchUsersFull(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                //_logger.LogWarning("Search term is null or empty.");
                return new Collection<ActiveDirectoryUser>();
            }

            try
            {
                using (var searcher = CreateDirectorySearcher(_ldapServer, _ldapContainer, GetDefaultResultFields()))
                {
                    searcher.Filter = $"(|(cn={searchTerm.ToLower()}*)(sn={searchTerm.ToLower()}*)(mail={searchTerm.ToLower()}*)(uid={searchTerm.ToLower()}*)(employeeNumber={searchTerm}))";

                    var results = searcher.FindAll();
                    return MapSearchResultsToAdUsers(results);
                }
            }
            catch (Exception ex)
            {
               // _logger.LogError(ex, "Error occurred while searching for users.");
                return new Collection<ActiveDirectoryUser>();
            }
        }

        /// <summary>
        /// Retrieves all Active Directory groups associated with a username.
        /// </summary>
        /// <param name="username">The username to search for.</param>
        /// <returns>A pipe-separated string of group names.</returns>
        public string SearchActiveDirectoryGroupsPerUserName(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                //_logger.LogWarning("Username is null or empty.");
                return string.Empty;
            }

            try
            {
                using (var root = new DirectoryEntry($"LDAP://{_ldapServer}/ou=Groups,dc=st,dc=com"))
                using (var searcher = new DirectorySearcher(root))
                {
                    searcher.Filter = $"(uniqueMember=st-eduid={GetAttributePerUser(username, LDAPParameters.st_eduid)},ou=people,dc=st,dc=com*)";
                    searcher.PropertiesToLoad.Add("cn");

                    var results = searcher.FindAll();
                    var groupsList = new StringBuilder();

                    foreach (SearchResult result in results)
                    {
                        groupsList.Append(result.Properties["cn"][0] + "|");
                    }

                    if (groupsList.Length > 0)
                        groupsList.Length -= 1; // Remove the trailing pipe

                    return groupsList.ToString();
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error occurred while searching for groups for username {Username}.", username);
                return string.Empty;
            }
        }

        /// <summary>
        /// Retrieves a specific LDAP attribute for a user.
        /// </summary>
        /// <param name="key">The key to search for (e.g., username, employee ID).</param>
        /// <param name="ldapParameter">The LDAP parameter to retrieve.</param>
        /// <returns>The value of the LDAP attribute.</returns>
        public object GetAttributePerUser(string key, LDAPParameters ldapParameter)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                //_logger.LogWarning("Key is null or empty.");
                return string.Empty;
            }

            try
            {
                using (var searcher = CreateDirectorySearcher(_ldapServer, "ou=People,dc=st,dc=com", new[] { ldapParameter.ToString().Replace("_", "-") }))
                {
                    searcher.Filter = $"(|(uid={key})(st-eduid={key}))";

                    var result = searcher.FindOne();
                    if (result != null && result.Properties.Contains(ldapParameter.ToString().Replace("_", "-")))
                    {
                        return result.Properties[ldapParameter.ToString().Replace("_", "-")][0]?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error occurred while retrieving attribute {LdapParameter} for key {Key}.", ldapParameter, key);
            }

            return string.Empty;
        }

        /// <summary>
        /// Finds a user in Active Directory based on a search term.
        /// </summary>
        /// <param name="searchTerm">The search term (e.g., username, email).</param>
        /// <returns>An ActiveDirectoryUser object if found; otherwise, null.</returns>
        public ActiveDirectoryUser FindUser(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
              //  _logger.LogWarning("Search term is null or empty.");
                return null;
            }

            try
            {
                using (var searcher = CreateDirectorySearcher(_ldapServer, _ldapContainer, GetDefaultResultFields()))
                {
                    searcher.Filter = $"(|(uid={searchTerm.ToLower()})(cn={searchTerm.ToLower()})(mail={searchTerm.ToLower()})(employeeNumber={searchTerm}))";

                    var result = searcher.FindOne();
                    return result != null ? MapSearchResultToAdUser(result) : null;
                }
            }
            catch (Exception ex)
            {
              //  _logger.LogError(ex, "Error occurred while finding user with search term {SearchTerm}.", searchTerm);
                return null;
            }
        }

        /// <summary>
        /// Asynchronously finds a user in Active Directory based on a search term.
        /// </summary>
        /// <param name="searchTerm">The search term (e.g., username, email).</param>
        /// <returns>A Task representing the asynchronous operation, with an ActiveDirectoryUser object as the result.</returns>
        public async Task<ActiveDirectoryUser> FindUserAsync(string searchTerm)
        {
            return await Task.Run(() => FindUser(searchTerm));
        }

        /// <summary>
        /// Searches for users in Active Directory based on username or employee ID.
        /// </summary>
        /// <param name="username">The username to search for.</param>
        /// <param name="employeeId">The employee ID to search for.</param>
        /// <returns>A list of UserPrincipal objects.</returns>
        public List<UserPrincipal> SearchUsers(string username = "", string employeeId = "")
        {
            var users = new List<UserPrincipal>();
            var uniqueUsers = new HashSet<string>();

            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _domain, _domainContainer))
                {
                    if (!string.IsNullOrEmpty(username))
                    {
                        var userPrincipal = new UserPrincipal(context)
                        {
                            SamAccountName = $"*{username}*"
                        };
                        users.AddRange(SearchByPrincipal(userPrincipal, uniqueUsers));
                    }

                    if (!string.IsNullOrEmpty(employeeId))
                    {
                        var userPrincipal = new UserPrincipal(context)
                        {
                            EmployeeId = employeeId
                        };
                        users.AddRange(SearchByPrincipal(userPrincipal, uniqueUsers));
                    }
                }
            }
            catch (Exception ex)
            {
               // _logger.LogError(ex, "Error occurred while searching for users with username {Username} or employee ID {EmployeeId}.", username, employeeId);
            }

            return users;
        }

        /// <summary>
        /// Searches for an Active Directory group and its members.
        /// </summary>
        /// <param name="activeDirectoryGroup">The name of the Active Directory group.</param>
        /// <returns>An ActiveDirectoryGroup object if found; otherwise, an empty object.</returns>
        public ActiveDirectoryGroup SearchActiveDirectoryGroup(string activeDirectoryGroup)
        {
            if (string.IsNullOrWhiteSpace(activeDirectoryGroup))
            {
               // _logger.LogWarning("Active Directory group name is null or empty.");
                return new ActiveDirectoryGroup();
            }

            try
            {
                using (var root = new DirectoryEntry($"LDAP://{_ldapServer}/ou=Groups,{_domainContainer}"))
                using (var searcher = new DirectorySearcher(root))
                {
                    searcher.Filter = $"(cn={activeDirectoryGroup})";
                    searcher.PropertiesToLoad.Add("uniqueMember");

                    var result = searcher.FindOne();
                    if (result != null)
                    {
                        var users = new List<ActiveDirectoryUser>();
                        foreach (var member in result.Properties["uniqueMember"])
                        {
                            var key = member.ToString().Split(',')[0].Split('=')[1];
                            users.Add(FindUserByEDUID(key));
                        }

                        return new ActiveDirectoryGroup
                        {
                            ADGroup = activeDirectoryGroup,
                            Email = $"{activeDirectoryGroup}@{_adGroupExtEmail}",
                            Members = users.Select(u => new ActiveDirectoryGroupMember {
                                 ADGroupName = activeDirectoryGroup,
                                 UserId = u.EmployeeId,
                                 Email = u.Email,
                                 FirstName = u.FirstName,
                                 LastName = u.LastName,
                                 UserName = u.Username 
                            }).ToList()
                            //UserNames = string.Join(", ", users.Select(u => u.Username)),
                            //Emails = string.Join(", ", users.Select(u => u.Email)),
                            //Users = users
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error occurred while searching for Active Directory group {ActiveDirectoryGroup}.", activeDirectoryGroup);
            }

            return new ActiveDirectoryGroup();
        }

        /// <summary>
        /// Finds a user in Active Directory by their EDUID.
        /// </summary>
        /// <param name="key">The EDUID of the user.</param>
        /// <returns>An ActiveDirectoryUser object if found; otherwise, null.</returns>
        public ActiveDirectoryUser FindUserByEDUID(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                //_logger.LogWarning("EDUID key is null or empty.");
                return null;
            }

            try
            {
                using (var searcher = CreateDirectorySearcher(_ldapServer, $"ou=People,{_domainContainer}", new[] { "uid", "sn", "cn", "mail", "employeeNumber", "st-eduid" }))
                {
                    searcher.Filter = $"(st-eduid={key})";

                    var result = searcher.FindOne();
                    return result != null ? MapSearchResultToAdUser(result) : null;
                }
            }
            catch (Exception ex)
            {
               // _logger.LogError(ex, "Error occurred while finding user by EDUID {Key}.", key);
                return null;
            }
        }





        /// <summary>
        /// Retrieves the profile photo of a user from Active Directory.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <returns>The profile photo as a byte array, or a default image if not found.</returns>
        public byte[] GetProfilePhoto(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                //_logger.LogWarning("Username is null or empty.");
                return LoadDefaultProfilePhoto();
            }

            try
            {
                using (var principalContext = new PrincipalContext(ContextType.Domain, _domain, _domainContainer))
                using (var userPrincipal = new UserPrincipal(principalContext) { SamAccountName = username })
                using (var principalSearcher = new PrincipalSearcher(userPrincipal))
                {
                    var principal = principalSearcher.FindOne();
                    if (principal is UserPrincipal user)
                    {
                        var directoryEntry = (DirectoryEntry)user.GetUnderlyingObject();
                        var photoProperty = directoryEntry.Properties["thumbnailPhoto"];

                        if (photoProperty?.Value is byte[] photoBytes)
                        {
                            return photoBytes;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
              // _logger.LogError(ex, "Error occurred while retrieving profile photo for user {Username}.", username);
            }

            return LoadDefaultProfilePhoto();
        }

        /// <summary>
        /// Updates the profile picture of the current logged-in user in Active Directory.
        /// </summary>
        /// <param name="imageBytes">The image bytes to update.</param>
        public void UpdateUserProfilePicture(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
            {
              //  _logger.LogWarning("Image bytes are null or empty.");
                return;
            }

            try
            {
                // Get the current logged-in user's username
                string currentUsername = WindowsIdentity.GetCurrent().Name.Split('\\')[1];

                // Set the LDAP path to the user's AD object
                string ldapPath = $"LDAP://{_ldapServer}/CN={currentUsername},OU=Users,DC=st,DC=com";

                using (var userEntry = CreateDirectoryEntry(ldapPath))
                {
                    if (userEntry != null)
                    {
                        // Update the thumbnailPhoto attribute with the image bytes
                        userEntry.Properties["thumbnailPhoto"].Value = imageBytes;
                        userEntry.CommitChanges();
                      //  _logger.LogInformation("Profile picture updated successfully for user {Username}.", currentUsername);
                    }
                    else
                    {
                      //  _logger.LogWarning("User {Username} not found in Active Directory.", currentUsername);
                    }
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error occurred while updating profile picture.");
            }
        }

        /// <summary>
        /// Maps a list of UserPrincipal objects to ActiveDirectoryUser models.
        /// </summary>
        /// <param name="users">The list of UserPrincipal objects.</param>
        /// <returns>A list of ActiveDirectoryUser models.</returns>
        private List<ActiveDirectoryUser> MapToActiveDirectoryUserModel(List<UserPrincipal> users)
        {
            return users.Select(user => new ActiveDirectoryUser
            {
                Username = user.SamAccountName,
                DisplayName = user.DisplayName,
                Email = user.EmailAddress,
                EmployeeId = user.EmployeeId
            }).ToList();
        }

        /// <summary>
        /// Searches for users in Active Directory based on a UserPrincipal filter.
        /// </summary>
        /// <param name="userPrincipal">The UserPrincipal filter.</param>
        /// <param name="uniqueUsers">A set to ensure unique results.</param>
        /// <returns>A list of UserPrincipal objects.</returns>
        private List<UserPrincipal> SearchByPrincipal(UserPrincipal userPrincipal, HashSet<string> uniqueUsers)
        {
            var results = new List<UserPrincipal>();

            try
            {
                using (var searcher = new PrincipalSearcher(userPrincipal))
                {
                    foreach (var result in searcher.FindAll())
                    {
                        if (result is UserPrincipal user && uniqueUsers.Add(user.SamAccountName))
                        {
                            results.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
              //  _logger.LogError(ex, "Error occurred while searching by principal.");
            }

            return results;
        }

        /// <summary>
        /// Loads a default profile photo from a predefined path.
        /// </summary>
        /// <returns>The default profile photo as a byte array.</returns>
        private byte[] LoadDefaultProfilePhoto()
        {
            try
            {
                string defaultPhotoPath = HttpContext.Current.Server.MapPath("/images/shared/profile-generic-user.png");
                return File.ReadAllBytes(defaultPhotoPath);
            }
            catch (Exception ex)
            {
              //  _logger.LogError(ex, "Error occurred while loading the default profile photo.");
                return Array.Empty<byte>();
            }
        }

        /// <summary>
        /// Creates a DirectoryEntry object for the specified LDAP path.
        /// </summary>
        /// <param name="ldapPath">The LDAP path.</param>
        /// <returns>A DirectoryEntry object.</returns>
        private DirectoryEntry CreateDirectoryEntry(string ldapPath)
        {
            try
            {
                return new DirectoryEntry(ldapPath, null, null, AuthenticationTypes.Secure);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error occurred while creating DirectoryEntry for path {LdapPath}.", ldapPath);
                return null;
            }
        }

        // Helper Methods

        private DirectorySearcher CreateDirectorySearcher(string ldapServer, string ldapContainer)
        {
            return new DirectorySearcher(new DirectoryEntry($"LDAP://{ldapServer}/{ldapContainer}", null, null, AuthenticationTypes.Secure));
        }
        private DirectorySearcher CreateDirectorySearcher(string ldapServer, string ldapContainer, string[] propertiesToLoad)
        {
            var searcher = new DirectorySearcher(new DirectoryEntry($"LDAP://{ldapServer}/{ldapContainer}", null, null, AuthenticationTypes.Secure));
            searcher.PropertiesToLoad.AddRange(propertiesToLoad);
            return searcher;
        }
        private string[] GetDefaultResultFields()
        {
            return new[]
            {
                "givenname", "sn", "employeenumber", "samaccountname", "mail", "cn",
                "department", "division", "employeeid", "thumbnailphoto", "manager"
            };
        }

        private ICollection<ActiveDirectoryUser> MapSearchResultsToAdUsers(SearchResultCollection results)
        {
            var users = new Collection<ActiveDirectoryUser>();
            foreach (SearchResult result in results)
            {
                users.Add(MapSearchResultToAdUser(result));
            }
            return users;
        }

        private ActiveDirectoryUser MapSearchResultToAdUser(SearchResult result)
        {
            return new ActiveDirectoryUser
            {
                FirstName = GetProperty(result, "givenname"),
                LastName = GetProperty(result, "sn"),
                EmployeeId = GetProperty(result, "employeenumber"),
                Username = GetProperty(result, "samaccountname"),
                Email = GetProperty(result, "mail"),
                DisplayName = GetProperty(result, "cn"),
                Department = GetProperty(result, "department"),
                Division = GetProperty(result, "division"),
                ManagerUsername = ParseManagerUsername(GetProperty(result, "manager"))
            };
        }

        private string GetProperty(SearchResult result, string propertyName)
        {
            return result.Properties.Contains(propertyName) ? result.Properties[propertyName][0]?.ToString() : string.Empty;
        }

        private string ParseManagerUsername(string managerDn)
        {
            if (string.IsNullOrWhiteSpace(managerDn)) return string.Empty;

            var components = managerDn.Split(',');
            foreach (var component in components)
            {
                if (component.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
                {
                    return component.Substring(3);
                }
            }
            return string.Empty;
        }
    }
}
