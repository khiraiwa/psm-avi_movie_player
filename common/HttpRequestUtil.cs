using System;
using System.IO;
using System.Net;

namespace Avi_Movie_Player
{
    public class HttpRequestUtil
    {
        private byte[] readBuffer;
        private string statusCode;
        private long contentLength;
        private int totalReadSize;
        private const int maxReadSize = 1024;
        private FileStream  dfs = null;
        private String fileName;

        public ConnectState State = ConnectState.None;
        public event EventHandler Completed;
        public delegate void ErrorEventHandler(object sender, ErrorEventArgs e);
        public event ErrorEventHandler ErrorOccurred;


        public enum ConnectState
        {
            None,
            Ready,
            Connect,
            Success,
            Failed
        }

        public bool DownloadFile(Uri uri, String fileName) {
            this.fileName = fileName;
            statusCode = "Unknown";
            contentLength = 0;
            try {
                WebRequest webRequest = HttpWebRequest.Create(uri);
                webRequest.BeginGetResponse(new AsyncCallback(requestCallBack), webRequest);
                State = ConnectState.Connect;
            } catch (Exception e) {
                Console.WriteLine(e);
                State = ConnectState.Failed;
                ErrorEventArgs args = new ErrorEventArgs();
                args.Message = e.Message;
                OnErrorOccurred(args);
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
                State = ConnectState.Failed;
                ErrorEventArgs args = new ErrorEventArgs();
                args.Message = e.Message;
                OnErrorOccurred(args);
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
                    State = ConnectState.Success;
                    Console.WriteLine("connect close!!:" +  readSize);
                    onCompleted(EventArgs.Empty);

               } else {
                    stream.BeginRead(readBuffer, 0, readBuffer.Length, new AsyncCallback(readCallBack), stream);
                    Console.WriteLine("connect next!!:" + totalReadSize);
                }
            } catch (Exception e) {
                Console.WriteLine(e);
                State = ConnectState.Failed;
                ErrorEventArgs args = new ErrorEventArgs();
                args.Message = e.Message;
                OnErrorOccurred(args);
            }
        }

        private void onCompleted(EventArgs e)
        {
            if (Completed != null) {
                Completed(this, e);
            }
        }

        private void OnErrorOccurred(ErrorEventArgs e) {
            if (ErrorOccurred != null) {
                ErrorOccurred(this, e);
            }
        }
    }
}

