/*
 * ReverseProxyConfig.cs
 *     Created: 2024-06-28T04:34:06-04:00
 *    Modified: 2024-06-28T04:34:06-04:00
 *      Author: David G. Moore, Jr. <david@dgmjr.io>
 *   Copyright: © 2022 - 2024 David G. Moore, Jr., All Rights Reserved
 *     License: MIT (https://opensource.org/licenses/MIT)
 */

namespace Dgmjr.AspNetCore.ReverseProxy.Models;

public class Route
{
    public string ProxyRoute { get; set; }
    public string UpstreamRoute { get; set; }
}
