﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LiveCodingTV.Server.Models;
using Newtonsoft.Json;

namespace LiveCodingTV.Server {
    public class APIAccess {
        WebClient wc = new WebClient();
        public Request req;

        private string _bearer;
        public  string Bearer { get { return _bearer; } }

        public APIAccess(Request request) {
            this.req = request;

            // Okay the token we originally setup 
            // has become invalid
            if(!isValid()) RefreshToken();
        }

        public bool isValid() {
            if (req.getGUIDState() != GUID_Status.STATE_ID_VALID)
                return false;

            // TODO: Do a test on a light weight api element
            // check response!

            return false;
        }

        public MODEL_Bearer_Response getBearerCode() {
            string  url = req.getServerResources().LOCATION_APP_TOKEN_GET;
                    url += "?g=" + req.GUID;
            string  json = wc.DownloadString(url);
            var     obj = JsonConvert.DeserializeObject<MODEL_Bearer_Response>(json);

            if(!obj.Error)
                _bearer = obj.Token;

            return obj;
        }

        public MODEL_Refresh_TOKEN RefreshToken() {
            // Check if a refresh is really required!
            if (isValid()) return new MODEL_Refresh_TOKEN() { status = MODEL_Refresh_TOKEN.State.Success, message = "Everything works fine, your just crazy!" };

            // Check if the guid as expired
            var state = req.getGUIDState();
            if(state == GUID_Status.STATE_ID_NOTFOUND) 
                return new MODEL_Refresh_TOKEN() { status = MODEL_Refresh_TOKEN.State.Remake, message = "The token and guid have expired," };

            // Attempt to refresh the token!
            string  url = req.getServerResources().LOCATION_APP_TOKEN_REFRESH;
                    url += "?g=" + req.GUID;
            string  json = wc.DownloadString(url);
            var     obj = JsonConvert.DeserializeObject<MODEL_Refresh_Response>(json);

            if (obj.status == MODEL_Refresh_Response.Status.SUCCESS)
                 return new MODEL_Refresh_TOKEN() { status = MODEL_Refresh_TOKEN.State.Success, message = "Everything went swell!" };
            else return new MODEL_Refresh_TOKEN() { status = MODEL_Refresh_TOKEN.State.Retry, message = "There was a problem refreshing your token!" };
        }

    }
}
