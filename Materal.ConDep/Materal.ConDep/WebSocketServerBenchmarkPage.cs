﻿using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using Materal.ConDep.Authority;
using Materal.ConDep.Common;
using Materal.ConvertHelper;
using Materal.Model;
using Materal.WebSocket.Http;
using Materal.WebSocket.Http.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DotNetty.Common.Utilities;

namespace Materal.ConDep
{
    public class WebSocketServerBenchmarkPage
    {
        /// <summary>
        /// 获取应答
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static DefaultFullHttpResponse GetResponse(IFullHttpRequest request)
        {
            if (request.Uri.LastIndexOf("/api", StringComparison.Ordinal) == 0)
            {
                return GetAPI(request);
            }
            return GetFiles(request.Uri);
        }
        /// <summary>
        /// 获得接口
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static DefaultFullHttpResponse GetAPI(IFullHttpRequest request)
        {
            try
            {
                string[] names = request.Uri.Substring(5).Split('/');
                var controllerBus = ApplicationData.GetService<IControllerBus>();
                object controller = controllerBus.GetController($"{names[0]}Controller");
                if (controller == null) return GetFailResult("未找到控制器");
                string actionName = names[1].Split('?')[0];
                return InvokeAction(request, controller, actionName);
            }
            catch (Exception ex)
            {
                return GetFailResult(ex.Message);
            }
        }

        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="request"></param>
        /// <param name="controller"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        private static DefaultFullHttpResponse InvokeAction(IFullHttpRequest request, object controller, string actionName)
        {
            Type controllerType = controller.GetType();
            MethodInfo action = controllerType.GetMethod(actionName);
            if (action == null) return GetFailResult("未找到方法");
            string bodyParams = GetBodyParams(request);
            Dictionary<string, string> urlParams = GetUrlParams(request);
            if (!CanLoginSuccess(action, bodyParams, urlParams)) return GetFailResult("请登录");
            if (!CanMethodSuccess(request, action)) return GetFailResult("Method错误");
            ParameterInfo[] parameters = action.GetParameters();
            var objects = new object[parameters.Length];
            object actionResult;
            if (parameters.Length > 0)
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType.IsClass && parameters[i].ParameterType != typeof(string))
                    {
                        objects[i] = bodyParams.JsonToObject(parameters[i].ParameterType);
                    }
                    else
                    {
                        objects[i] = urlParams[parameters[i].Name].ConvertTo(parameters[i].ParameterType);
                    }
                }
                actionResult = action.Invoke(controller, objects);
            }
            else
            {
                actionResult = action.Invoke(controller, null);
            }
            switch (actionResult)
            {
                case ResultModel resultModel:
                    IByteBuffer body = Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes(resultModel.ToJson()));
                    var response = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK, body);
                    response.Headers.Set(HttpHeaderNames.ContentType, "application/json; charset=UTF-8");
                    return response;
            }
            return GetFailResult("返回错误");
        }
        /// <summary>
        /// Method验证
        /// </summary>
        /// <param name="request"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private static bool CanMethodSuccess(IFullHttpRequest request, MethodInfo action)
        {
            Attribute[] attribute = action.GetCustomAttributes().ToArray();
            if (request.Method.Name == "GET" && attribute.Any(m => m is HttpGetAttribute) ||
                request.Method.Name == "POST" && attribute.Any(m => m is HttpPostAttribute))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 是否登录
        /// </summary>
        /// <param name="action"></param>
        /// <param name="bodyParams"></param>
        /// <param name="urlParams"></param>
        /// <returns></returns>
        private static bool CanLoginSuccess(MemberInfo action, string bodyParams, IReadOnlyDictionary<string, string> urlParams)
        {
            Attribute[] attribute = action.GetCustomAttributes().ToArray();
            if (attribute.Any(m => m is AllowAnonymousAttribute)) return true;
            string token;
            if (urlParams.ContainsKey("token"))
            {
                token = urlParams["token"];
            }
            else if (bodyParams.Contains("Token"))
            {
                var model = bodyParams.JsonToObject<DefaultRequestModel>();
                token = model.Token;
            }
            else
            {
                return false;
            }
            var authorityService = ApplicationData.GetService<IAuthorityService>();
            return authorityService.IsLogin(token);
        }
        /// <summary>
        /// 获得参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string GetBodyParams(IFullHttpRequest request)
        {
            string @params = request.Content.ReadString(request.Content.Capacity, Encoding.UTF8);
            return @params;
        }
        /// <summary>
        /// 获得字典参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetUrlParams(IFullHttpRequest request)
        {
            var @params = new Dictionary<string, string>();
            string[] tempString = request.Uri.Split('?');
            if (tempString.Length <= 1) return @params;
            string[] paramsString = tempString[1].Split('&');
            foreach (string item in paramsString)
            {
                string[] values = item.Split("=");
                if (@params.ContainsKey(values[0])) continue;
                @params.Add(values[0], values[1]);
            }
            return @params;
        }
        /// <summary>
        /// 获得失败返回
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static DefaultFullHttpResponse GetFailResult(string message)
        {
            IByteBuffer body = Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes(ResultModel.Fail(message).ToJson()));
            var response = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK, body);
            response.Headers.Set(HttpHeaderNames.AccessControlAllowOrigin, "*");
            response.Headers.Set(HttpHeaderNames.ContentLength, "body.Length");
            response.Headers.Set(HttpHeaderNames.ContentType, "application/json; charset=utf-8");
            response.Headers.Set(HttpHeaderNames.Date, DateTime.Now);
            response.Headers.Set(HttpHeaderNames.Server, "Materal.ConDep");
            response.Headers.Set(HttpHeaderNames.Vary, "Accept-Encoding");
            return response;
        }
        /// <summary>
        /// 获取文件
        /// </summary>
        /// <param name="pageName"></param>
        /// <returns></returns>
        private static DefaultFullHttpResponse GetFiles(string pageName)
        {
#if DEBUG
            const string basePath = @"E:/Project/Materal/Project/Materal.ConDep/Materal.ConDep/";
#else
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
#endif
            string actionName = pageName.Split('?')[0];
            string filePath = pageName.Contains(".") ? $"{basePath}HtmlPages{actionName}" : $"{basePath}HtmlPages{actionName}.html";
            if (!File.Exists(filePath)) filePath = basePath + "/HtmlPages/404.html";
            using (var streamReader = new StreamReader(filePath))
            {
                string htmlText = streamReader.ReadToEnd();
                IByteBuffer body = Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes(htmlText));
                var result = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK, body);
                result.Headers.Set(HttpHeaderNames.ContentType, "text/html; charset=UTF-8");
                return result;
            }
        }
    }
}
