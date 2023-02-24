using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FisSst.BlazorMaps {
	public interface IDivIconFactory {
		Task<Icon> CreateAsync(DivIconOptions options);
	}
}