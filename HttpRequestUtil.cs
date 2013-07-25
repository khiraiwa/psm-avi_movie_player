using System;
using System.IO;
using System.Net;

namespace Avi_Movie_Player
{
    public class HttpRequestUtil
    {
        public event EventHandler Completed;

        public enum ConnectState
        {
            None,
            Ready,
            Connect,
            Success,
            Failed
        }
        private byte[] readBuffer;
        public ConnectState connectState = ConnectState.None;
        private string statusCode;
        private long contentLength;
        private int totalReadSize;
        private const int maxReadSize = 1024;
        private FileStream  dfs = null;
        private String DOWNLOAD_PATH = "/Documents";
        private String fileName;

        public bool downloadFile(Uri uri, String fileName) {
            this.fileName = fileName;
            statusCode = "Unknown";
            contentLength = 0;
            try {
                WebRequest webRequest = HttpWebRequest.Create(uri);
                // If you use web proxy, uncomment this and set appropriate address.
                //webRequest.Proxy = new System.Net.WebProxy("http://your_proxy.com:10080");
                webRequest.BeginGetResponse(new AsyncCallback(requestCallBack), webRequest);
                connectState = ConnectState.Connect;
            } catch (Exception e) {
                Console.WriteLine(e);
                connectState = ConnectState.Failed;
                return false;
            }
            return true;
        }

        private void requestCallBack(IAsyncResult ar) {
            try {
                var webRequest = (HttpWebRequest)ar.AsyncState;
                var webResponse = (HttpWebResponse)webRequest.EndGetResponse(ar);
    
                statusCode = webResponse.StatusCode.ToString();
                contentLength = webResponse.ContentLength;
                readBuffer = new byte[1024];
                totalReadSize = 0;
    
                Uri uri = webRequest.RequestUri;
                dfs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);

                var stream = webResponse.GetResponseStream();
                stream.BeginRead(readBuffer, 0, readBuffer.Length, new AsyncCallback(readCallBack), stream);
            } catch (Exception e) {
                Console.WriteLine(e);
                connectState = ConnectState.Failed;
            }
        }

        private void readCallBack(IAsyncResult ar) {
            try {
                var stream = (Stream)ar.AsyncState;
                int readSize = stream.EndRead(ar);
    
                if (readSize > 0) {
                    if (readSize != readBuffer.Length) {
                        byte[] tmp = new byte [readSize];
                        Array.Copy(readBuffer, 0, tmp, 0, readSize);
                        BinaryUtil.BinaryWrite(dfs, totalReadSize, ref tmp);
                    } else {
                        BinaryUtil.BinaryWrite(dfs, totalReadSize, ref readBuffer);
    
                    }
                    totalReadSize += readSize;
                }
                if (readSize <= 0) {
                    dfs.Close();
                    dfs.Dispose();
    
                    stream.Close();
                    connectState = ConnectState.Success;
                    Console.WriteLine("connect close!!:" +  readSize);
                    OnCompleted(EventArgs.Empty);

               } else {
                    stream.BeginRead(readBuffer, 0, readBuffer.Length, new AsyncCallback(readCallBack), stream);
                    Console.WriteLine("connect next!!:" + totalReadSize);
                }
            } catch (Exception e) {
                Console.WriteLine(e);
                connectState = ConnectState.Failed;
            }
        }

        private void OnCompleted(EventArgs e)
        {
            if (Completed != null) {
                Completed(this, e);
            }
        }
    }
}

