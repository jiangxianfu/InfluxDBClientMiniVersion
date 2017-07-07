# InfluxDBClientMiniVersion

一个 mini 版本的InfluxDBClient

Use:
```
  using (InfluxDBClient client = new InfluxDBClient(""))
            {
                //写
                var result = client.Write("db=abc&dd=sss", "");
                //读
                var data = client.Query("db=abc&mm=ss");
            }
```