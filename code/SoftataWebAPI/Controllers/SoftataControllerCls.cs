using Microsoft.AspNetCore.Mvc;
using Softata.Enums;
using SoftataWebAPI.Data;
using SoftataWebAPI.Data.Db;
using System.Net.Sockets;

namespace SoftataWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SoftataControllerCls : ControllerBase
    {
        #region
        public readonly SoftataDbContext softataContext;
        public readonly ISoftataGenCmds sharedService;

        public SoftataControllerCls(SoftataDbContext softataContext, ISoftataGenCmds sharedService)
        {
            this.softataContext = softataContext;
            this.sharedService = sharedService;
        }


        protected Softata.SoftataLib softatalib
        {
            get
            {
                return sharedService.GetSoftata(HttpContext);
            }
        }
        protected Socket? _Connect(string ipAddress, int _port)
        {

            //softatalib softatalib = Getsoftatalib();
            Socket? client = softatalib.Connect(ipAddress, _port);
            if (client != null)
            {
                if (client.Connected)
                {
                    var connection = new Tuple<string, int>(ipAddress, _port);
                    HttpContext.Session.Set<Tuple<string, int>>("ConnectionDetails", connection);
                    return client;
                }
            }
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
