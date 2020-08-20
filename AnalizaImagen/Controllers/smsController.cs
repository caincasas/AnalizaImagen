using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Twilio.AspNet.Common;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;
using Task = System.Threading.Tasks.Task;

namespace AnalizaImagen.Controllers
{
    public class smsController : TwilioController
    {
        // GET: sms
        [HttpPost]
        public async Task<TwiMLResult> Index(SmsRequest request, int numMedia)
        {
            string respuesta = "";
            var response = new MessagingResponse();
            string urlImagen = numMedia > 0 ? Request.Form[$"MediaUrl0"] : "";
            if (urlImagen == "")
            {
                respuesta = "Por favor envía una imagen para analizar. 🤖👀 \n\n" +
                    " Es importante que la mandes para poder analizarla. Recuerda que deben aparecer personas.";
            }
            else
            {
                respuesta = "Url de imagen: \n" + urlImagen;
            }
            response.Message(respuesta);
            return TwiML(response);
        }
    }
}