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
        public string store(byte[] bytes)
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
                return fileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine("OSS Error: " + ex.Message);
                return null;
            }
        }
        public bool delete(string fileName)
        {
            string filePath = Path.Combine(folder, fileName + ".oss");
            try
            {
                _lock.EnterWriteLock();
                File.Delete(filePath);
                _lock.ExitWriteLock();
                Console.WriteLine($"File deleted : {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("OSS Error: " + ex.Message);
                return false;
            }
        }
        public byte[] fetchFile(string fileName)
        {
            string filePath = Path.Combine(folder, fileName + ".oss");
            try
            {
                _lock.EnterReadLock();
                byte[] bytes = File.ReadAllBytes(filePath);
                _lock.ExitReadLock();
                return bytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine("OSS Error: " + ex.Message);
                return null;
            }
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
