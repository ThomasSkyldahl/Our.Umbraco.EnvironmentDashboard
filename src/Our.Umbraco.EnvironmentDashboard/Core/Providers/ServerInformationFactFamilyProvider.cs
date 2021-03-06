﻿using Our.Umbraco.EnvironmentDashboard.Core.Models;
using System;

namespace Our.Umbraco.EnvironmentDashboard.Core.Providers
{
	public class ServerInformationFactFamilyProvider : IFactFamilyProvider
	{
		private readonly string _machineName;
		private readonly int _tickCount;

		public ServerInformationFactFamilyProvider(string machineName, int tickCount)
		{
			_machineName = machineName;
			_tickCount = tickCount;
		}

		public ServerInformationFactFamilyProvider() : this(Environment.MachineName, Environment.TickCount)
		{ }


		public FactFamily Build(string environment)
		{
			var factFamily = new FactFamily("Server Information");
			factFamily.Pairs.Add(new Fact("Machine Name", _machineName));
			//factFamily.Pairs.Add(new Fact("Processor Core Count", _processorCount.ToString("D")));
			factFamily.Pairs.Add(new Fact("Uptime", TimeSpan.FromMilliseconds(_tickCount).ToString("c")));

			return factFamily;
		}
	}
}