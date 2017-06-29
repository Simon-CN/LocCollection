using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LocReceiverService.Controllers
{
    [Route("api/[controller]")]
    public class LocServiceController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        bool isStore = false;
        static List<string> cache = new List<string>();
        private static readonly string _appsecret = "H2+k+y+BVgy7qsZJMm+bsdLadyJJ+6JBmTe4tiAEiyI";
        private static readonly string _uri = "/api/e2ad0736-3525-4cc9-b025-44c70a7db989/HuaweiServer/locationRequest";

        public LocServiceController(IHostingEnvironment environment)
        {
            this._hostingEnvironment = environment;
        }

        [HttpGet]
        [Route("output")]
        public string Get(int r)
        {
            string s = "";
            isStore = true;
            s = WriteToFile();
            cache.Clear();
            isStore = false;
            return s;
        }


        // POST api/values
        [HttpPost]
        public String Post([FromBody]JObject value, [FromHeader] string key, [FromHeader]string sign)
        {
            if (isStore)
                return "当前不可用";
            DateTime time = DateTime.Now;
            string s = "*************************************************************\n";
            try
            {
                s += "time : " + time.GetDateTimeFormats('f')[0].ToString() + "\n";
                s += "key : " + key + "\n";
                s += "sign : " + sign + "\n";
                s += "body : " + value.ToString(Formatting.None) + "\n";
            }
            catch (Exception e)
            {
                s += e.Message;
            }
            finally
            {
                s += "\n";
            }
            cache.Add(s);
            if (CheckSign(key, sign, value.ToString(Formatting.None)))
                return "认证成功";
            else
                return "认证失败";
        }

        private string WriteToFile()
        {
            if (cache.Count == 0)
                return "empty";
            try
            {
                string s = _hostingEnvironment.WebRootPath + "/";
                FileStream fs = new FileStream(s + DateTime.Now.ToFileTime() + ".txt", FileMode.Append);
                StreamWriter sw = new StreamWriter(fs);
                for (int i = 0; i < cache.Count; i++)
                    sw.Write(cache[i]);
                sw.Flush();
                sw.Close();
                fs.Close();
                return "finished";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return e.Message;
            }
            finally
            {
                Console.WriteLine("文件已保存");
            }

        }

        private bool CheckSign(string key, string sign, string body)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(_appsecret);
            builder.Append(_uri);
            builder.Append(body);
            string s = sha256(builder.ToString());
            return sign.Equals(s);
        }

        static string sha256(string password)
        {
            SHA256Managed crypt = new SHA256Managed();
            StringBuilder hash = new StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetByteCount(password));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString().ToUpper();
        }
    }
}
