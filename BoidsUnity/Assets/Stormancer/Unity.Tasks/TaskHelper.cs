﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniRx;

namespace System.Threading.Tasks
{

    public static class TaskHelper
    {
        public static Task<T> FromResult<T>(T result)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(result);
            return tcs.Task;
        }

        public static Task<T> FromExceptions<T>(IEnumerable<Exception> exceptions)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetException(exceptions);
            return tcs.Task;
        }

        public static Task If(bool condition, Func<Task> action)
        {
            if (condition)
            {
                return action();
            }
            else
            {
                return TaskHelper.FromResult(true);
            }
        }

        public static Task Delay(int milliseconds)
        {
            return Delay(TimeSpan.FromMilliseconds(milliseconds));
        }

        public static Task Delay(TimeSpan delay)
        {
            var tcs = new TaskCompletionSource<Unit>();

            Timer timer = null;
            timer = new Timer(_ =>
            {
                tcs.SetResult(Unit.Default);
                timer.Dispose();
            }, null, delay, TimeSpan.FromMilliseconds(-1));


            return tcs.Task;
        }
    }

}
