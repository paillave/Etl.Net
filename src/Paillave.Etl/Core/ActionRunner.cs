using System;

namespace Paillave.Etl.Core
{
    public class ActionRunner
    {
        public static void TryExecute(int attempts, Action action)
        {
            while (attempts > 0)
            {
                try
                {
                    action();
                    return;
                }
                catch
                {
                    if (--attempts <= 0) throw;
                }
                System.Threading.Thread.Sleep(1000);
            }
        }
        public static T TryExecute<T>(int attempts, Func<T> action)
        {
            while (attempts > 0)
            {
                try
                {
                    return action();
                }
                catch
                {
                    if (--attempts <= 0) throw;
                }
                System.Threading.Thread.Sleep(1000);
            }
            throw new Exception($"{nameof(ActionRunner)}: This failure should not happen");
        }
    }
}