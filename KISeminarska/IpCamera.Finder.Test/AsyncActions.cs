using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IpCamera.Finder.Test
{
    public class AsyncActions
    {
        /// <summary>
        /// Makes a certain action asynchronious.
        /// </summary>
        /// <param name="toBeAsync">The action that needs to be converted to async model</param>
        /// <param name="onException">Action to be invoked when an exception appears</param>
        /// <param name="final">The action to be invoked after the operation has ended</param>
        /// <returns>The converted action - now it is async.</returns>
        public static Action<int> MakeAsync(Action<int> toBeAsync, Action<Exception> onException, Action final)
        {
            Action<int> asyncAction = new Action<int>(
                delegate(int integerParameter)
                {
                    toBeAsync.BeginInvoke(integerParameter,

                        // the call back
                        delegate(IAsyncResult result)
                        {
                            // This is called asynchroniously when the action has finished
                            var origHandler = (Action<int>)((System.Runtime.Remoting.Messaging.AsyncResult)result).AsyncDelegate;
                            try
                            {
                                origHandler.EndInvoke(result);
                            }
                            catch (Exception e)
                            {
                                if (onException != null)
                                {
                                    onException(e);
                                }
                            }
                            finally
                            {
                                // What do we do if this guy throws? Probably not likely, but, I've seen this happen in real life
                                if (final != null)
                                {
                                    final.Invoke();
                                }
                            }
                        }, null);
                });
            return asyncAction;
        }
    }
}
