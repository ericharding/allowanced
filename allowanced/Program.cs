using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ObjectPool;
using Scriban;

namespace allowanced
{
  class Program
  {
    public static IResult Html(string html)
    {
      return Results.Content(html, "text/html");
    }

    public static string HelloRoute2() => "hello world";

    public static async Task<IResult> nameRoute(HttpContext ctx, IFileProvider fileProvider)
    {
      var template = fileProvider.getTemplate("name.html");
      var result = await template.RenderAsync(new { name = "JOE" });
      Console.WriteLine(result);
      return Html(result);
    }

    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);
      builder.Services.ConfigureFileProvider();
      builder.Services.AddAuthentication("cookie").AddCookie("cookie");

      var app = builder.Build();
      app.MapGet("/", HelloRoute2);
      app.MapGet("/name", nameRoute);
      app.Run();
    }
  }
}
