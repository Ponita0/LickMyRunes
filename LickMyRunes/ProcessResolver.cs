using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace LickMyRunes
{
	public static class ProcessResolver
	{
		public static string GetCommandLine(Process process)
		{
			string result;
			using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(string.Format("SELECT CommandLine FROM Win32_Process WHERE ProcessId = {0}", process.Id)))
			{
				using (ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get())
				{
					ManagementBaseObject managementBaseObject = managementObjectCollection.Cast<ManagementBaseObject>().SingleOrDefault<ManagementBaseObject>();
					string text;
					if (managementBaseObject == null)
					{
						text = null;
					}
					else
					{
						object obj = managementBaseObject["CommandLine"];
						text = ((obj != null) ? obj.ToString() : null);
					}
					result = text;
				}
			}
			return result;
		}
		public static Process[] GetProcessesByName(string name)
		{
			return Process.GetProcessesByName(name);
		}
	}
}
