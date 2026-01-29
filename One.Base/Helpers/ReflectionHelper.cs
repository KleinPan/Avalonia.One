using System;
using System.Diagnostics;
using System.Reflection;

namespace One.Base.Helpers;

/// <summary>反射帮助类 - 注意：此类不支持 AOT 编译，仅在 JIT 环境中使用</summary>
[AttributeUsage(AttributeTargets.Class)]
internal class ReflectionHelperAttribute : Attribute
{
}

/// <summary>反射帮助类 - 使用反射，不兼容 AOT</summary>
public class ReflectionHelper
{
    /// <summary>遍历类属性</summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="model">要遍历的对象实例</param>
    public static void ForeachClassProperties<T>(T model)
    {
        try
        {
            var type = model?.GetType();
            if (type == null) return;

            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                try
                {
                    var value = property.GetValue(model, null);
                    Console.WriteLine($"{property.Name}: {value}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading property {property.Name}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ForeachClassProperties: {ex.Message}");
        }
    }

    /// <summary>遍历类字段</summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="model">要遍历的对象实例</param>
    public static void ForeachClassFields<T>(T model)
    {
        try
        {
            var type = model?.GetType();
            if (type == null) return;

            var fields = type.GetFields();
            foreach (var field in fields)
            {
                try
                {
                    var value = field.GetValue(model);
                    Console.WriteLine($"{field.Name}: {value}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading field {field.Name}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ForeachClassFields: {ex.Message}");
        }
    }

    /// <summary>获取调用者的方法名</summary>
    /// <returns>调用者的方法名</returns>
    public static string GetParentMethodName()
    {
        try
        {
            var stackTrace = new StackTrace(true);
            var frame = stackTrace.GetFrame(1);
            var method = frame?.GetMethod();
            return method?.Name ?? "Unknown";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetParentMethodName: {ex.Message}");
            return "Unknown";
        }
    }
}