using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestJsonSerialization
{
    public class Program
    {
        public static void Main(String[] args) {
            var d = new Dictionary<String, Object>();

            d.Add("ProcessId", 123);
            d.Add("ProcessName", "w3wp.exe");
            d.Add("ApplicationPath", "c:\\test.exe");
            d.Add("CPU", 12m);

            var s = JsonConvert.SerializeObject(d);

            Console.WriteLine(s);

            var o = JsonConvert.DeserializeObject<Dictionary<String, Object>>(s);

            foreach (var k in o) {
                Console.WriteLine("key: '{0}', value: {1} of type: {2}", k.Key, k.Value, k.Value.GetType());
            }
        }
    }
}
