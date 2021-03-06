﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apache.NMS;

namespace NMS.AMQP.Util
{
    class TaskUtil
    {
        public static T Wait<T>(Task<T> t, long millis)
        {
            return TaskUtil.Wait(t, TimeSpan.FromMilliseconds(millis));
        }
        public static bool Wait(Task t, long millis)
        {
            return TaskUtil.Wait(t, TimeSpan.FromMilliseconds(millis));
        }

        public static T Wait<T>(Task<T> t, TimeSpan ts)
        {
            if (TaskUtil.Wait((Task)t, ts))
            {
                return t.Result;
            }
            else
            {
                throw new Exception(string.Format("Failed to exceute task {0} in time {1}ms.", t, ts.TotalMilliseconds));
                //return default(T);
            }
        }

        public static bool Wait(Task t, TimeSpan ts)
        {
            DateTime current = DateTime.Now;
            DateTime end = current.Add(ts);
            do
            {
                try
                {
                    t.Wait(10);
                }
                catch (AggregateException ae)
                {
                    if (t.IsFaulted || t.IsCanceled || t.Exception != null)
                    {
                        Tracer.ErrorFormat("Error Excuting task Failed to Complete: {0}", t.Exception);
                        break;
                    }
                }
            } while ((current = current.AddMilliseconds(10)) < end && !t.IsCompleted);
            return t.IsCompleted;
        }
    }
}
