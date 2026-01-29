using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using static One.Base.Helpers.HIDInterfaceHelper;

namespace One.Base.Helpers;

public class HIDInterfaceHelper
{
    #region constants

    private const int DIGCF_DEFAULT = 0x1;
    private const int DIGCF_PRESENT = 0x2;
    private const int DIGCF_ALLCLASSES = 0x4;
    private const int DIGCF_PROFILE = 0x8;
    private const int DIGCF_DEVICEINTERFACE = 0x10;

    // 常量定义
    private const int ERROR_NO_MORE_ITEMS = 259;

    #endregion constants

    #region win32_API_declarations

    [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, uint Flags);

    [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool SetupDiEnumDeviceInterfaces(IntPtr hDevInfo, IntPtr devInfo, ref Guid interfaceClassGuid, uint memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

    /// <summary>使用此函数获取接口详细信息通常分为两步：</summary>
    /// <param name="hDevInfo">指向设备信息集的指针，它包含了所要接收信息的接口。该句柄通常由 <see cref="SetupDiGetClassDevs"/> 函数返回。</param>
    /// <param name="deviceInterfaceData">一个指向 <see cref="SP_DEVICE_INTERFACE_DATA"/> 结构的指针，该结构指定了 DeviceInfoSet 参数中设备的接口。</param>
    /// <param name="deviceInterfaceDetailData">该结构用于接收指定接口的信息。</param>
    /// <param name="deviceInterfaceDetailDataSize">指定的缓冲的大小。</param>
    /// <param name="requiredSize"></param>
    /// <param name="deviceInfoData"></param>
    /// <returns></returns>
    [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool SetupDiGetDeviceInterfaceDetail(
       IntPtr hDevInfo,
       ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
       //ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData,  // 使用IntPtr而不是结构体引用
       IntPtr deviceInterfaceDetailData,
       uint deviceInterfaceDetailDataSize,
       out uint requiredSize,
       ref SP_DEVINFO_DATA deviceInfoData
    );

    [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

    [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool SetupDiGetDeviceRegistryProperty(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, uint property, out uint propertyRegDataType, IntPtr propertyBuffer, uint propertyBufferSize, out uint requiredSize);

    #endregion win32_API_declarations

    #region structs

    [StructLayout(LayoutKind.Sequential)]
    public struct SP_DEVINFO_DATA
    {
        public uint cbSize;
        public Guid ClassGuid;
        public uint DevInst;
        public IntPtr Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SP_DEVICE_INTERFACE_DATA
    {
        public uint cbSize;
        public Guid InterfaceClassGuid;
        public uint Flags;
        public IntPtr Reserved;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct SP_DEVICE_INTERFACE_DETAIL_DATA
    {
        public int cbSize;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string DevicePath;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HIDD_ATTRIBUTES
    {
        public int Size;
        public short VendorID;
        public short ProductID;
        public short VersionNumber;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct COMMTIMEOUTS
    {
        public uint ReadIntervalTimeout;
        public uint ReadTotalTimeoutMultiplier;
        public uint ReadTotalTimeoutConstant;
        public uint WriteTotalTimeoutMultiplier;
        public uint WriteTotalTimeoutConstant;
    }

    #endregion structs

    #region static_methods

    public static Guid GetGuid(DeviceType deviceType)
    {
        //全部的定义
        //https://learn.microsoft.com/en-us/windows-hardware/drivers/install/guid-devinterface-usb-hub

        Guid _NewGuid = Guid.Parse("{86e0d1e0-8089-11d0-9ce4-08003e301f73}");
        switch (deviceType)
        {
            case DeviceType.ComPort:
                //https://learn.microsoft.com/en-us/windows-hardware/drivers/install/guid-devinterface-comport
                _NewGuid = Guid.Parse("{86e0d1e0-8089-11d0-9ce4-08003e301f73}");
                break;

            case DeviceType.Android:
                //https://android.googlesource.com/platform/development/+/refs/heads/master/host/windows/usb/android_winusb.inf
                _NewGuid = Guid.Parse("{f72fe0d4-cbcb-407d-8814-9ed673d0dd6b}"); //Dev_AddReg DeviceInterfaceGUIDs
                break;

            case DeviceType.Modem:
                //https://learn.microsoft.com/en-us/windows-hardware/drivers/install/guid-devinterface-modem
                _NewGuid = Guid.Parse("{2C7089AA-2E0E-11D1-B114-00C04FC2AAE4}");
                break;

            case DeviceType.USBDevice:
                //https://learn.microsoft.com/en-us/windows-hardware/drivers/install/guid-devinterface-usb-device
                _NewGuid = Guid.Parse("{a5dcbf10-6530-11d2-901f-00c04fb951ed}"); //USB_DEVICE
                break;

            case DeviceType.HID:
                //https://learn.microsoft.com/en-us/windows-hardware/drivers/install/guid-devinterface-hid
                _NewGuid = Guid.Parse("{4D1E55B2-F16F-11CF-88CB-001111000030}");
                break;

            default:
                break;
        }

        return _NewGuid;
    }

    /// <summary>暂时不获取复合设备</summary>
    /// <param name="deviceType"></param>
    /// <returns></returns>
    public static List<DeviceInfo> GetConnectedDevices(DeviceType deviceType)
    {
        List<DeviceInfo> deviceInfos = new List<DeviceInfo>();

        var guid = GetGuid(deviceType);

        IntPtr hDevInfo = SetupDiGetClassDevs(ref guid, IntPtr.Zero, IntPtr.Zero, DIGCF_DEVICEINTERFACE | DIGCF_PRESENT);
        if (hDevInfo == IntPtr.Zero)
        {
            return deviceInfos; // 无法获取设备列表，返回空
        }

        // 首先枚举所有顶层设备
        int deviceIndex = 0;

        while (true)
        {
            SP_DEVICE_INTERFACE_DATA devIface = new SP_DEVICE_INTERFACE_DATA();
            devIface.cbSize = (uint)(Marshal.SizeOf(devIface));
            // 枚举设备接口
            bool success = SetupDiEnumDeviceInterfaces(hDevInfo, IntPtr.Zero, ref guid, (uint)deviceIndex, ref devIface);

            if (!success)
            {
                int error = Marshal.GetLastWin32Error();
                if (error == ERROR_NO_MORE_ITEMS)
                    break; // 枚举完成
                else
                {
                    deviceIndex++;
                    continue; // 其他错误，尝试下一个设备
                }
            }
            // 获取设备详细信息
            DeviceInfo deviceInfo = GetDeviceDetails(hDevInfo, ref devIface);
            if (deviceInfo != null)
            {
                deviceInfos.Add(deviceInfo);

                // 检查是否为复合设备，如果是则获取其子设备
                //if (IsCompositeDevice(hDevInfo, ref deviceInfo.DevInfoData))
                //{
                //    List<DeviceInfo> childDevices = GetChildDevices(hDevInfo, ref deviceInfo, guid);
                //    deviceInfos.AddRange(childDevices);
                //}
            }
            //每次循环结束时递增设备索引
            deviceIndex++;
        }
        SetupDiDestroyDeviceInfoList(hDevInfo);

        return deviceInfos;
    }

    // 辅助方法：获取设备详细信息
    private static DeviceInfo GetDeviceDetails(IntPtr hDevInfo, ref SP_DEVICE_INTERFACE_DATA devIface)
    {
        try
        {
            // 准备设备接口详情数据结构
            SP_DEVICE_INTERFACE_DETAIL_DATA didd = new SP_DEVICE_INTERFACE_DETAIL_DATA();
            didd.cbSize = (IntPtr.Size == 8) ? 8 : (4 + Marshal.SystemDefaultCharSize);

            SP_DEVINFO_DATA devInfo = new SP_DEVINFO_DATA();
            devInfo.cbSize = (uint)Marshal.SizeOf(devInfo);

            uint requiredSize = 0;

            // 1. 首先调用SetupDiGetDeviceInterfaceDetail来获取所需的缓冲区大小
            var res1 = SetupDiGetDeviceInterfaceDetail(hDevInfo, ref devIface, IntPtr.Zero, 0, out requiredSize, ref devInfo);
            if (!res1)
            {
                //expect that result is false and that error is ERROR_INSUFFICIENT_BUFFER = 122,
                int error = Marshal.GetLastWin32Error();
            }

            IntPtr DeviceInterfaceDetailData = Marshal.AllocHGlobal((int)requiredSize);
            uint size = requiredSize;
            Marshal.WriteInt32(DeviceInterfaceDetailData, (int)size);
            // 2. 然后再次调用SetupDiGetDeviceInterfaceDetail来获取实际的设备路径
            bool success = SetupDiGetDeviceInterfaceDetail(hDevInfo, ref devIface, DeviceInterfaceDetailData, size, out requiredSize, ref devInfo);
            if (!success)
            {
                int error = Marshal.GetLastWin32Error();
            }

            // 收集设备属性
            DeviceInfo deviceInfo = new DeviceInfo
            {
                DevicePath = didd.DevicePath,
                Description = GetDeviceProperty(hDevInfo, ref devInfo, SetupDiGetDeviceRegistryPropertyEnum.SPDRP_DEVICEDESC),
                FriendlyName = GetDeviceProperty(hDevInfo, ref devInfo, SetupDiGetDeviceRegistryPropertyEnum.SPDRP_FRIENDLYNAME),
                LoctionPath = GetDeviceProperty(hDevInfo, ref devInfo, SetupDiGetDeviceRegistryPropertyEnum.SPDRP_LOCATION_PATHS),
                DevInfoData = devInfo // 保存设备信息数据结构用于后续处理
            };

            return deviceInfo;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    // 获取设备属性的辅助方法
    private static string GetDeviceProperty(IntPtr hDevInfo, ref SP_DEVINFO_DATA devInfo, SetupDiGetDeviceRegistryPropertyEnum property)
    {
        uint requiredSize = 0;
        SetupDiGetDeviceRegistryProperty(hDevInfo, ref devInfo, (uint)property, out _, IntPtr.Zero, 0, out requiredSize);

        if (requiredSize == 0)
            return string.Empty;

        IntPtr buffer = IntPtr.Zero;
        try
        {
            buffer = Marshal.AllocHGlobal((int)requiredSize);
            if (SetupDiGetDeviceRegistryProperty(hDevInfo, ref devInfo, (uint)property, out _, buffer, requiredSize, out _))
            {
                return Marshal.PtrToStringAuto(buffer) ?? string.Empty;
            }
        }
        finally
        {
            if (buffer != IntPtr.Zero)
                Marshal.FreeHGlobal(buffer);
        }

        return string.Empty;
    }

    #endregion static_methods
}

public enum DeviceType
{
    ComPort,
    Android,
    Modem,

    /// <summary>android设备</summary>
    USBDevice,

    HID,
}

public class DeviceInfo : ICloneable
{
    public int ComPort { get; set; }

    /// <summary>设备管理器中的【设备描述】</summary>
    public string Description { get; set; }

    /// <summary>设备路径</summary>
    public string DevicePath { get; set; }

    /// <summary>设备管理器中的【友好名称】</summary>
    public string FriendlyName { get; set; }

    /// <summary>位置路径</summary>
    public string LoctionPath { get; set; } = "";

    public SP_DEVINFO_DATA DevInfoData { get; set; }

    public object Clone()
    {
        DeviceInfo other = (DeviceInfo)this.MemberwiseClone();
        return other;
    }
}

/// <summary>Flags for SetupDiGetDeviceRegistryProperty().</summary>
internal enum SetupDiGetDeviceRegistryPropertyEnum : uint
{
    SPDRP_DEVICEDESC = 0x00000000, // DeviceDesc (R/W)
    SPDRP_HARDWAREID = 0x00000001, // HardwareID (R/W)
    SPDRP_COMPATIBLEIDS = 0x00000002, // CompatibleIDs (R/W)
    SPDRP_UNUSED0 = 0x00000003, // unused
    SPDRP_SERVICE = 0x00000004, // Service (R/W)
    SPDRP_UNUSED1 = 0x00000005, // unused
    SPDRP_UNUSED2 = 0x00000006, // unused
    SPDRP_CLASS = 0x00000007, // Class (R--tied to ClassGUID)
    SPDRP_CLASSGUID = 0x00000008, // ClassGUID (R/W)
    SPDRP_DRIVER = 0x00000009, // Driver (R/W)
    SPDRP_CONFIGFLAGS = 0x0000000A, // ConfigFlags (R/W)
    SPDRP_MFG = 0x0000000B, // Mfg (R/W)
    SPDRP_FRIENDLYNAME = 0x0000000C, // FriendlyName (R/W)
    SPDRP_LOCATION_INFORMATION = 0x0000000D, // LocationInformation (R/W)
    SPDRP_PHYSICAL_DEVICE_OBJECT_NAME = 0x0000000E, // PhysicalDeviceObjectName (R)
    SPDRP_CAPABILITIES = 0x0000000F, // Capabilities (R)
    SPDRP_UI_NUMBER = 0x00000010, // UiNumber (R)
    SPDRP_UPPERFILTERS = 0x00000011, // UpperFilters (R/W)
    SPDRP_LOWERFILTERS = 0x00000012, // LowerFilters (R/W)
    SPDRP_BUSTYPEGUID = 0x00000013, // BusTypeGUID (R)
    SPDRP_LEGACYBUSTYPE = 0x00000014, // LegacyBusType (R)
    SPDRP_BUSNUMBER = 0x00000015, // BusNumber (R)
    SPDRP_ENUMERATOR_NAME = 0x00000016, // Enumerator Name (R)
    SPDRP_SECURITY = 0x00000017, // Security (R/W, binary form)
    SPDRP_SECURITY_SDS = 0x00000018, // Security (W, SDS form)
    SPDRP_DEVTYPE = 0x00000019, // Device Type (R/W)
    SPDRP_EXCLUSIVE = 0x0000001A, // Device is exclusive-access (R/W)
    SPDRP_CHARACTERISTICS = 0x0000001B, // Device Characteristics (R/W)
    SPDRP_ADDRESS = 0x0000001C, // Device Address (R)
    SPDRP_UI_NUMBER_DESC_FORMAT = 0X0000001D, // UiNumberDescFormat (R/W)
    SPDRP_DEVICE_POWER_DATA = 0x0000001E, // Device Power Data (R)
    SPDRP_REMOVAL_POLICY = 0x0000001F, // Removal Policy (R)
    SPDRP_REMOVAL_POLICY_HW_DEFAULT = 0x00000020, // Hardware Removal Policy (R)
    SPDRP_REMOVAL_POLICY_OVERRIDE = 0x00000021, // Removal Policy Override (RW)
    SPDRP_INSTALL_STATE = 0x00000022, // Device Install State (R)
    SPDRP_LOCATION_PATHS = 0x00000023, // Device Location Paths (R)
    SPDRP_BASE_CONTAINERID = 0x00000024  // Base ContainerID (R)
}