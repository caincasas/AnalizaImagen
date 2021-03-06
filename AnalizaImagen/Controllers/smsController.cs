﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
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
                //respuesta = "Url de imagen: \n" + urlImagen;
                string URL = "https://api.clarifai.com/v2/models/c0c0ac362b03416da06ab3fa36fb58e3/outputs";
                string DATA = "{" +
                        @"   ""inputs"": [" +
                        @" 	{" +
                        @" 	  ""data"": {" +
                        @" 		""image"": {" +
                        @" 		  ""url"": """ + urlImagen + @""" " +
                        " 		}" +
                        " 	  }" +
                        " 	}" +
                        "   ]" +
                        " }";
                string r = api(URL, DATA);
                dynamic jsonObj = JsonConvert.DeserializeObject(r, typeof(object));
                string status = jsonObj.status.description.ToString();//estatus del api
                if(status == "Ok")
                {
                    string data = jsonObj.outputs[0].data.ToString();
                    //string dd = data.Length >= 600 ? data.Remove(600) : data;
                    //respuesta = "datos <" + dd + ">";
                    if (data != "{}")
                    {
                        string edad = "";
                        double edadPorcent = 0;
                        //string edad = jsonObj.outputs[0].data.regions[0].data.concepts[0].name.ToString();
                        //double edadPorcent = Convert.ToDouble(jsonObj.outputs[0].data.regions[0].data.concepts[0].value.ToString()) * 100;
                        //edadPorcent = Math.Truncate(edadPorcent * 100) / 100;
                        //respuesta = $"Resultados: \n\nEdad: *{edad}* \nExactitud: *{edadPorcent}%*";
                        int elementos = jsonObj.outputs[0].data.regions[0].data.concepts.Count;
                        string cultura = "";
                        double culturaPorcet = 0;
                        string sexo = "";
                        double sexoPorcet = 0;
                        for (int i = 0; i < elementos; i++)
                        {
                            if(jsonObj.outputs[0].data.regions[0].data.concepts[i].vocab_id.ToString() == "age_appearance")//verifico apariencia de edad
                            {
                                double actualPorcent = Convert.ToDouble(jsonObj.outputs[0].data.regions[0].data.concepts[i].value.ToString());
                                //edadPorcent = edadPorcent > actualPorcent ? edadPorcent : actualPorcent;
                                if(edadPorcent < actualPorcent)
                                {
                                    edadPorcent = actualPorcent;
                                    edad = jsonObj.outputs[0].data.regions[0].data.concepts[i].name.ToString();
                                }
                            }
                            else if (jsonObj.outputs[0].data.regions[0].data.concepts[i].vocab_id.ToString() == "gender_appearance")//verifico sexo
                            {
                                double actualPorcent = Convert.ToDouble(jsonObj.outputs[0].data.regions[0].data.concepts[i].value.ToString());
                                if (sexoPorcet < actualPorcent)
                                {
                                    sexoPorcet = actualPorcent;
                                    sexo = jsonObj.outputs[0].data.regions[0].data.concepts[i].name.ToString();
                                }
                            }
                            else if (jsonObj.outputs[0].data.regions[0].data.concepts[i].vocab_id.ToString() == "multicultural_appearance")//verifico cultura
                            {
                                double actualPorcent = Convert.ToDouble(jsonObj.outputs[0].data.regions[0].data.concepts[i].value.ToString());
                                if (culturaPorcet < actualPorcent)
                                {
                                    culturaPorcet = actualPorcent;
                                    cultura = jsonObj.outputs[0].data.regions[0].data.concepts[i].name.ToString();
                                }
                            }
                        }
                        edadPorcent = Math.Truncate((edadPorcent*100) * 100) / 100;//Se redondea con 2 dijitos
                        culturaPorcet = Math.Truncate((culturaPorcet*100) * 100) / 100;//Se redondea con 2 dijitos
                        sexoPorcet = Math.Truncate((sexoPorcet * 100) * 100) / 100;

                        respuesta = $"Resultados: \n\nEdad: *{edad}* \nExactitud: *{edadPorcent}%* \n\nCultura: *{cultura}* \nExactitud: *{culturaPorcet}%* \n\nSexo: {sexo} \nExactitud: {sexoPorcet}";
                        //respuesta = "Total de elementos: *" + elementos + "*";
                    }
                    else
                    {
                        respuesta = "No encontre personas en la imagen 😓😭😪";
                    }
                }
                else
                {
                    respuesta = "Lo siento ocurrio un error al ejecutar el API.";
                }
                //respuesta = status.Length >= 600 ? status.Remove(600) : status;
            }
            response.Message(respuesta);
            return TwiML(response);
        }
        public string api(string URL, string DATA)
        {
            string re = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.Method = "POST";
            request.Headers.Add("Authorization", "Key dc04f2688bad4819a0bf335e279b4a51"); 
            request.ContentType = "application/json";
            request.ContentLength = DATA.Length;
            using (Stream webStream = request.GetRequestStream())
            using (StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.ASCII))
            {
                requestWriter.Write(DATA);
            }
            try
            {
                WebResponse webResponse = request.GetResponse();
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            string response = responseReader.ReadToEnd();
                            Console.Out.WriteLine(response);
                            re = response;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Console.Out.WriteLine("-----------------");
                //Console.Out.WriteLine(e.Message);
                re = e.Message;
            }
            return re;
        }
        public void log()
        {

        }
    }
}