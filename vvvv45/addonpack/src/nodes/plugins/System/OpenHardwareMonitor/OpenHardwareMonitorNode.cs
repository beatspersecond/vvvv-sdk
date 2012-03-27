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
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

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
		
		[Output("Hardware")]
		ISpread<string> FHardware;

		[Output("Identifier")]
		ISpread<string> FIdentifier;
		
		[Output("Name")]
		ISpread<string> FName;
		
		[Output("Value")]
		ISpread<float> FValue;

		[Import()]
		ILogger FLogger;
		
		private Computer FComputer = new Computer();
		private SortedList<String,List<ISensor>> FInstances = new SortedList<String, List<ISensor>>();
		private bool FInit = true;
		#endregion fields & pins
		
		
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(FInit)
			{
				FComputer.Open();
				FInit = false;
			}
			
			if(FUpdate.IsChanged && FUpdate[0] == true)
			{
				ReadComputerHardware();
				int Counter = 0;
				IList<List<ISensor>> SensorLists= FInstances.Values;
				foreach(List<ISensor> List in SensorLists)
				{
					FIdentifier.SliceCount = FHardware.SliceCount = FValue.SliceCount = FName.SliceCount = Counter + List.Count;
					foreach(ISensor Sensor in List)
					{
						Sensor.Hardware.Update();
						FHardware[Counter] = Sensor.Hardware.Name;
						FIdentifier[Counter] = Sensor.Identifier.ToString();
						FName[Counter] = Sensor.Name;
						FValue[Counter] = (float) Sensor.Value;
						Counter++;
					}
				}
			}
		}
		
		private void ReadComputerHardware()
		{
			FInstances.Clear();
			foreach (IHardware Hardware in FComputer.Hardware)
				ComputerHardwareAdded(Hardware);
		}
		
		private void ComputerHardwareAdded(IHardware Hardware)
		{
			if (!Exists(Hardware.Identifier.ToString()))
			{
				List<ISensor> SensorList = new List<ISensor>();
				foreach (ISensor Sensor in Hardware.Sensors)
					SensorList.Add(Sensor);

				FInstances.Add(Hardware.Identifier.ToString(),SensorList);

			}

			foreach (IHardware subHardware in Hardware.SubHardware)
				ComputerHardwareAdded(subHardware);
		}
		
		private bool Exists(string Identifier) {
			return FInstances.ContainsKey(Identifier);
		}
	}
}