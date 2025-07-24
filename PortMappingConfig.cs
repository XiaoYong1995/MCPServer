using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SampleMcpServer
{

    public class PortMappingConfig
    {
        public string Mask { get; set; }
        public string LanIp { get; set; }
        public int Count { get; set; }

        [JsonIgnore]
        public Dictionary<string, PortMappingRule> Rules { get; set; } = new Dictionary<string, PortMappingRule>();

        // 用于存储其他非规则属性
        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        [OnDeserialized]
        private void OnDeserialized(System.Runtime.Serialization.StreamingContext context)
        {
            Rules = new Dictionary<string, PortMappingRule>();

            if (_additionalData != null)
            {
                // 提取所有pmRule开头的属性
                var ruleProperties = _additionalData.Where(p => p.Key.StartsWith("pmRule")).ToList();

                foreach (var property in ruleProperties)
                {
                    Rules[property.Key] = property.Value.ToObject<PortMappingRule>();
                    _additionalData.Remove(property.Key);
                }
            }
        }

        [OnSerializing]
        private void OnSerializing(System.Runtime.Serialization.StreamingContext context)
        {
            if (_additionalData == null)
            {
                _additionalData = new Dictionary<string, JToken>();
            }

            // 将Rules添加到_additionalData以便序列化
            foreach (var rule in Rules)
            {
                _additionalData[rule.Key] = JToken.FromObject(rule.Value);
            }
        }
    }

    public class PortMappingRule
    {
        public string Protocol { get; set; }
        public int InPort { get; set; }
        public int Enable { get; set; }
        [JsonProperty("desp")]
        public string Description { get; set; }
        public string Client { get; set; }
        public int ExPort { get; set; }
    }
}
