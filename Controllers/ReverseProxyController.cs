// /*
//  * ReverseProxyController.cs
//  *     Created: 2024-07-01T06:14:15-04:00
//  *    Modified: 2024-07-01T06:14:16-04:00
//  *      Author: David G. Moore, Jr. <david@dgmjr.io>
//  *   Copyright: © 2022 - 2024 David G. Moore, Jr., All Rights Reserved
//  *     License: MIT (https://opensource.org/licenses/MIT)
//  */

// namespace Dgmjr.AspNetCore.ReverseProxy.Controllers;

// using System.Net.Http;
// using System.Net.Http.Headers;

// using Dgmjr.AspNetCore.Mvc;

// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Options;

// [ApiController]
// [Route("[controller]")]
// public class ReverseProxyController(
//     ILogger<ReverseProxyController> logger,
//     HttpClient httpClient,
//     IOptions<ReverseProxyConfig> proxyConfig
// ) : ApiControllerBase(logger)
// {
//     private readonly HttpClient _httpClient = httpClient;
//     private readonly ReverseProxyConfig _proxyConfig = proxyConfig.Value;

//     [HttpGet]
//     [HttpPost]
//     [HttpPut]
//     [HttpDelete]
//     [HttpPatch]
//     [HttpHead]
//     [HttpOptions]
//     public async Task<IActionResult> ProxyAllRequests()
//     {
//         var matchedRoute = _proxyConfig.Routes
//             .OrderByDescending(route => route.Value.Path.Length) // Ensure the longest matching prefix is selected
//             .FirstOrDefault(
//                 route =>
//                     HttpContext.Request.Path.StartsWithSegments(
//                         route.Value.Path,
//                         out var remainingPath
//                     )
//             );

//         if (matchedRoute.Value is null)
//         {
//             return NotFound();
//         }

//         HttpContext.Request.Path.StartsWithSegments(matchedRoute.Value.Path, out var remainingPath);

//         var targetUri = new Uri(
//             matchedRoute.Value.ProxyPass
//                 + HttpContext.Request.Path
//                 + (remainingPath.HasValue ? "/" + remainingPath : "")
//                 + HttpContext.Request.QueryString
//         );
//         var requestMessage = new HttpRequestMessage
//         {
//             Method = new HttpMethod(HttpContext.Request.Method),
//             RequestUri = targetUri,
//             Content = HttpContext.Request.Body.CanSeek
//                 ? new StreamContent(HttpContext.Request.Body)
//                 : null
//         };

//         foreach (var header in HttpContext.Request.Headers)
//         {
//             requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
//         }

//         foreach (var header in matchedRoute.Value.Config.SetHeaders)
//         {
//             requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
//         }

//         var responseMessage = await _httpClient.SendAsync(requestMessage);

//         var responseContent = await responseMessage.Content.ReadAsStringAsync();

//         foreach (var replacement in matchedRoute.Value.Config.Replacements)
//         {
//             responseContent = responseContent.Replace(replacement.Key, replacement.Value);
//         }

//         return new ContentResult()
//         {
//             Content = responseContent,
//             ContentType = responseMessage.Content.Headers.ContentType?.ToString(),
//             StatusCode = (int)responseMessage.StatusCode
//         };
//     }
// }
