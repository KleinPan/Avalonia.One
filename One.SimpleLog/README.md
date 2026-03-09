## Usage:

```csharp
var logger = LogManager.GetLogger()
    .WithPatternProperty("thread", $"{Thread.CurrentThread.Name ?? ""}:{Environment.CurrentManagedThreadId}");

logger.Debug("Hello World");
logger.Info("Hello World");
logger.Warn("Hello World");
logger.Error("Hello World");
logger.Fatal("Hello World");

try
{
    throw new InvalidOperationException("boom");
}
catch (Exception ex)
{
    logger.Error(ex, "Something failed");
}
```

## Config

App.config

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <logging>
    <target value="File" file="log/Mojito.log" maxRollBackups="30" maxRollTime="1d" />
    <level value="Info" />
    <pattern value="%date [%thread] %level - %message%newline" />
  </logging>
</configuration>
```

### Options

`target`: logger target `Console` | `File`
- `file`: log path
- `maxRollBackups="10"`: keep latest 10 rolled files
- `maxRollSize="512kb"`: roll when file size >= 512kb, supports `b`, `kb`, `mb`, `gb`
- `maxRollTime="1d"`: roll every 1 day, supports `s`, `m`, `h`, `d`

`level`: log level
- `Debug`
- `Info`
- `Warn`
- `Error`
- `Fatal`

`pattern`: log pattern (supports lower/upper case placeholders)
- `%date` / `%Date`
- `%level` / `%Level`
- `%stack` / `%Stack`
- `%message` / `%Message`
- `%newline` / `%Newline`
- custom placeholders via `.WithPatternProperty(...)`, e.g. `%thread`
