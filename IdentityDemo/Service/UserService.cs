using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using IdentityDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityDemo.Service
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(string id);
        Task<IdentityResult> CreateAsync(User user, ClaimsPrincipal currentUser);
        Task<IdentityResult> SendMailAsync(string id);
        Task<IdentityResult> UpdateUserAsync(User user, ClaimsPrincipal currentUser);
        Task<IdentityResult> SoftDeleteUserAsync(string id, ClaimsPrincipal currentUser);
        Task SendPasswordSetEmailAsync(User user);
        bool UserExists(string id);
    }
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMailService _mailService;

        public UserService(UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<User> userStore,
            ILogger<UserService> logger,
            IConfiguration configuration,
            IMailService mailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _configuration = configuration;
            _mailService = mailService;
        }


        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)_userStore;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {

            return await _userManager.Users.Where(u => u.DeletedBy == null).ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id && u.DeletedBy == null);
        }

        public async Task<IdentityResult> CreateAsync(User user, ClaimsPrincipal currentUser)
        {
            user.CreatedBy = _userManager.GetUserId(currentUser) ?? "System";
            user.CreatedAt = DateTime.UtcNow;

            await _userStore.SetUserNameAsync(user, user.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, user.Email, CancellationToken.None);

            var result = await _userManager.CreateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                var checkRole = await _roleManager.FindByIdAsync("User");

                if (checkRole == null)
                {
                    //Afer user created, create the role
                    var role = new IdentityRole("User");
                    await _roleManager.CreateAsync(role);
                }

                await _userManager.AddToRoleAsync(user, "User");

                //await SendEmailConfermationAsync(user);

            }

            return result;
        }

        public async Task<IdentityResult> SendMailAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }
            await SendPasswordSetEmailAsync(user);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateUserAsync(User user, ClaimsPrincipal currentUser)
        {
            var existingUser = await _userManager.FindByIdAsync(user.Id);
            if (existingUser == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.UpdatedBy = _userManager.GetUserId(currentUser);
            existingUser.UpdatedAt = DateTime.UtcNow;

            await _userStore.SetUserNameAsync(existingUser, existingUser.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(existingUser, existingUser.Email, CancellationToken.None);

            return await _userManager.UpdateAsync(existingUser);
        }

        public async Task<IdentityResult> SoftDeleteUserAsync(string id, ClaimsPrincipal currentUser)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.DeletedBy = _userManager.GetUserId(currentUser);
                user.DeletedAt = DateTime.UtcNow;
                user.UserName = user.UserName + "_deleted_" + DateTime.Now.Ticks;
                user.Email = user.Email + "_deleted_" + DateTime.Now.Ticks;
                return await _userManager.UpdateAsync(user);
            }

            return IdentityResult.Failed(new IdentityError { Description = "User not found" });
        }

        public bool UserExists(string id)
        {
            return _userManager.Users.Any(u => u.Id == id && u.DeletedBy == null);
        }

        public async Task SendPasswordSetEmailAsync(User user)
        {
            string baseurl = _configuration.GetSection("ApplicationInfo").GetSection("BaseUrl").Value!;

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedEmail = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.Email));
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = $"{baseurl}/Identity/Account/ResetPassword?code={code}&e={encodedEmail}&s={true}";

            var userName = user.FirstName + " " + user.LastName;

            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "PasswordResetEmailTemplate.html");
            string emailBody = await File.ReadAllTextAsync(templatePath);

            emailBody = emailBody.Replace("{{callbackUrl}}", HtmlEncoder.Default.Encode(callbackUrl));
            emailBody = emailBody.Replace("{{userName}}", userName);

            await _mailService.SendEmailAsync(user.Email,"Set Your Password", emailBody);
        }

        //public async Task SendEmailConfermationAsync(User user)
        //{
            
        //    string baseurl = _configuration.GetSection("ApplicationInfo").GetSection("BaseUrl").Value!;

        //    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
           
        //    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        //    var callbackUrl = $"{baseurl}/Identity/Account/ConfirmEmail?userId={user.Id}&code={code}";
            

        //    await _mailService.SendEmailAsync(user.Email, "Confirm your email",
        //                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
        //}
    }
}
