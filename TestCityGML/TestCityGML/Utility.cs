using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace TestCityGML
{
    class Utility
    {
        public static void SaveData<T>(T data, string path)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            using (var writer = new StreamWriter(path))
            {
                var result = JsonConvert.SerializeObject(data, settings);
                writer.WriteLine(result);
            }
        }

        public static T LoadData<T>(string path)
        {
            using (var reader = new StreamReader(path))
            {
                var str = reader.ReadToEnd();
                var dt = (T)JsonConvert.DeserializeObject(str, typeof(T));
                return dt;
            }
        }
    }
}