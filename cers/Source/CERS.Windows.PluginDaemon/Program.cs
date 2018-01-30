using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using CERS.Plugins;
using UPF;

namespace CERS.Windows.PluginDaemon
{
	internal class Program
	{
		internal static string PluginsToRun
		{
			get { return ConfigurationManager.AppSettings["PluginsToRun"]; }
		}

		private static void Main(string[] args)
		{
			var plugins = GetPluginDefinitionsToRun();
			PluginInvoker.RunBatch(plugins);
		}

		private static List<PluginDefinition> GetPluginDefinitionsToRun()
		{
			List<PluginDefinition> results = new List<PluginDefinition>();
			var plugins = PluginUtilities.GetAvailablePluginsFromAppSettings("PluginAssemblies");

			if (!string.IsNullOrWhiteSpace(PluginsToRun))
			{
				var targetPlugins = PluginsToRun.ToList(';');
				results.AddRange(
					from r in plugins where targetPlugins.Contains(r.Type.Name) select r
					);
			}

			return results;
		}
	}
}