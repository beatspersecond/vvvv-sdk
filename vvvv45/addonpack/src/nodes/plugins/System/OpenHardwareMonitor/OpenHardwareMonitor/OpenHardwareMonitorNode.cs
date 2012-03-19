/*
 * Created by SharpDevelop.
 * User: buhlert
 * Date: 19.03.2012
 * Time: 22:14
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using OpenHardwareMonitor.Collections;
using OpenHardwareMonitor.Hardware;


using VVVV.Core.Logging;
#endregion usings


namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Monitor",
	            Category = "System",
	            Author = "Phlegma",
	            Help = "Read OS Informations with OpenHardwareMonitoring",
	            Tags = "OpenHardwareMonitor, Monitoring, System, CPU, Fan"
	           )]
	#endregion PluginInfo
	public class OpenHardwareMonitorNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Update", DefaultValue = 0)]
		IDiffSpread<bool> FUpdate;

		[Output("Output")]
		ISpread<string> FOutput;

		[Import()]
		ILogger FLogger;
		
		private Computer FComputer = new Computer();
		private IHardware FHardware;
		#endregion fields & pins
		
		
 
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(FUpdate.IsChanged)
			{
				FOutput.SliceCount = FComputer.Hardware.Length;
				
				FComputer.Open();
				int counter = 0;
				foreach(IHardware Hardware in FComputer.Hardware)
				{
					Identifier Ident = Hardware.Identifier;
					FOutput[counter] = Ident.ToString();
					counter++;
				}
			}
		}
	}
}