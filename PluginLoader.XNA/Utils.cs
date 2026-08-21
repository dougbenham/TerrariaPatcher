using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;

namespace PluginLoader
{
    public static class Utils
    {
        #region UAC / Admin / Elevated

        private const string uacRegistryKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
        private const string uacRegistryValue = "EnableLUA";

        private static uint STANDARD_RIGHTS_READ = 0x00020000;
        private static uint TOKEN_QUERY = 0x0008;
        private static uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool GetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, uint TokenInformationLength, out uint ReturnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        public enum TOKEN_INFORMATION_CLASS
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin,
            TokenElevationType,
            TokenLinkedToken,
            TokenElevation,
            TokenHasRestrictions,
            TokenAccessInformation,
            TokenVirtualizationAllowed,
            TokenVirtualizationEnabled,
            TokenIntegrityLevel,
            TokenUIAccess,
            TokenMandatoryPolicy,
            TokenLogonSid,
            MaxTokenInfoClass
        }

        public enum TOKEN_ELEVATION_TYPE
        {
            TokenElevationTypeDefault = 1,
            TokenElevationTypeFull,
            TokenElevationTypeLimited
        }

        public static bool IsUacEnabled
        {
            get
            {
                using (RegistryKey uacKey = Registry.LocalMachine.OpenSubKey(uacRegistryKey, false))
                {
                    object result = uacKey?.GetValue(uacRegistryValue);
                    return result != null && result.Equals(1);
                }
            }
        }

        public static bool IsProcessElevated
        {
            get
            {
                if (!IsUacEnabled)
                {
                    try
                    {
                        using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                            return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
                    }
                    catch
                    {
                        return false;
                    }
                }

                if (!OpenProcessToken(Process.GetCurrentProcess().Handle, TOKEN_READ, out var tokenHandle))
                {
                    throw new ApplicationException("Could not get process token.  Win32 Error Code: " + Marshal.GetLastWin32Error());
                }

                IntPtr elevationTypePtr = IntPtr.Zero;
                try
                {
                    int elevationResultSize = Marshal.SizeOf(typeof(int));
                    elevationTypePtr = Marshal.AllocHGlobal(elevationResultSize);

                    if (!GetTokenInformation(tokenHandle, TOKEN_INFORMATION_CLASS.TokenElevationType, elevationTypePtr, (uint) elevationResultSize, out _))
                    {
                        throw new ApplicationException("Unable to determine the current elevation.");
                    }

                    return (TOKEN_ELEVATION_TYPE) Marshal.ReadInt32(elevationTypePtr) == TOKEN_ELEVATION_TYPE.TokenElevationTypeFull;
                }
                finally
                {
                    if (elevationTypePtr != IntPtr.Zero)
                        Marshal.FreeHGlobal(elevationTypePtr);
                    CloseHandle(tokenHandle);
                }
            }
        }

        #endregion

        public static bool IsFolderWritable(string folder)
        {
            try
            {
                var probe = Path.Combine(folder, Path.GetRandomFileName());
                using (new FileStream(probe, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose))
                    return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsFileLocked(FileInfo file)
        {
            if (file == null || 
                !file.Exists) return false;

            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        public static bool IstModLoaderInstalled()
        {
            return System.Reflection.Assembly.GetEntryAssembly().GetType("Terraria.ModLoader.Mod") != null;
        }
    }
}
