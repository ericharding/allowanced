namespace allowanced
{
  public static class Response
  {
    public static IResult ofHtmlString(string content)
    {
      return Results.Content(content, "text/html");
    }

    public static IResult htmlFileContent(IFileProvider fileProvider, string fileName)
    {
      return ofHtmlString(fileProvider.getFileContents(fileName));
    }

    public static async Task<IResult> htmlTemplate(IFileProvider fileProvider, string fileName, object context)
    {
      return ofHtmlString(await fileProvider.getTemplate(fileName).RenderAsync(context));
    }
  };
}
