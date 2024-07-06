/*
 * ReverseProxyConfig.cs
 *     Created: 2024-06-28T04:34:06-04:00
 *    Modified: 2024-06-28T04:34:06-04:00
 *      Author: David G. Moore, Jr. <david@dgmjr.io>
 *   Copyright: © 2022 - 2024 David G. Moore, Jr., All Rights Reserved
 *     License: MIT (https://opensource.org/licenses/MIT)
 */

using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Dgmjr.AspNetCore.ReverseProxy.Models;

public class Upstream
{
    public bool IsSsl { get; set; } = true;
    public string Host { get; set; }
    public string UpstreamHost { get; set; }
    public List<Route> Routes { get; set; }
    public StringDictionary RequestHeaders { get; set; } = [];
    public StringDictionary ResponseHeaders { get; set; } = [];
    public StringDictionary Replacements { get; set; } = [];
}
