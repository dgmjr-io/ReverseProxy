/*
 * Constants.cs
 *     Created: 2024-07-02T17:11:51-04:00
 *    Modified: 2024-07-02T17:11:51-04:00
 *      Author: David G. Moore, Jr. <david@dgmjr.io>
 *   Copyright: © 2022 - 2024 David G. Moore, Jr., All Rights Reserved
 *     License: MIT (https://opensource.org/licenses/MIT)
 */

using MimeKit;

namespace Dgmjr.AspNetCore.ReverseProxy;

public static class Constants
{
    public const string AppSettings = nameof(AppSettings);
    public const string _Json = ".json";
    public const string AppsettingsJson = "appsettings" + _Json;
    public const string ReverseProxyConfigJson = $"{nameof(ReverseProxyConfig)}{_Json}";
    public const string SerilogJson = $"{nameof(Serilog)}{_Json}";

    public static class Protocols
    {
        public const string Http = "http://";
        public const string Https = "https://";
    }

    public static class Headers
    {
        public const string Forwarded = "Forwarded";
        public const string Host = "Host";
        public const string ContentLength = "content-length";
        public const string XForwardedFor = "X-Forwarded-For";
        public const string XForwardedHost = "X-Forwarded-Host";
        public const string XForwardedProto = "X-Forwarded-Proto";
        public const string XForwardedPath = "X-Forwarded-Path";
        public const string XForwardedUri = "X-Forwarded-Uri";
        public const string ContentEncoding = "Content-Encoding";
        public static readonly string[] OverwrittenRequestHeaders = [Host];
        public static readonly string[] OverwrittenResponseHeaders = [ContentEncoding];
    }
}
