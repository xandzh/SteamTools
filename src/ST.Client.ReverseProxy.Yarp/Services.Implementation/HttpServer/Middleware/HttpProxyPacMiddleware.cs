using System.Text;
using System.Application.Models;
using Microsoft.AspNetCore.Http;

namespace System.Application.Services.Implementation.HttpServer.Middleware;

/// <summary>
/// Http 代理策略中间件
/// </summary>
sealed class HttpProxyPacMiddleware
{
    readonly IReverseProxyConfig reverseProxyConfig;

    public HttpProxyPacMiddleware(IReverseProxyConfig reverseProxyConfig)
    {
        this.reverseProxyConfig = reverseProxyConfig;
    }

    /// <summary>
    /// 处理请求
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Http 请求经过了 HttpProxy 中间件
        var proxyFeature = context.Features.Get<IHttpProxyFeature>();
        if (proxyFeature != null && proxyFeature.ProxyProtocol == ProxyProtocol.None)
        {
            var proxyPac = CreateProxyPac(context.Request.Host);
            context.Response.ContentType = "application/x-ns-proxy-autoconfig";
            context.Response.Headers.Add("Content-Disposition", $"attachment;filename=proxy.pac");
            await context.Response.WriteAsync(proxyPac);
        }
        else
        {
            await next(context);
        }
    }

    /// <summary>
    /// 创建 proxypac 脚本
    /// </summary>
    /// <param name="proxyHost"></param>
    /// <returns></returns>
    string CreateProxyPac(HostString proxyHost)
    {
        var buidler = new StringBuilder();
        buidler.AppendLine("function FindProxyForURL(url, host){");
        buidler.AppendLine($"    var pac = 'PROXY {proxyHost}';");
        foreach (var domain in reverseProxyConfig.GetDomainPatterns())
        {
            buidler.AppendLine($"    if (shExpMatch(host, '{domain}')) return pac;");
        }
        buidler.AppendLine("    return 'DIRECT';");
        buidler.AppendLine("}");
        return buidler.ToString();
    }
}
