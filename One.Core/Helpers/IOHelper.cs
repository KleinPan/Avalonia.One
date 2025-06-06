﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.Unicode;
 

namespace One.Base.Helpers;

public class IOHelper : BaseHelper
{
    public static IOHelper Instance = new Lazy<IOHelper>(() => new IOHelper()).Value;

    private JsonSerializerOptions jsonSerializerSettings;

    public IOHelper(Action<string> logAction = null) : base(logAction)
    {
        jsonSerializerSettings = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs),
            WriteIndented = true,
            // TypeInfoResolver = SourceGenerationContext.Default
        };
    }

    #region Json

    public T ReadContentFromLocal<T>(string filePath)
    {
        try
        {
            T Config;

            var content = System.IO.File.ReadAllText(filePath);

            Config = System.Text.Json.JsonSerializer.Deserialize<T>(content);

            return Config;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public T ReadContentFromLocalSourceGeneration<T>(string filePath, JsonTypeInfo<T> jsonTypeInfo)
    {
        try
        {
            T Config;

            var content = System.IO.File.ReadAllText(filePath);

            Config = JsonSerializer.Deserialize(content, jsonTypeInfo);

            return Config;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary></summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="filePath">全路径，包括后缀</param>
    public void WriteContentTolocal<T>(T obj, string filePath)
    {
        try
        {
            string newpath = System.IO.Path.GetDirectoryName(filePath);

            if (!Directory.Exists(newpath))
            {
                Directory.CreateDirectory(newpath);
            }

            var json = JsonSerializer.Serialize(obj, jsonSerializerSettings);//SourceGenerationContext.Default.AllConfigModel

            System.IO.File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary></summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="filePath">全路径，包括后缀</param>
    public void WriteContentTolocalSourceGeneration<T>(T obj, string filePath, JsonTypeInfo<T> jsonTypeInfo)
    {
        try
        {
            string newpath = System.IO.Path.GetDirectoryName(filePath);

            if (!Directory.Exists(newpath))
            {
                Directory.CreateDirectory(newpath);
            }

            var json = JsonSerializer.Serialize(obj, jsonTypeInfo);//SourceGenerationContext.Default.AllConfigModel

            System.IO.File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    #endregion Json

    #region Xml

    /// <summary>XML序列化某一类型到指定的文件</summary>
    /// <param name="filePath"></param>
    /// <param name="obj"></param>
    /// <param name="type"></param>
    public void SerializeToXml<T>(string filePath, T obj)
    {
        try
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath))
            {
                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(T));
                xs.Serialize(writer, obj);
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>从某一XML文件反序列化到某一类型</summary>
    /// <param name="filePath">待反序列化的XML文件名称</param>
    /// <param name="type">反序列化出的</param>
    /// <returns></returns>
    public T DeserializeFromXml<T>(string filePath)
    {
        try
        {
            if (!System.IO.File.Exists(filePath))
                throw new ArgumentNullException(filePath + " not Exists");

            using (System.IO.StreamReader reader = new System.IO.StreamReader(filePath))
            {
                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(T));
                T ret = (T)xs.Deserialize(reader);
                return ret;
            }
        }
        catch (Exception ex)
        {
            return default(T);
        }
    }

    #endregion Xml

    #region Ini

    /// <summary>读取INI文件值</summary>
    /// <param name="section">节点名</param>
    /// <param name="key">键</param>
    /// <param name="def">未取到值时返回的默认值</param>
    /// <param name="filePath">INI文件完整路径</param>
    /// <returns>读取的值</returns>
    //public static string Read(string section, string key, string def, string filePath)
    //{
    //    StringBuilder sb = new StringBuilder(1024);
    //    Vanara.PInvoke.Kernel32.GetPrivateProfileString(section, key, def, sb, 1024, filePath);
    //    return sb.ToString();
    //}

    /// <summary>写INI文件值</summary>
    /// <param name="section">欲在其中写入的节点名称</param>
    /// <param name="key">欲设置的项名</param>
    /// <param name="value">要写入的新字符串</param>
    /// <param name="filePath">INI文件完整路径</param>
    /// <returns>非零表示成功，零表示失败</returns>
    //public static bool Write(string section, string key, string value, string filePath)
    //{
    //    if (!File.Exists(filePath))
    //    {
    //        return false;
    //    }
    //    return Vanara.PInvoke.Kernel32.WritePrivateProfileString(section, key, value, filePath);
    //}

    /// <summary>删除节</summary>
    /// <param name="section">节点名</param>
    /// <param name="filePath">INI文件完整路径</param>
    /// <returns>非零表示成功，零表示失败</returns>
    //public static bool DeleteSection(string section, string filePath)
    //{
    //    return Write(section, null, null, filePath);
    //}

    /// <summary>删除键的值</summary>
    /// <param name="section">节点名</param>
    /// <param name="key">键名</param>
    /// <param name="filePath">INI文件完整路径</param>
    /// <returns>非零表示成功，零表示失败</returns>
    //public static bool DeleteKey(string section, string key, string filePath)
    //{
    //    return Write(section, key, null, filePath);
    //}

    #endregion Ini

    public void WipeFile(string filename, int timesToWrite)
    {
        try
        {
            if (File.Exists(filename))
            {
                //设置文件的属性为正常，这是为了防止文件是仅仅读
                File.SetAttributes(filename, FileAttributes.Normal);
                //计算扇区数目
                double sectors = Math.Ceiling(new FileInfo(filename).Length / 512.0);
                // 创建一个相同大小的虚拟缓存
                byte[] dummyBuffer = new byte[512];
                // 创建一个加密随机数目生成器
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                // 打开这个文件的FileStream
                FileStream inputStream = new FileStream(filename, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                for (int currentPass = 0; currentPass < timesToWrite; currentPass++)
                {
                    // 文件流位置
                    inputStream.Position = 0;
                    //循环全部的扇区
                    for (int sectorsWritten = 0; sectorsWritten < sectors; sectorsWritten++)
                    {
                        //把垃圾数据填充到流中
                        rng.GetBytes(dummyBuffer);
                        // 写入文件流中
                        inputStream.Write(dummyBuffer, 0, dummyBuffer.Length);
                    }
                }
                // 清空文件
                inputStream.SetLength(0);
                // 关闭文件流
                inputStream.Close();
                // 清空原始日期须要
                DateTime dt = new DateTime(2037, 1, 1, 0, 0, 0);
                File.SetCreationTime(filename, dt);
                File.SetLastAccessTime(filename, dt);
                File.SetLastWriteTime(filename, dt);
                // 删除文件
                File.Delete(filename);
            }
        }
        catch (Exception)
        {
        }
    }
}