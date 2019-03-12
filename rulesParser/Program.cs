using System;
using System.Collections.Generic;

using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace rulesParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var pastIndex = false;
            var rules = new Rules();
            List<Section> sections = new List<Section>();
            List<Subsection> subsections = new List<Subsection>();
            List<Rule> rulelist = new List<Rule>();
            Section tempSection = null;
            Subsection tempsubsection = null;
            var list = new List<string>();
            var fileStream = new FileStream(@"/Users/josephsmith/Downloads/MagicCompRules 20190125.txt", FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length > 0)
                    {
                        if (line == "Glossary")
                        {
                            if (pastIndex)
                            {
                                break;
                            }
                            pastIndex = true;
                            continue;
                        }
                        if (line == "Credits") continue;
                        if (pastIndex)
                        {
                            if (line.Substring(1, 1) == ".")
                            {
                                if (tempSection != null)
                                {
                                    tempsubsection.Rules = rulelist.ToArray();
                                    tempSection.Subsections = subsections.ToArray();
                                    sections.Add(tempSection);
                                    subsections = new List<Subsection>();
                                    rulelist = new List<Rule>();

                                }
                                Console.WriteLine(line);
                                tempSection = new Section();
                                tempSection.Number = long.Parse(line.Substring(0, 1));
                                var length = line.Length;
                                tempSection.Name = line.Substring(1, length - 1);

                            }
                            else if (line.Substring(3, 1) == "." && line.Substring(4, 1) == " ")
                            {
                                if (tempsubsection != null)
                                {
                                    tempsubsection.Rules = rulelist.ToArray();
                                    subsections.Add(tempsubsection);
                                    rulelist = new List<Rule>();
                                }
                                Console.WriteLine(line);
                                tempsubsection = new Subsection();
                                var space = line.IndexOf(" ");
                                tempsubsection.Number = line.Substring(0, space);
                                tempsubsection.Text = line.Substring(space, line.Length - space);
                           
                            }
                            else
                            {
                                Console.WriteLine(line);
                                var rule = new Rule();
                                var space = line.IndexOf(" ");
                                rule.Number = line.Substring(0, space);
                                rule.Text = line.Substring(space, line.Length - space);
                                rulelist.Add(rule);
                            }
                        }
                    }
                }
                list.Add(line);
            }
            tempsubsection.Rules = rulelist.ToArray();
            tempSection.Subsections = subsections.ToArray();
            sections.Add(tempSection);
            subsections = new List<Subsection>();
            rulelist = new List<Rule>();
            rules.Sections = sections.ToArray();
            var json = rules.ToJson();
            Console.WriteLine(json);
            //lines = list.ToArray();
            using (StreamWriter writer = new StreamWriter(@"/Users/josephsmith/Downloads/rules.json"))
            {
                writer.Write(json);
            
            }
        }
      
    }

    public partial class Rules
    {
        [JsonProperty("sections")]
        public Section[] Sections { get; set; }
    }

    public partial class Section
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("number")]
        public long Number { get; set; }

        [JsonProperty("subsections")]
        public Subsection[] Subsections { get; set; }
    }

    public partial class Subsection
    {
        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("rules")]
        public Rule[] Rules { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public partial class Rule
    {
        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public partial class Rules
    {
        public static Rules FromJson(string json) => JsonConvert.DeserializeObject<Rules>(json, rulesParser.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Rules self) => JsonConvert.SerializeObject(self, rulesParser.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}
