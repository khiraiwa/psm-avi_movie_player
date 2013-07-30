using System;

namespace Avi_Movie_Player
{
    public class MainThreadUtility
    {
        public event EventHandler Invoked;

        public void OnInvoked(EventArgs e)
        {
            if (Invoked != null) {
                Invoked(this, e);
                Delegate[] subscribers = Invoked.GetInvocationList();
                for(int i = 0; i < subscribers.Length; i++) {
                //    Invoked -= subscribers[i];
                }
            }
        }
    }
}

