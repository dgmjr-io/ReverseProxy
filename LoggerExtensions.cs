/*
 * LoggerExtensions.cs
 *     Created: 2024-07-05T11:47:17-04:00
 *    Modified: 2024-07-05T11:47:17-04:00
 *      Author: David G. Moore, Jr. <david@dgmjr.io>
 *   Copyright: © 2022 - 2024 David G. Moore, Jr., All Rights Reserved
 *     License: MIT (https://opensource.org/licenses/MIT)
 */

namespace Dgmjr.AspNetCore.ReverseProxy;

public static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = """"
        Rewrote content from
        {OriginalContent} to {RewrittenContent}
        """"
    )]
    public static partial void RewroteContent(
        this ILogger logger,
        string originalContent,
        string rewrittenContent
    );

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Redirected request from {OriginalUrl} to {NewUrl}"
    )]
    public static partial void RedirectedRequest(
        this ILogger logger,
        string originalUrl,
        string newUrl
    );
}
