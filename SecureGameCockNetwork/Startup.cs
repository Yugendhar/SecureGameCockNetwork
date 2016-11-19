using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Owin;

namespace SecureGameCockNetwork
{
    public partial class Startup
    {

        public void Configuration(IAppBuilder app)
        {   
            //Mapping SignalR to begin connection to hubs from this application
            app.MapSignalR();
            ConfigureAuth(app);
        }
    }
}