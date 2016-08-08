//using EloBuddy.Sandbox;
//using EloBuddy.Sandbox.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using System.ServiceModel;

namespace LeagueSharp.Common
{
    class SandboxConfig
    {
        // Token: 0x0400002C RID: 44
        public static int MenuKey;

        // Token: 0x0400002D RID: 45
        public static int MenuToggleKey;
        // Token: 0x04000013 RID: 19

        static SandboxConfig()
        {
            MenuKey = 16;
            MenuToggleKey = 45;
     //       Reload();
        }
        /*
        public static void Reload()
        {
            Configuration configuration = null;
            try
            {
            //    configuration = CreateProxy<ILoaderService>().GetConfiguration(Pid);
            }
            catch (Exception ex)
            {
//				\u206C\u200F\u202A\u200B\u202B\u200E\u206B\u202D\u206E\u202B\u202C\u200D\u206B\u202D\u200E\u200C\u202A\u200F\u206A\u202D\u202C\u202C\u206E\u206A\u206A\u206E\u206F\u200C\u200B\u200D\u200F\u206D\u200F\u206B\u206A\u206B\u200B\u200E\u200E\u206A\u202E.\u202C\u206A\u206A\u206A\u202A\u202A\u200F\u206B\u202A\u200F\u200B\u202B\u206D\u206D\u206F\u206D\u200C\u200F\u206B\u200B\u202A\u206C\u202A\u206A\u200E\u200B\u206B\u200B\u206F\u202E\u206D\u206E\u200F\u200F\u200F\u200D\u202A\u200C\u206F\u206A\u202E("Sandbox: Reload, getting configuration failed", new object[0]);
//				\u206C\u200F\u202A\u200B\u202B\u200E\u206B\u202D\u206E\u202B\u202C\u200D\u206B\u202D\u200E\u200C\u202A\u200F\u206A\u202D\u202C\u202C\u206E\u206A\u206A\u206E\u206F\u200C\u200B\u200D\u200F\u206D\u200F\u206B\u206A\u206B\u200B\u200E\u200E\u206A\u202E.\u202C\u206A\u206A\u206A\u202A\u202A\u200F\u206B\u202A\u200F\u200B\u202B\u206D\u206D\u206F\u206D\u200C\u200F\u206B\u200B\u202A\u206C\u202A\u206A\u200E\u200B\u206B\u200B\u206F\u202E\u206D\u206E\u200F\u200F\u200F\u200D\u202A\u200C\u206F\u206A\u202E(ex.ToString(), new object[0]);
            }
            if (configuration != null)
            {
                SandboxConfig.MenuKey = configuration.MenuKey;
                SandboxConfig.MenuToggleKey = configuration.MenuToggleKey;              
            }
            else
            {
//				\u206C\u200F\u202A\u200B\u202B\u200E\u206B\u202D\u206E\u202B\u202C\u200D\u206B\u202D\u200E\u200C\u202A\u200F\u206A\u202D\u202C\u202C\u206E\u206A\u206A\u206E\u206F\u200C\u200B\u200D\u200F\u206D\u200F\u206B\u206A\u206B\u200B\u200E\u200E\u206A\u202E.\u202C\u206A\u206A\u206A\u202A\u202A\u200F\u206B\u202A\u200F\u200B\u202B\u206D\u206D\u206F\u206D\u200C\u200F\u206B\u200B\u202A\u206C\u202A\u206A\u200E\u200B\u206B\u200B\u206F\u202E\u206D\u206E\u200F\u200F\u200F\u200D\u202A\u200C\u206F\u206A\u202E("Sandbox: Reload, config is null", new object[0]);
			}
		}
        internal static int Pid
        {
            // Token: 0x06000026 RID: 38 RVA: 0x00003868 File Offset: 0x00001A68
            get
            {
                return Process.GetCurrentProcess().Id;
            }
        }

        public static TInterfaceType CreateProxy<TInterfaceType>() where TInterfaceType : class
        {
            TInterfaceType result;
            try
            {
                result = new ChannelFactory<TInterfaceType>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/EloBuddy" + typeof(TInterfaceType).Name)).CreateChannel();
            }
            catch (Exception innerException)
            {
                throw new Exception("Failed to connect to assembly pipe for communication. The targetted assembly may not be loaded yet. Desired interface: " + typeof(TInterfaceType).Name, innerException);
            }
            return result;
        }

        public interface ILoaderService
        {
            // Token: 0x06000077 RID: 119
            Configuration GetConfiguration(int pid);

            // Token: 0x06000078 RID: 120
            void Recompile(int pid);
        }
        */
    }
}
