using Microsoft.AspNetCore.Mvc;
using Softata.Enums;
using SoftataWebAPI.Data;
using SoftataWebAPI.Data.Db;
using System.Net.Sockets;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SoftataWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SoftataControllerCls : ControllerBase
    {
        #region
        public readonly SoftataDbContext softataContext;
        public readonly ISoftataGenCmds sharedService;

        public SoftataControllerCls(SoftataDbContext softataContext, ISoftataGenCmds _sharedService)
        {
            this.softataContext = softataContext;
            this.sharedService = _sharedService;
        }


        protected Softata.SoftataLib softatalib
        {
            get { 
                var x =  sharedService.GetSoftata(HttpContext);               
                return x ;
            }
            set
            {
                sharedService.SetSoftata(HttpContext, value);
            }
        }
        protected Socket? _Connect(string ipAddress, int _port)
        {
            Socket? client = null;
            var conn = HttpContext.Session.Get<Tuple<string, int>>("ConnectionDetails");
            if(conn!= null)
            {
                bool good = false;
                if (conn.Item1== ipAddress)
                {
                    if (conn.Item1 == ipAddress)
                    {
                        if (conn.Item2 == _port)
                        {
                            good = true;
                            System.Diagnostics.Debug.WriteLine($"Reusing existing connection details {conn.Item1}.{conn.Item2} from session.");
                        }
                    }
                }
                if (!good)
                {
                    System.Diagnostics.Debug.WriteLine($"Fail to reuse existing connection details: {conn.Item1}.{conn.Item2} from session. Called with : {ipAddress}.{_port}");

                }
            }
            
            //softatalib softatalib = Getsoftatalib();
            client = softatalib.Connect(ipAddress, _port);
            if (client != null)
            {
                if (client.Connected)
                {
                    var connectionDetails = new Tuple<string, int>(ipAddress, _port);
                    HttpContext.Session.Set<Tuple<string, int>>("ConnectionDetails", connectionDetails);

                    System.Diagnostics.Debug.WriteLine("Connected to Softata device at " + ipAddress + ":" + _port);
                    return client;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Already connected to Softata device at " + ipAddress + ":" + _port);
                    return client;

                }
            }
            System.Diagnostics.Debug.WriteLine("No connection to Softata device at " + ipAddress + ":" + _port);
            return null;
        }

        public Socket? _client = null;
        protected Socket? Client
        {
            get
            {
                if (_client != null)
                {
                    return _client;
                }
                else
                {
                    Tuple<string, int>? connectionDetails = HttpContext.Session.Get<Tuple<string, int>>("ConnectionDetails");
                    if (connectionDetails == null)
                        return null;


                    _client = _Connect(connectionDetails.Item1, connectionDetails.Item2);
                    return _client;
                }

            }
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
    }
}
