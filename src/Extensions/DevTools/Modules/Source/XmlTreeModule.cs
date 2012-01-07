namespace Sage.DevTools.Modules.Source
{
	using System;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Xml;

	using Sage.Views;

	using log4net;
	using Sage.Modules;

	public class XmlTreeModule : IModule
	{
		private static readonly XmlNamespaceManager nm = XmlNamespaces.Manager;
		private static readonly ILog log = LogManager.GetLogger(typeof(XmlTreeModule).FullName);

		public ModuleResult ProcessRequest(XmlElement moduleElement, ViewConfiguration configuration)
		{
			Contract.Requires<ArgumentNullException>(moduleElement != null);
			Contract.Requires<ArgumentNullException>(configuration != null);

			SageContext context = configuration.Context;
			ModuleResult result = new ModuleResult(moduleElement);

			ModuleResultStatus status = ModuleResultStatus.None;

			XmlNode pathNode = moduleElement.SelectSingleNode("mod:config/mod:source", nm);
			XmlNode selectionNode = moduleElement.SelectSingleNode("mod:config/mod:xpath", nm);
			if (pathNode != null)
			{
				string path = pathNode.InnerText.Trim();
				if (string.IsNullOrEmpty(path))
				{
					log.WarnFormat("The path element shouldn't be empty.");
					status = ModuleResultStatus.ModuleWarning;
				}
				else
				{
					path = context.Path.Substitute(path);

					if (!Path.IsPathRooted(path))
					{
						string configDirectory = Path.GetDirectoryName(configuration.Info.ConfigPath);
						path = Path.Combine(configDirectory, path);
					}

					string resolved = context.Path.Resolve(path);
					if (!File.Exists(resolved))
					{
						log.WarnFormat("The specified xml path '{0}' ('{1}') doesn't exist", path, resolved);
						status = ModuleResultStatus.ModuleWarning;
					}
					else
					{
						try
						{
							XmlNode selection = context.Resources.LoadXml(resolved);
							if (selectionNode != null)
							{
								string xpath = selectionNode.InnerText.Trim();
								if (string.IsNullOrEmpty(xpath))
								{
									log.WarnFormat("The selection element shouldn't be empty.");
									status = ModuleResultStatus.ModuleWarning;
								}
								else
								{
									try
									{
										selection = selection.SelectSingleNode(xpath);
										if (selection == null)
										{
											log.WarnFormat("The xpath expression '{0}' yielded not result.", xpath);
											status = ModuleResultStatus.ModuleWarning;
										}
									}
									catch (Exception ex)
									{
										log.ErrorFormat("Attempting to select using xpath expression '{0}' resulted in error: {1}.",
											xpath, ex.Message);

										status = ModuleResultStatus.ModuleError;
									}
								}
							}

							if (status == ModuleResultStatus.None)
							{
								result.AppendDataElement(selection);
								status = ModuleResultStatus.Ok;
							}
						}
						catch (Exception ex)
						{
							log.ErrorFormat("Failed to add specified file '{0}' ('{1}') as XML document: {2}", 
								path, resolved, ex.Message);

							status = ModuleResultStatus.ModuleError;
						}
					}
				}
			}

			result.Status = status;
			return result;
		}
	}
}
