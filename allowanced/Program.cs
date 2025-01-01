using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ObjectPool;
using Scriban;

namespace allowanced
{
  class Program
  {
    public static IResult styleRoute(IFileProvider fileProvider) =>
      Results.Content(fileProvider.getFileContents("styles.css"), "text/css");

    public static Task<IResult> nameRoute(HttpContext ctx, IFileProvider fileProvider)
    {
      return Response.htmlTemplate(fileProvider, "name.html", new { name = "JOE" });
    }

    public static IResult loginRoute(IFileProvider fileProvider) =>
      Response.htmlFileContent(fileProvider, "login.html");

    static bool checkAuth(string? username, string? password)
    {
      return username == "admin" && password == "1234";
    }

    public static async Task loginHandler(HttpContext ctx)
    {
      var formData = await ctx.Request.ReadFormAsync();
      var username = formData["username"];
      var password = formData["password"];
      if (checkAuth(username, password))
      {
        var claims = new List<Claim> { new Claim("user", username!) };
        var identity = new ClaimsIdentity(claims, "cookie");
        var user = new ClaimsPrincipal(identity);
        await ctx.SignInAsync("cookie", user);
        Console.WriteLine($"Logged in {username} / {password}");
        ctx.Response.Redirect("/");
        return;
      } else {
        Console.WriteLine($"Invalid password for {username}");
        ctx.Response.Redirect("/login?error=1");
      }
    }

    public static async Task logoutHandler(HttpContext ctx)
    {
      await ctx.SignOutAsync();
      ctx.Response.Redirect("/");
    }

    public static IResult dashboardRoute(IFileProvider fileProvider) =>
      Response.htmlFileContent(fileProvider, "home.html");

    public static IResult indexRoute(HttpContext ctx, IFileProvider fileProvider)
    {
      var isAuthenticated = ctx.User.Identity?.IsAuthenticated;
      if (isAuthenticated == true)
      {
        return dashboardRoute(fileProvider);
      }
      else
      {
        return loginRoute(fileProvider);
      }
    }

    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);
      builder.Services.ConfigureFileProvider();
      builder.Services.AddAuthentication("cookie").AddCookie("cookie");

      var app = builder.Build();
      app.MapGet("/", indexRoute);
      app.MapGet("/styles.css", styleRoute);
      app.MapGet("/name", nameRoute);
      app.MapGet("/login", loginRoute);

      app.MapPost("/login", loginHandler);
      app.MapGet("/logout", logoutHandler);
      app.Run();
    }
  }
}
