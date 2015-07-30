using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System;

namespace WebCompiler
{
    /// <summary>
    /// Handles reading and writing config files to disk.
    /// </summary>
    public class ConfigHandler
    {
        /// <summary>
        /// Adds a config file if no one exist or adds the specified config to an existing config file.
        /// </summary>
        /// <param name="fileName">The file path of the configuration file.</param>
        /// <param name="config">The compiler config object to add to the configration file.</param>
        public void AddConfig(string fileName, Config config)
        {
            IEnumerable<Config> existing = GetConfigs(fileName);
            List<Config> configs = new List<Config>();
            configs.AddRange(existing);
            configs.Add(config);
            config.FileName = fileName;

            string content = JsonConvert.SerializeObject(configs, Formatting.Indented);
            File.WriteAllText(fileName, content, new UTF8Encoding(true));
        }

        /// <summary>
        /// Removes the specified config from the file.
        /// </summary>
        public void RemoveConfig(Config configToRemove)
        {
            IEnumerable<Config> configs = GetConfigs(configToRemove.FileName);
            List<Config> newConfigs = new List<Config>();

            if (configs.Contains(configToRemove))
            {
                newConfigs.AddRange(configs.Where(b => !b.Equals(configToRemove)));
                string content = JsonConvert.SerializeObject(newConfigs, Formatting.Indented);
                File.WriteAllText(configToRemove.FileName, content);
            }
        }

        /// <summary>
        /// Get all the config objects in the specified file.
        /// </summary>
        /// <param name="fileName">A relative or absolute file path to the configuration file.</param>
        /// <returns>A list of Config objects.</returns>
        public static IEnumerable<Config> GetConfigs(string fileName)
        {
            FileInfo file = new FileInfo(fileName);

            if (!file.Exists)
                return Enumerable.Empty<Config>();

            string content = File.ReadAllText(fileName);
            var configs = JsonConvert.DeserializeObject<IEnumerable<Config>>(content).ToList();
            //string folder = Path.GetDirectoryName(file.FullName);

            List<Config> globbedLessFiles = configs.Where(x => x.InputFile.Contains("*.less")).ToList();

            //If an input file contains a * glob character, treat that config object as a representation
            //of multiple configs. Then expand those to the represented configs.
            foreach (Config config in globbedLessFiles)
            {
                string baseDirectory = file.Directory.FullName;
                string directoryForLessFiles = config.InputFile.Substring(0, config.InputFile.LastIndexOf('*'));

                //hard coding the search for *.less for now. This can be changed later.
                string[] inputFilePaths = Directory.GetFiles(Path.Combine(baseDirectory, directoryForLessFiles), "*.less");

                Uri baseUri = new Uri(baseDirectory + Path.DirectorySeparatorChar);
                foreach (string inputPath in inputFilePaths)
                {
                    Uri fileUri = new Uri(inputPath);
                    string relativePath = baseUri.MakeRelativeUri(fileUri).ToString();

                    Config resolvedConfig = new Config(config);
                    resolvedConfig.InputFile = relativePath;
                    resolvedConfig.OutputFile = relativePath.Substring(0, relativePath.LastIndexOf(".")) + ".css";

                    configs.Add(resolvedConfig);
                }
            }

            //Remove the globbed less files from the config list.
            globbedLessFiles.ForEach(x => configs.Remove(x));

            foreach (Config config in configs)
            {
                config.FileName = fileName;
            }

            return configs;
        }
    }
}
