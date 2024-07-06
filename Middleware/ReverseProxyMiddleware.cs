/*
 * ReverseProxyMiddleware.cs
 *     Created: 2024-07-02T10:49:29-04:00
 *    Modified: 2024-07-02T10:49:29-04:00
 *      Author: David G. Moore, Jr. <david@dgmjr.io>
 *   Copyright: © 2022 - 2024 David G. Moore, Jr., All Rights Reserved
 *     License: MIT (https://opensource.org/licenses/MIT)
 */

namespace Dgmjr.AspNetCore.ReverseProxy.Middleware;

using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression; // Add this line

using Dgmjr.Abstractions;
using Dgmjr.Mime;
using System.Net.Http.Headers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Options;

using Serilog.Configuration;

using static Microsoft.AspNetCore.Http.StatusCodes;

using Constants = Dgmjr.AspNetCore.ReverseProxy.Constants;

public class ReverseProxyMiddleware : ILog
{
    private readonly HttpClient _httpClient;
    private readonly ReverseProxyConfig _proxyConfig;

    public virtual ILogger Logger { get; }

    public ReverseProxyMiddleware(
        RequestDelegate _,
        IHttpClientFactory httpClientFactory,
        IOptions<ReverseProxyConfig> proxyConfig,
        ILogger<ReverseProxyMiddleware> logger
    )
    {
        Logger = logger;
        _httpClient = httpClientFactory.CreateClient(typeof(ReverseProxyMiddleware).FullName);
        _proxyConfig = proxyConfig.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var host = context.Request.Host.Host;

        var upstreamConfig = _proxyConfig.Upstreams.Find(
            u =>
                string.Equals(u.Host, host, OrdinalIgnoreCase)
                && u.Routes.Exists(
                    r =>
                        context.Request.Path.ToString().StartsWith(r.ProxyRoute, OrdinalIgnoreCase)
                )
        );

        upstreamConfig ??= _proxyConfig.DefaultUpstream;

        var route = upstreamConfig.Routes.Find(
            r => context.Request.Path.ToString().StartsWith(r.ProxyRoute, OrdinalIgnoreCase)
        ) ?? _proxyConfig.DefaultUpstream.Routes[0];

        var remainingPath = context.Request.Path.ToString()[route.ProxyRoute.Length..];

        // context.Request.Path.StartsWithSegments(route.ProxyRoute, out var remainingPath);
        var targetUri = new Uri(
            (upstreamConfig.IsSsl ? Constants.Protocols.Https : Constants.Protocols.Http)
                + upstreamConfig.UpstreamHost
                + route.UpstreamRoute
                + remainingPath
                + context.Request.QueryString
        );
        Logger.RedirectedRequest(context.Request.GetDisplayUrl(), targetUri.ToString());

        var requestMessage = new HttpRequestMessage
        {
            RequestUri = targetUri,
            Method = new HttpMethod(context.Request.Method)
        };

        CopyRequestContentAndHeaders(context.Request, requestMessage, upstreamConfig);

        var responseMessage = await _httpClient.SendAsync(
            requestMessage,
            HttpCompletionOption.ResponseHeadersRead,
            context.RequestAborted
        );
        context.Response.StatusCode = (int)responseMessage.StatusCode;
        context.Response.Headers[Constants.Headers.XForwardedFor] =
            context.Connection.RemoteIpAddress.ToString();
        context.Response.Headers[Constants.Headers.XForwardedUri] = targetUri.ToString();

        CopyResponseHeaders(context, responseMessage, upstreamConfig);

        if (responseMessage.Content != null && IsTextOrJsonContent(responseMessage.Content.Headers))
        {
            using var originalContentStream = await responseMessage.Content.ReadAsStreamAsync();
            var originalContent = await new StreamReader(originalContentStream).ReadToEndAsync();
            var modifiedContent = originalContent;

            foreach(var replacement in upstreamConfig.Replacements)
            {
                modifiedContent = Regx.Replace(modifiedContent, replacement.Key, replacement.Value);
            }

            Logger.RewroteContent(originalContent, modifiedContent);

            context.Response.Headers[Constants.Headers.ContentLength] = Encoding.UTF8
                .GetByteCount(modifiedContent)
                .ToString();

            await context.Response.WriteAsync(modifiedContent, Encoding.UTF8);
        }
        else
        {
            await responseMessage.Content.CopyToAsync(context.Response.Body);
        }
    }

    private static bool IsTextOrJsonContent(HttpContentHeaders headers)
    {
        var mediaType = headers.ContentType?.MediaType?.ToLowerInvariant();
        return mediaType?.StartsWith("text") == true || mediaType == "application/json";
    }

    private static void CopyRequestContentAndHeaders(
        HttpRequest request,
        HttpRequestMessage requestMessage,
        Upstream upstreamConfig
    )
    {
        foreach (var header in request.Headers)
        {
            if (
                !Constants.Headers.OverwrittenRequestHeaders.Contains(
                    header.Key,
                    StringComparer.OrdinalIgnoreCase
                )
            )
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        foreach (var header in upstreamConfig?.RequestHeaders ?? [])
        {
            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (
            !(
                HttpMethods.IsGet(request.Method)
                || HttpMethods.IsHead(request.Method)
                || HttpMethods.IsDelete(request.Method)
                || HttpMethods.IsTrace(request.Method)
            )
        )
        {
            requestMessage.Content = new StreamContent(request.Body);
            foreach (var header in request.Headers)
            {
                requestMessage.Content.Headers.TryAddWithoutValidation(
                    header.Key,
                    header.Value.ToArray()
                );
            }
        }

        requestMessage.Headers.TryAddWithoutValidation(
            Constants.Headers.XForwardedFor,
            request.HttpContext.Connection.RemoteIpAddress.ToString()
        );
        requestMessage.Headers.TryAddWithoutValidation(Constants.Headers.Host, upstreamConfig?.UpstreamHost);
    }

    private static void CopyResponseHeaders(
        HttpContext context,
        HttpResponseMessage responseMessage,
        Upstream upstreamConfig
    )
    {
        foreach (var header in responseMessage.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        foreach (var header in responseMessage.Content.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        foreach (var header in upstreamConfig?.ResponseHeaders ?? new Dictionary<string, string>())
        {
            context.Response.Headers[header.Key] = header.Value;
        }

        context.Response.Headers.Remove("transfer-encoding");
    }
}
