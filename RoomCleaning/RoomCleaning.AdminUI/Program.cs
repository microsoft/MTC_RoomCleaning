using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RoomCleaning.AdminUI.Services;
using Blazor.ModalDialog;

namespace RoomCleaning.AdminUI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            
            
            var baseApi_Uri = new Uri(builder.Configuration["baseApi_Uri"]);
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = baseApi_Uri });
            builder.Services.AddHttpClient<RoomPolicyService>(service => service.BaseAddress = baseApi_Uri);
            builder.Services.AddModalDialog();

            await builder.Build().RunAsync();
        }
    }
}
