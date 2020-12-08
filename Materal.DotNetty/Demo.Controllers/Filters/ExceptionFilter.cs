﻿using Demo.Common;
using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using Materal.ConvertHelper;
using Materal.DotNetty.Common;
using Materal.DotNetty.ControllerBus.Filters;
using Materal.Model;
using System;

namespace Demo.Controllers.Filters
{
    public class ExceptionFilter : IExceptionFilter
    {
        public IFullHttpResponse HandlerException(IByteBufferHolder byteBufferHolder, Exception exception)
        {
            ConsoleHelper.WriteError(exception);
            ResultModel resultModel = ResultModel.Fail("服务器错误，请联系后端工程师");
            return HttpResponseHelper.GetHttpResponse(HttpResponseStatus.OK, resultModel.ToJson());
        }
    }
}
