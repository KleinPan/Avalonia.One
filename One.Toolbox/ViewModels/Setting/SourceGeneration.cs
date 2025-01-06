using One.Toolbox.ViewModels.BingImage;
using One.Toolbox.ViewModels.Dashboard;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace One.Toolbox.ViewModels.Setting;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AllConfigModel))]
[JsonSerializable(typeof(BingImageOriginalModel))]
[JsonSerializable(typeof(YiyanAPIM))]
[JsonSerializable(typeof(List<UsefullImageInfoModel>))]
[JsonSerializable(typeof(GithubReleaseInfoM))] 
internal partial class SourceGenerationContext : JsonSerializerContext
{
}