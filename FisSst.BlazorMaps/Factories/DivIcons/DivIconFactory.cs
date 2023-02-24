using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FisSst.BlazorMaps {
	internal class DivIconFactory : IDivIconFactory {
		private const string Create = "L.divIcon";
		private readonly IJSRuntime JSRuntime;

		public DivIconFactory(IJSRuntime jsRuntime) {
			JSRuntime = jsRuntime;
		}

		public async Task<Icon> CreateAsync(DivIconOptions options)
			=> new Icon(await JSRuntime.InvokeAsync<IJSObjectReference>(Create, options));
	}
}