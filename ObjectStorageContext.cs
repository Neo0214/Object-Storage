using MyLock;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Object_Storage
{
    public class ObjectStorageContext
    {
        private string folder;
        private static ObjectStorageContext _instance;
        private MyReaderWriterLockSlim _lock;
        public static ObjectStorageContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ObjectStorageContext();
                }
                else
                {
                    Console.WriteLine("WARN: You're re-instancing OSS Context.");
                }
                return _instance;
            }
        }
        private ObjectStorageContext()
        {

            string currentDirectory = Directory.GetCurrentDirectory();
            string folderName = "data";
            string folderPath = Path.Combine(currentDirectory, folderName);
            folder = folderPath;
            // 检查文件夹是否存在
            if (!Directory.Exists(folderPath))
            {
                // 如果文件夹不存在，则创建它
                Directory.CreateDirectory(folderPath);
                Console.WriteLine($"Directory '{folderName}' created at {folderPath}");
            }
            else
            {
                // 如果文件夹已存在，输出提示
                Console.WriteLine("Object Storage init OK");
            }
            // init lock
            _lock = new MyReaderWriterLockSlim();
        }

        private void storeTask(byte[] bytes, ref string name)
        {
            string fileName = generateName(bytes);
            string filePath = Path.Combine(folder, fileName + ".oss");
            // 写入文件内容
            try
            {
                _lock.EnterWriteLock();
                File.WriteAllBytes(filePath, bytes);
                _lock.ExitWriteLock();
                Console.WriteLine($"File written to: {filePath}");
                name = fileName;
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("OSS Error: " + ex.Message);
                name = null;
            }
        }
        public string store(byte[] bytes)
        {
            string fileName = "";
            Thread thread = new Thread(() => storeTask(bytes, ref fileName));
            thread.Start();
            thread.Join();
            return fileName;
        }
        private void deleteTask(string fileName, ref bool isOk)
        {
            string filePath = Path.Combine(folder, fileName + ".oss");
            try
            {
                _lock.EnterWriteLock();
                File.Delete(filePath);
                _lock.ExitWriteLock();
                Console.WriteLine($"File deleted : {filePath}");
                isOk = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("OSS Error: " + ex.Message);
                isOk = false;
            }
        }

        public bool delete(string fileName)
        {
            bool isOK = false;
            Thread thread = new Thread(() => deleteTask(fileName, ref isOK));
            thread.Start();
            thread.Join();
            return isOK;

        }
        private void fetchTask(ref byte[] content, string fileName)
        {
            string filePath = Path.Combine(folder, fileName + ".oss");
            try
            {
                _lock.EnterReadLock();
                byte[] bytes = File.ReadAllBytes(filePath);
                _lock.ExitReadLock();
                content = bytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine("OSS Error: " + ex.Message);
                content = null;
            }
        }
        public byte[] fetchFile(string fileName)
        {
            byte[] content = null;
            Thread thread = new Thread(() => fetchTask(ref content, fileName));
            thread.Start();
            thread.Join();
            return content;


        }
        public string generateName(byte[] bytes)
        {
            long unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            // 转为bytes
            byte[] time = BitConverter.GetBytes(unixTime);
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(bytes.Concat(time).ToArray());
                // 将字节数组转换为十六进制字符串
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }
                return builder.ToString();
            }

        }
    }
}
