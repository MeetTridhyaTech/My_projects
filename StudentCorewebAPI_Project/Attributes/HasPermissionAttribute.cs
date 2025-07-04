//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Filters;
//using StudentCorewebAPI_Project.Repository_Interface;
//using System.Security.Claims;

//public class HasPermissionAttribute : Attribute, IAsyncAuthorizationFilter
//{
//    private readonly string _permission;

//    public HasPermissionAttribute(string permission)
//    {
//        _permission = permission;
//    }

//    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
//    {
//        //var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
//        var userIdClaim = context.HttpContext.User.FindFirst("UserId");


//        if (userIdClaim == null)
//        {
//            context.Result = new UnauthorizedResult();
//            return;
//        }

//        var permissionRepo = context.HttpContext.RequestServices.GetService<IPermissionRepository>();

//        if (permissionRepo == null || !await permissionRepo.HasPermissionAsync(Guid.Parse(userIdClaim.Value), _permission))
//        {
//            context.Result = new ForbidResult();
//        }
//    }
//}



using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StudentCorewebAPI_Project.Repository_Interface;
using System.Security.Claims;
using System.Text.Json;

public class HasPermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _permission;

    public HasPermissionAttribute(string permission)
    {
        _permission = permission;
    }

    //public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    //{
    //    var userIdClaim = context.HttpContext.User.FindFirst("UserId");

    //    if (userIdClaim == null)
    //    {
    //        context.Result = new UnauthorizedResult();
    //        return;
    //    }

    //    Guid menuId = Guid.Empty;
    //    bool isMenuIdRequired = true;

    //    if (context.RouteData.Values.TryGetValue("menuId", out var routeValue) &&
    //        Guid.TryParse(routeValue.ToString(), out var parsedRouteId))
    //    {
    //        menuId = parsedRouteId;
    //    }
    //    else if (context.HttpContext.Request.Query.TryGetValue("menuId", out var queryValue) &&
    //             Guid.TryParse(queryValue, out var parsedQueryId))
    //    {
    //        menuId = parsedQueryId;
    //    }
    //    else
    //    {
    //        // Optional: only require menuId if it's relevant
    //        isMenuIdRequired = false;
    //    }

    //    var permissionRepo = context.HttpContext.RequestServices.GetService<IPermissionRepository>();
    //    var userId = Guid.Parse(userIdClaim.Value);

    //    if (permissionRepo == null || !await permissionRepo.HasPermissionAsync(userId, menuId, _permission))
    //    {
    //        context.Result = new ForbidResult();
    //    }
    //}
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var userIdClaim = context.HttpContext.User.FindFirst("UserId");

        if (userIdClaim == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Try multiple ways to get menuId
        Guid menuId = Guid.Empty;

        // 1. From route values (for route parameters like "/api/menus/{menuId}")
        if (context.RouteData.Values.TryGetValue("menuId", out var menuIdValue))
        {
            Guid.TryParse(menuIdValue?.ToString(), out menuId);
        }

        // 2. From query string (for requests like "/api/menus?menuId=...")
        if (menuId == Guid.Empty && context.HttpContext.Request.Query.TryGetValue("menuId", out var queryMenuId))
        {
            Guid.TryParse(queryMenuId, out menuId);
        }

        // 3. From request body (for POST/PUT requests)
        if (menuId == Guid.Empty && context.HttpContext.Request.ContentLength > 0)
        {
            context.HttpContext.Request.EnableBuffering(); // Allows re-reading the body
            try
            {
                var body = await new StreamReader(context.HttpContext.Request.Body).ReadToEndAsync();
                context.HttpContext.Request.Body.Position = 0; // Rewind for actual action

                if (!string.IsNullOrEmpty(body))
                {
                    var json = JsonSerializer.Deserialize<JsonElement>(body);
                    if (json.TryGetProperty("menuId", out var menuIdProp))
                    {
                        Guid.TryParse(menuIdProp.GetString(), out menuId);
                    }
                }
            }
            catch { /* Ignore errors */ }
        }

        if (menuId == Guid.Empty)
        {
            context.Result = new BadRequestObjectResult("menuId is required.");
            return;
        }

        var permissionRepo = context.HttpContext.RequestServices.GetService<IPermissionRepository>();
        var userId = Guid.Parse(userIdClaim.Value);

        if (permissionRepo == null || !await permissionRepo.HasPermissionAsync(userId, menuId, _permission))
        {
            context.Result = new ForbidResult();
        }
    }
}
