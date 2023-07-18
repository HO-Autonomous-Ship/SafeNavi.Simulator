using System;
using System.IO;
using Newtonsoft.Json;

namespace SyDLab.Usv.Simulator.Domain
{
    public class JsonSerializerService
    {
        public static void FileSerializer<T>(string fileName, T serializedObject)
        {
            try
            {
                using (TextWriter tw = new StreamWriter(fileName))
                {
                    using (var jw = new JsonTextWriter(tw))
                    {
                        var js = new JsonSerializer();
                        js.TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;
                        js.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                        js.TypeNameHandling = TypeNameHandling.All;
                        js.Formatting = Formatting.Indented;
                        js.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize;

                        js.Serialize(jw, serializedObject);
                        jw.Close();
                    }
                    tw.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static T FileDeserializer<T>(string fileName)
        {
            T deserializedObject = default(T);

            try
            {
                using (TextReader tr = new StreamReader(fileName))
                {
                    using (var jr = new JsonTextReader(tr))
                    {
                        var js = new JsonSerializer();
                        js.TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;
                        js.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                        js.TypeNameHandling = TypeNameHandling.All;
                        js.Formatting = Formatting.Indented;
                        js.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize;

                        // deserialize & close
                        deserializedObject = js.Deserialize<T>(jr);
                        jr.Close();
                    }
                    tr.Close();
                }

                return deserializedObject;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static T Clone<T>(T serializedObject)
        {
            var setting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full,
                TypeNameHandling = TypeNameHandling.All
            };

            string output = JsonConvert.SerializeObject(serializedObject, Formatting.Indented, setting);
            T clone = JsonConvert.DeserializeObject<T>(output, setting);
            return clone;
        }
    }
}