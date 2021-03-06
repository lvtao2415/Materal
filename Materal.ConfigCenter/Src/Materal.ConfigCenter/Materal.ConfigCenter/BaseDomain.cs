﻿using Materal.TTA.Common;
using System;

namespace Materal.ConfigCenter
{
    public abstract class BaseDomain : IEntity<Guid>
    {
        protected BaseDomain()
        {
            ID = Guid.NewGuid();
            CreateTime = DateTime.Now;
            UpdateTime = DateTime.MinValue;
        }
        public Guid ID { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }
}
