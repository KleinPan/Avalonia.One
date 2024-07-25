## Usage:

```csharp
var logger = LogManager.GetLogger(typeof(YourClass));

logger.Debug("Hello World");
logger.Info("Hello World");
logger.Warn("Hello World");
logger.Error("Hello World");
logger.Fatal("Hello World");
```

## Config

App.config

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <logging>
    <target value="file" file="log/Mojito.log" maxRollBackups="30" maxRollTime="1d" />
    <level value="Info" />
    <pattern value="%date [%thread] %level %logger - %message%newline" />
  </logging>
</configuration>
```

Print stack

```xml
<pattern value="%date [%thread] %level %logger - %message%newline %stack" />
```


### Options

`target`: // Logger target `Console` | `file` 
- `file` // Log path
- `maxRollBackups="10"` // The maximum retention is 10 copies  
- `maxRollSize="512kb"` // The log is larger than or equal to 512kb, Supported units is `b`, `kb`, `mb`, `gb`  
- `maxRollTime="1d"` // The log is rolled every 1 day, Supported units is `s`, `m`, `h`, `d`

`level`: // Log level
- `Debug`
- `Info`
- `Warn`
- `Error`
- `Fatal`

`pattern` // Log pattern
- `%date` // Date time default format is `yyyy-MM-dd HH:mm:ss`
- `%level` // Log level
- `%thread` // Thread name and Thread ID
- `%logger` // Caller class name
- `%stack` // Stack Trace
- `%message` // Your message
- `%newline` // Environment.NewLine