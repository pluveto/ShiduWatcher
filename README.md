## API

### 暂停统计

`POST /control/pause`

### 恢复统计

`POST /control/continue`

### 获取当前监视状态

`GET /control/status`

### 修改统计间隔

`POST /control/set-interval，请求体为 { "interval": 2000 }（单位为毫秒）`

### 获取程序图标

`GET /icon/get-icon?path=<path_to_executable>`

Response:

```
{"error":null,"data":"iVBORw0KGgoAAAANSUhEU...=="}
```

### 获取使用报告

`GET /usagereport/usage-report?start_time=<start_unix_timestamp>&end_time=<end_unix_timestamp>`

Response:

```
{
  "totalDuration": 1528,
  "details": [
    {
      "processName": "explorer",
      "executablePath": "C:\\Windows\\explorer.exe",
      "totalDuration": 21,
      "usage": [
        {
          "timestamp": "2024-06-25T22:15:36.0556528",
          "duration": 1
        },
      ]
    },
    {
      "processName": "vivaldi",
      "executablePath": "C:\\Users\\i\\AppData\\Local\\Vivaldi\\Application\\vivaldi.exe",
      "totalDuration": 383,
      "usage": [
        {
          "timestamp": "2024-06-25T22:02:16.1124328",
          "duration": 3
        },
        {
          "timestamp": "2024-06-25T22:02:28.910189",
          "duration": 131
        },
      ]
    }
  ]
}
```
