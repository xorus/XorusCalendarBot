using EmbedIO;
using Newtonsoft.Json;
using Swan.Logging;

namespace XorusCalendarBot.Api;

public class Jsonassafdasdf
{
    // /// <summary>
    // /// Asynchronously deserializes a request body in JSON format, using JSON.NET.
    // /// </summary>
    // /// <typeparam name="TData">The expected type of the deserialized data.</typeparam>
    // /// <param name="context">The <see cref="IHttpContext"/> whose request body is to be deserialized.</param>
    // /// <returns>A <see cref="Task{TResult}">Task</see>, representing the ongoing operation,
    // /// whose result will be the deserialized data.</returns>
    // public static async Task<TData?> NewtonsoftJson<TData>(IHttpContext context)
    // {
    //     using (var reader = context.OpenRequestText())
    //     using (var jsonReader = new JsonTextReader(reader))
    //     {
    //         var serializer = new JsonSerializer();
    //         try
    //         {
    //             return await Task.FromResult(serializer.Deserialize<TData>(jsonReader));
    //         }
    //         catch (Exception)
    //         {
    //             $"[{context.Id}] Cannot convert JSON request body to {typeof(TData).Name}, sending 400 Bad Request..."
    //                 .Warn($"{nameof(Json)}.{nameof(NewtonsoftJson)}");
    //
    //             throw HttpException.BadRequest("Incorrect request data format.");
    //         }
    //     }
    // }
}