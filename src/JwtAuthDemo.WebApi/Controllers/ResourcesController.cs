using JwtAuthDemo.WebApi.Data.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthDemo.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ResourcesController : ControllerBase
{
    [Authorize]
    [HttpGet("authenticated-only")]
    public IActionResult AuthenticatedOnlyEndpoint()
    {
        return Ok("You are authenticated!");
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet("user-only")]
    public IActionResult UserOnlyEndpoint()
    {
        return Ok("You are a user!");
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet("admin-only")]
    public IActionResult AdminOnlyEndpoint()
    {
        return Ok("You are an admin!");
    }
}
