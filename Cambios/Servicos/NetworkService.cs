﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cambios.Servicos
{
    public class NetworkService
    {
        public Response CheckConnection()
        {
            var client = new WebClient();

            try
            {
                //Verifica se existe conecção a internet
                using (client.OpenRead("http://clients3.google.com/generate_204"))
                {
                    return new Response
                    {
                        IsSuccess = true
                    };
                }
            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Configure a sua ligação á Internet",
                };
            }
        }

    }
}
