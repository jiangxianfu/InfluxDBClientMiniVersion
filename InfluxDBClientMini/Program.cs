using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfluxDBClientMiniVersion
{
    class Program
    {
        static void Main(string[] args)
        {
            using (InfluxDBClient client = new InfluxDBClient(""))
            {
                var result = client.Write("db=abc&dd=sss", "");
                var data = client.Query("db=abc&mm=ss");
            }
        }
    }
}
