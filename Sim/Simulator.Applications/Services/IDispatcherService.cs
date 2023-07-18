using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyDLab.Usv.Simulator.Applications.Services
{
	public interface IDispatcherService
	{
		void Invoke(Action action);
	}
}
