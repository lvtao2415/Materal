﻿using Materal.APP.HttpClient;
using Materal.StringHelper;
using WebAPP.Common;

namespace WebAPP.HttpClientImpl.Deploy
{
    public abstract class DeployHttpClient : BaseHttpClient
    {
        protected DeployHttpClient(IAuthorityManage authorityManage) : base(authorityManage)
        {
        }
        protected override string GetUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(UrlManage.DeployUrl) || !UrlManage.DeployUrl.IsUrl()) throw new WebAPPException("尚未与发布服务取得联系");
            return $"{UrlManage.DeployUrl}{url}";
        }
    }
}
