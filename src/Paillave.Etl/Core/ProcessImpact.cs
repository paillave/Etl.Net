namespace Paillave.Etl.Core
{
    public enum ProcessImpact
    {
        /// <summary>
        /// Memorywise: what comes from what is supposed to be the largest input stream is never stored: output are directly issued with the flow)
        /// Performancewise: nearly transparent computation finger print
        /// </summary>
        Light = 1,
        /// <summary>
        /// Memorywise: stores a reasonable part of what is provided by the input streams
        /// Performancewise: wait till the last entry what is supposed to be the largest input stream before issuing the first output OR proceeds heavy computations
        /// </summary>
        Average = 2,
        /// <summary>
        /// Memorywise: stores everything from what is supposed to be the largest input stream
        /// Performancewise: wait till the last entry from what is supposed to be the largest input stream before issuing the first output AND proceeds heavy computations
        /// </summary>
        Heavy = 3
    }
}