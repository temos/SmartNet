using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace SmartNet;
public static class Extensions
{
    public static IResult JsonText(this IResultExtensions results, string jsonString)
    {
        return Results.Text(jsonString, "application/json");
    }

    public static string GetString(this JsonElement element, string property)
    {
        return element.GetProperty(property).GetString();
    }
}
