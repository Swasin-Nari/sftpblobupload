using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace sftpblobupload
{
    public class blobupload
    {
        [FunctionName("blobcopy")]
        public void Run([BlobTrigger("fdarchived/{name}", Connection = "storageAcct")]Stream myBlob, string name, ILogger log)
        {
            try
            {
                log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
                        var keyStr = @"";

                byte[] byteArray = Encoding.ASCII.GetBytes(keyStr);
                MemoryStream sshkey = new MemoryStream(byteArray);

                    PrivateKeyFile keyFile = new PrivateKeyFile(sshkey);
                    var keyFiles = new[] { keyFile };
                // These variables can also be stored on key vault instead of local.settings.json. If it is stored in keyvault, you will need to change the ref. to that of the location of the keys.
                   string host = "ab"; //Environment.GetEnvironmentVariable("serveraddress");
                    string sftpUsername = "123"; //Environment.GetEnvironmentVariable("sftpusername");
                    var methods = new List<AuthenticationMethod>();
                    methods.Add(new PrivateKeyAuthenticationMethod(sftpUsername, keyFiles));
                    

                    // Connect to SFTP Server and Upload file 
                    ConnectionInfo con = new ConnectionInfo(host, 22, sftpUsername, methods.ToArray());
                    using (var client = new SftpClient(con))
                    {
                        client.Connect();
                        client.UploadFile(myBlob, $"fdarchived/{name}");

                        var files = client.ListDirectory("/fdarchived");
                        foreach (var file in files)
                        {
                            log.LogInformation(file.Name);
                        }
                        client.Disconnect();
                    }
                
            }
            catch(Exception e)
            {
                log.LogInformation(e.ToString());
            }
           
        }
    }
}
