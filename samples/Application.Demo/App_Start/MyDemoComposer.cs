using System.Collections.Generic;
using Our.Umbraco.EnvironmentDashboard;
using Our.Umbraco.EnvironmentDashboard.Components;
using Our.Umbraco.EnvironmentDashboard.Groups;
using Umbraco.Core.Composing;

namespace Application.Demo
{
	public class MyDemoComposer : IUserComposer
	{
		public void Compose(Composition composition)
		{
			composition.AddEnvironmentDashboard(new Dictionary<DashboardEnvironment, IEnumerable<string>>
			            {
				            {new DashboardEnvironment("Local"), new[] {"localhost:21801"}},
				            {new DashboardEnvironment("Development"), new[] {"environmentdashboard-dev.ry.com"}},
				            {new DashboardEnvironment("QA"), new[] {"environmentdashboard-qa.ry.com"}},
				            {new DashboardEnvironment("UAT"), new[] {"environmentdashboard-uat.ry.com"}},
				            {new DashboardEnvironment("Production"), new[] {"environmentdashboard.ry.com"}},
			            })
			           .Append<UptimeDashboardGroupsProvider>()
			           .Append(f => new DatabaseServerGroupsProvider());

		}
	}
}