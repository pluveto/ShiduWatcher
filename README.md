# Shidu (Watcher)

Shidu (Watcher) 时度监控程序 is an open-source monitoring tool designed to help you keep track of application usage.

## Features

- **Real-time Monitoring**: Track system performance and application health in real-time.
- **Customizable Dashboards**: Create and customize dashboards to display the metrics that matter most to you.
- **Historical Data Analysis**: Store and analyze historical data to identify trends and patterns.

## API Documentation

Default Port: 1893

Increase the number if occupied until port 1949.

### Pause Monitoring

#### Endpoint
`POST /control/pause`

#### Description
Pauses the monitoring process.

### Resume Monitoring

#### Endpoint
`POST /control/continue`

#### Description
Resumes the monitoring process.

### Get Current Monitoring Status

#### Endpoint
`GET /control/status`

#### Description
Retrieves the current status of the monitoring process.

#### Response
```json
{
  "name": "ShiduWatcher",
  "status": "running",
  "interval": 1000
}
```

### Set Monitoring Interval

#### Endpoint
`POST /control/set-interval`

#### Description
Sets the interval for monitoring data collection.

#### Request Body
```json
{
  "interval": 2000
}
```
*Note: The interval is in milliseconds. The minimum interval is 1000ms.*

### Get Program Icon

#### Endpoint
`GET /icon/get-icon-base64?path=<path_to_executable>`

#### Description
Retrieves the icon of the specified executable as a base64 encoded string.

#### Response
```json
{
  "error": null,
  "data": "iVBORw0KGgoAAAANSUhEU...=="
}
```

#### Endpoint
`GET /icon/get-icon?path=<path_to_executable>`

#### Description
Retrieves the icon of the specified executable.

### Get Usage Report

#### Endpoint
`GET /usagereport/usage-report?start_time=<start_unix_timestamp>&end_time=<end_unix_timestamp>`

#### Description
Retrieves the usage report for the specified time range.

#### Response
```json
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
        }
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
        }
      ]
    }
  ]
}
```

## Contributing

We welcome contributions from the community. Please read our [Contributing Guidelines](CONTRIBUTING.md) for more details.

## License

Shidu (Watcher) is licensed under the [Anti-996 License](LICENSE).

## Contact

For any questions or feedback, please open an issue on our [GitHub repository](https://github.com/pluveto/ShiduWatcher/issues).
