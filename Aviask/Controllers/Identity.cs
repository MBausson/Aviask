using Aviask.Models.Input;
using Aviask.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Aviask.Controllers;

public class Identity : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IAviaskRepository<IdentityUser, string> _userRepository;

    public Identity(UserManager<IdentityUser> userManager, 
        SignInManager<IdentityUser> signInManager,
        IAviaskRepository<IdentityUser, string> userRepository)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userRepository = userRepository;
    }

    public async Task<IActionResult> Index()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToRoute("/");
        }

        var user = await _userManager.GetUserAsync(User);

        return View(User.Identity);
    }

    public async Task<IActionResult> Login()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index");
        }

        return View();
    }

    public async Task<IActionResult> Register()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index");
        }

        return View();
    }

    [HttpGet("Identity/Profile/{identifier}")]
    public async Task<IActionResult> Profile(string? identifier)
    {
        IdentityUser? user = null;

        //  If not identifier, is given, show the current user's profile if logged in.
        if (identifier == null)
        {
            if (User == null)
            {
                return Forbid();
            }

            return View(await _userManager.GetUserAsync(User));
        }

        //  Else, try to retrieve the account via its ID first, then username if not found.
        user = await _userRepository.GetByIdAsync(identifier);

        //  If the get by ID fails, try with username
        if (user == null)
        {
            user = await ((UserRepository)_userRepository).GetByUsername(identifier);
        }

        //  If we really didn't find nothing, 404
        if (user is null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Login([Bind("Email,Password,RememberMe")] LoginInputModel input)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        var signedUser = await _userManager.FindByEmailAsync(input.Email);

        if (signedUser == null)
        {
            ModelState.AddModelError(string.Empty, "The e-mail provided is unknown.");
            return View(input);
        }

        var result = await _userManager.CheckPasswordAsync(signedUser, input.Password);
        
        //  If the user successfully logs on, redirect him to the dashboard page, aka Index '/'
        if (result)
        {
            await _signInManager.SignInAsync(signedUser, isPersistent: input.RememberMe);
            return Redirect("/");
        }

        //  Here, the user failed to log in, show the error message
        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(input);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterInputModel input)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        var user = new IdentityUser
        {
            Email = input.Email,
            UserName = input.UserName,
        };

        var result = await _userManager.CreateAsync(user, input.Password);
        
        //  If the user has successfully reistered, add a default role and redirect to his dashboard
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "user");

            await _signInManager.SignInAsync(user, isPersistent: false);
            return Redirect("/");
        }

        //  If the user cannot register, show the errors
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(input);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();

        if (returnUrl != null)
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}
