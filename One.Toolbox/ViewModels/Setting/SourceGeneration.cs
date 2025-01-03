using One.Toolbox.ViewModels.BingImage;

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
[JsonSerializable(typeof(List<UsefullImageInfoModel>))] 
internal partial class SourceGenerationContext : JsonSerializerContext
{
}
