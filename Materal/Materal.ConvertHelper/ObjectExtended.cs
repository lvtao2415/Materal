﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Materal.ConvertHelper
{
    /// <summary>
    /// Object扩展
    /// </summary>
    public static class ObjectExtended
    {
        /// <summary>
        /// 可转换类型字典
        /// </summary>
        private static readonly Dictionary<Type, Func<object, object>> Dict = new Dictionary<Type, Func<object, object>>();
        /// <summary>
        /// 构造方法
        /// </summary>
        static ObjectExtended()
        {
            Dict.Add(typeof(int), WrapValueConvert(Convert.ToInt32));
            Dict.Add(typeof(long), WrapValueConvert(Convert.ToInt64));
            Dict.Add(typeof(short), WrapValueConvert(Convert.ToInt16));
            Dict.Add(typeof(int?), WrapValueConvert(Convert.ToInt32));
            Dict.Add(typeof(double), WrapValueConvert(Convert.ToDouble));
            Dict.Add(typeof(float), WrapValueConvert(Convert.ToSingle));
            Dict.Add(typeof(Guid), f => new Guid(f.ToString()));
            Dict.Add(typeof(string), Convert.ToString);
            Dict.Add(typeof(DateTime), WrapValueConvert(Convert.ToDateTime));
        }
        /// <summary>
        /// 写入值转换类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        private static Func<object, object> WrapValueConvert<T>(Func<object, T> input) where T : struct
        {
            return i =>
            {
                if (i == null || i is DBNull) return null;
                return input(i);
            };
        }
        /// <summary>
        /// 对象转换为数据行
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <param name="dr">数据行模版</param>
        /// <returns>数据行</returns>
        public static DataRow ToDataRow(this object obj, DataRow dr)
        {
            if (dr == null)throw new MateralConvertException("数据行不可为空");
            var type = obj.GetType();
            var props = type.GetProperties();
            foreach (var prop in props)
            {
                var value = prop.GetValue(obj, null);
                if (value == null)
                {
                    dr[prop.Name] = DBNull.Value;
                }
                else
                {
                    dr[prop.Name] = value;
                }
            }

            return dr;
        }
        /// <summary>
        /// 对象转换为数据行
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns>数据行</returns>
        public static DataRow ToDataRow(this object obj)
        {
            var type = obj.GetType();
            var dt = type.ToDataTable();
            var dr = dt.NewRow();
            return obj.ToDataRow(dr);
        }
        /// <summary>
        /// 通过数据行设置对象的值
        /// </summary>
        /// <param name="obj">要设置的对象</param>
        /// <param name="dr">数据行</param>
        public static void SetValueByDataRow(this object obj, DataRow dr)
        {
            var type = obj.GetType();
            var props = type.GetProperties();
            foreach (var prop in props)
            {
                try
                {
                    prop.SetValue(obj, dr[prop.Name], null);
                }
                catch
                {
                    // ignored
                }
            }
        }
        /// <summary>
        /// 获得默认对象
        /// </summary>
        /// <param name="obj">要设置的对象</param>
        /// <param name="type">要设置的类型</param>
        /// <returns>默认对象</returns>
        public static object GetDefultObject(this object obj, Type type)
        {
            return ConvertManager.GetDefultObject(type);
        }
        /// <summary>
        /// 获得默认对象
        /// </summary>
        /// <typeparam name="T">要设置的类型</typeparam>
        /// <param name="obj">要设置的对象</param>
        /// <returns>默认对象</returns>
        public static T GetDefultObject<T>(this object obj)
        {
            return ConvertManager.GetDefultObject<T>();
        }
        /// <summary>
        /// 对象转换为Josn
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <returns>转换后的Json字符串</returns>
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        /// <summary>
        /// 属性复制sourceM->targetM
        /// </summary>
        /// <typeparam name="T">复制的模型</typeparam>
        /// <param name="sourceM">复制源头对象</param>
        /// <param name="targetM">复制目标对象</param>
        /// <param name="notCopyPropertieNames">不复制的属性名称</param>
        /// <returns>复制的对象</returns>
        public static void CopyProperties<T>(this object sourceM, T targetM, params string[] notCopyPropertieNames)
        {
            if (sourceM == null) return;
            var t1Props = sourceM.GetType().GetProperties();
            var t2Props = typeof(T).GetProperties();
            foreach (var prop in t1Props)
            {
                if (notCopyPropertieNames.Contains(prop.Name)) continue;
                var tempProp = t2Props.FirstOrDefault(m => m.Name == prop.Name);
                if (tempProp != null && tempProp.CanWrite)
                {
                    tempProp.SetValue(targetM, prop.GetValue(sourceM, null), null);
                }
            }
        }
        /// <summary>
        /// 属性复制
        /// </summary>
        /// <typeparam name="T">复制的模型</typeparam>
        /// <param name="sourceM">复制源头对象</param>
        /// <param name="notCopyPropertieNames">不复制的属性名称</param>
        /// <returns>复制的对象</returns>
        public static T CopyProperties<T>(this object sourceM, params string[] notCopyPropertieNames)
        {
            if (sourceM == null) return default(T);
            var targetM = ConvertManager.GetDefultObject<T>();
            sourceM.CopyProperties(targetM, notCopyPropertieNames);
            return targetM;
        }
        /// <summary>
        /// 将对象转换为byte数组
        /// </summary>
        /// <param name="obj">被转换对象</param>
        /// <returns>转换后byte数组</returns>
        public static byte[] MToBytes(this object obj)
        {
            byte[] buff;
            using (var ms = new MemoryStream())
            {
                IFormatter iFormatter = new BinaryFormatter();
                iFormatter.Serialize(ms, obj);
                buff = ms.GetBuffer();
            }
            return buff;
        }

        /// <summary>
        /// 判断是否提供到特定类型的转换
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static bool CanConvertTo(this object obj, Type targetType)
        {
            return Dict.ContainsKey(targetType);
        }
        /// <summary>
        /// 转换到特定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T ConvertTo<T>(this object obj)
        {
            return (T)ConvertTo(obj, typeof(T));
        }
        /// <summary>
        /// 转换到特定类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static object ConvertTo(this object obj, Type targetType)
        {
            if (obj == null)return !targetType.IsValueType ? (object) null : throw new ArgumentNullException(nameof(obj), "不能将null转换为" + targetType.Name);
            if (obj.GetType() == targetType || targetType.IsInstanceOfType(obj))return obj;
            if (Dict.ContainsKey(targetType))return Dict[targetType](obj);
            try
            {
                return Convert.ChangeType(obj, targetType);
            }
            catch
            {
                throw new NotImplementedException("未实现到" + targetType.Name + "的转换");
            }
        }

        /// <summary>
        /// 克隆对象(Json序列化)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputObj">输入对象</param>
        /// <returns></returns>
        public static T CloneByJson<T>(this T inputObj)
        {
            string jsonStr = inputObj.ToJson();
            return jsonStr.JsonToObject<T>();
        }

        /// <summary>
        /// 克隆对象(XML序列化)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputObj">输入对象</param>
        /// <returns>克隆的对象</returns>
        public static T CloneByXml<T>(this T inputObj)
        {
            Type tType = inputObj.GetType();
            Attribute attr = tType.GetCustomAttribute(typeof(SerializableAttribute));
            if (attr == null) throw new MateralConvertException("未标识为可序列化");
            object resM;
            using (var ms = new MemoryStream())
            {
                var xml = new XmlSerializer(typeof(T));
                xml.Serialize(ms, inputObj);
                ms.Seek(0, SeekOrigin.Begin);
                resM = xml.Deserialize(ms);
                ms.Close();
            }
            return (T)resM;
        }
        /// <summary>
        /// 克隆对象(反射)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputObj">输入对象</param>
        /// <returns>克隆的对象</returns>
        public static T CloneByReflex<T>(this T inputObj)
        {
            Type tType = inputObj.GetType();
            var resM = (T)Activator.CreateInstance(tType);
            PropertyInfo[] pis = tType.GetProperties();
            foreach (PropertyInfo pi in pis)
            {
                object piValue = pi.GetValue(inputObj);
                pi.SetValue(resM, piValue is ValueType ? piValue : Clone(piValue));
            }
            return resM;
        }
        /// <summary>
        /// 克隆对象(二进制序列化)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputObj">输入对象</param>
        /// <returns>克隆的对象</returns>
        public static T CloneBySerializable<T>(this T inputObj)
        {
            Type tType = inputObj.GetType();
            Attribute attr = tType.GetCustomAttribute(typeof(SerializableAttribute));
            if (attr == null) throw new MateralConvertException("未标识为可序列化");
            using (var stream = new MemoryStream())
            {
                var bf2 = new BinaryFormatter();
                bf2.Serialize(stream, inputObj);
                stream.Position = 0;
                return (T)bf2.Deserialize(stream);
            }
        }
        /// <summary>
        /// 克隆对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputObj">输入对象</param>
        /// <returns>克隆的对象</returns>
        public static T Clone<T>(this T inputObj)
        {
            Type tType = inputObj.GetType();
            Attribute attr = tType.GetCustomAttribute(typeof(SerializableAttribute));
            return attr != null ? CloneBySerializable(inputObj) : CloneByReflex(inputObj);
        }
    }
}