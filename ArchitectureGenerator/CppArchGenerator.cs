using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace ArchitectureGenerator
{
    class CppArchGenerator
    {
        public async static Task<Dictionary<string, HashSet<int>>> GenerateAsync(StorageFolder dict)
        {
            //var dict = await Windows.Storage.StorageFolder.GetFolderFromPathAsync("C:\\Dev\\Repos\\ShadowDriver\\ShadowDriver");
            var fileInfos = await dict.GetFilesAsync();
            var fileInfoList = fileInfos.ToList();
            List<StorageFile> selectedFiles = (from f in fileInfos
                                            where (f.FileType == ".h" || f.FileType == ".c" || f.FileType == ".cpp")
                                            select f).ToList();
            Dictionary<string, List<string>> fileLinkList = new Dictionary<string, List<string>>();
            Dictionary<string, int> fileIncludeCount = new Dictionary<string, int>();
            Dictionary<string, DependTreeNode<string>> correspoindingNodes = new Dictionary<string, DependTreeNode<string>>();
            Dictionary<string, int> fileEnterCount = new Dictionary<string, int>();
            DependTreeNode<string> nodes = new DependTreeNode<string>();

            //使用CodePagesEncodingProvider去注册扩展编码。
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //注册GBK编码
            Encoding encodingGbk = Encoding.GetEncoding("GBK");
            foreach (var fileInfo in selectedFiles)
            {
                IBuffer buffer = await FileIO.ReadBufferAsync(fileInfo);
                byte[] fileData = buffer.ToArray();
                Encoding encoding = Encoding.GetEncoding("GBK");
                string text = encoding.GetString(fileData, 0, fileData.Length);

                var lines = text.Split(Environment.NewLine);
                //string line = string.Empty;
                List<string> includeFiles = new List<string>();

                if (!fileEnterCount.ContainsKey(fileInfo.Name))
                {
                    fileEnterCount[fileInfo.Name] = 0;
                }
                foreach(var line in lines)
                {
                    if (line.Contains("#include"))
                    {
                        var hashIndex = line.IndexOf('#');
                        var startIndex = hashIndex + string.Format("#include").Length;
                        var leftString = line.Substring(startIndex, line.Length - startIndex);
                        leftString = leftString.Trim().Remove(0, 1);
                        leftString = leftString.Remove(leftString.Length - 1);
                        if (!fileLinkList.ContainsKey(fileInfo.Name))
                        {
                            fileLinkList[fileInfo.Name] = new List<string>();
                        }
                        fileLinkList[fileInfo.Name].Add(leftString);

                        if (!fileEnterCount.ContainsKey(leftString))
                        {
                            fileEnterCount[leftString] = 0;
                        }
                        fileEnterCount[leftString]++;

                        if (!fileIncludeCount.ContainsKey(leftString))
                        {
                            fileIncludeCount[leftString] = 0;
                        }
                        ++fileIncludeCount[leftString];
                    }
                }
            }
            fileIncludeCount = fileIncludeCount.OrderBy(key => key.Value).ToDictionary(a => a.Key, a => a.Value);
            SortedDictionary<int, List<string>> sortedByEnterNum = new SortedDictionary<int, List<string>>();

            foreach (var i in fileEnterCount)
            {
                if (!sortedByEnterNum.ContainsKey(i.Value))
                {
                    sortedByEnterNum[i.Value] = new List<string>();
                }
                sortedByEnterNum[i.Value].Add(i.Key);
            }

            //初始化分层
            Dictionary<string, HashSet<int>> layers = new Dictionary<string, HashSet<int>>();
            var keyList = sortedByEnterNum.Keys.ToList();
            for (int i = 0; i < sortedByEnterNum.Count; ++i)
            {
                var currentEnterNum = keyList[i];
                foreach (var file in sortedByEnterNum[currentEnterNum])
                {
                    layers[file] = new HashSet<int> { i + 1 };
                }
            }

            for (int currentLayer = 1; currentLayer < sortedByEnterNum.Count; ++currentLayer)
            {
                var currentLayerFiles = (from file in layers.Keys where layers[file].Contains(currentLayer) select file).ToList();

                for (int i = 0; i < currentLayerFiles.Count; ++i)
                {
                    for (int j = 0; j < currentLayerFiles.Count; ++j)
                    {
                        if (j != i)
                        {
                            if (fileLinkList.ContainsKey(currentLayerFiles[i]))
                            {
                                if (fileLinkList[currentLayerFiles[i]].Contains(currentLayerFiles[j]))
                                {
                                    layers[currentLayerFiles[j]].Add(currentLayer + 1);
                                }
                            }
                        }
                    }
                }

            }

            return layers;
        }
    }
}
