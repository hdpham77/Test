using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using CERS.Plugins;
using UPF;
using UPF.Core;

namespace CERS.TaskManager.Windows
{
	public class Utilities
	{
		public static void RunPlugin(PluginDefinitionSelection plugin, EventHandler<PluginNotificationEventArgs> notificationEvent = null, EventHandler<PluginProgressChangedEventArgs> progressChangedEvent = null)
		{
			int contextAccountID = Constants.DefaultAccountID;
			if (!plugin.Arguments.Contains(Plugin.ContextAccountIDArgumentKey))
			{
				contextAccountID = int.Parse(ConfigurationManager.AppSettings["ContextAccountID"]);
			}
			PluginInvoker.Run(plugin, contextAccountID, notificationEvent, progressChangedEvent);
		}

		public static List<string> GetPluginAssembliesFromConfig()
		{
			string assemblyNames = ConfigurationManager.AppSettings.GetValue("PluginAssemblies", string.Empty);
			List<string> assemblies = new List<string>(assemblyNames.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
			return assemblies;
		}

		public static List<PluginDefinitionSelection> GetAvailablePlugins()
		{
			List<string> assemblies = GetPluginAssembliesFromConfig();
			var plugins = PluginUtilities.GetPlugins(assemblies);
			List<PluginDefinitionSelection> availablePlugins = (from p in plugins
																select new PluginDefinitionSelection
																{
																	Name = p.Name,
																	Type = p.Type,
																	Selected = false,
																	Description = p.Description,
																	Arguments = p.DefaultArguments,
																	Order = p.Order,
																	ArgumentOptions = p.ArgumentOptions,
																	DeveloperName = p.DeveloperName,
																}).OrderBy(p => p.Order).ThenBy(p => p.Name).ToList();

			return availablePlugins;
		}
	}
}