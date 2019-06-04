﻿using Materal.Common;
using System;

namespace Log.Service.Model.Log
{
    /// <summary>
    /// 日志查询模型
    /// </summary>
    public class QueryLogFilterModel : PageRequestModel
    {
        /// <summary>
        /// 应用程序
        /// </summary>
        public string Application { get; set; }
        /// <summary>
        /// 等级
        /// </summary>
        public string Level { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? MinTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? MaxTime { get; set; }
    }
}