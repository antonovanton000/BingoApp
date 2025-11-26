using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WheelGame.Classes
{
    public static class DelegateHelperClass
    {
        public static void RemoveEventHandlers(this MulticastDelegate m)
        {

            string eventName = nameof(m);

            EventInfo eventInfo = m.GetType().GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
            if (eventInfo == null)
            {
                return;
            }
            Delegate[] subscribers = m.GetInvocationList();
            Delegate currentDelegate;
            for (int i = 0; i < subscribers.Length; i++)
            {

                currentDelegate = subscribers[i];
                eventInfo.RemoveEventHandler(currentDelegate.Target, currentDelegate);

            }

        }
    }
}
