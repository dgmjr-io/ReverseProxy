/*
 * ReverseProxyConfig.cs
 *     Created: 2024-06-28T04:34:06-04:00
 *    Modified: 2024-06-28T04:34:06-04:00
 *      Author: David G. Moore, Jr. <david@dgmjr.io>
 *   Copyright: © 2022 - 2024 David G. Moore, Jr., All Rights Reserved
 *     License: MIT (https://opensource.org/licenses/MIT)
 */

using System.Collections;

namespace Dgmjr.AspNetCore.ReverseProxy.Models;

public class ReverseProxyConfig
{
    public List<Upstream> Upstreams { get; set; }
    public Upstream DefaultUpstream { get; set; }
}
