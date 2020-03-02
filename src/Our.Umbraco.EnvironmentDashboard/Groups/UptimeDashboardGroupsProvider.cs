using System;
using System.Collections.Generic;
using Our.Umbraco.EnvironmentDashboard.Models;

namespace Our.Umbraco.EnvironmentDashboard.Groups
{
	public class UptimeDashboardGroupsProvider : IDashboardGroupsProvider
	{
		public IEnumerable<InfoGroup> GetGroups(DashboardEnvironment environment)
		{
			var infoGroup = new InfoGroup("Server Information");
			infoGroup.Pairs.Add(new InfoPair("Machine Name", Environment.MachineName));
			infoGroup.Pairs.Add(new InfoPair("Processor Core Count", Environment.ProcessorCount.ToString("D")));
			infoGroup.Pairs.Add(new InfoPair("Uptime", TimeSpan.FromMilliseconds(Environment.TickCount).ToString("c")));

			return new[] { infoGroup };
		}
	}
}