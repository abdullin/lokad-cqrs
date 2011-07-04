using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public interface ITapeStream
    {
        /// <summary>
        /// Reads up to <see cref="maxCount"/> records with <see cref="offset"/>.
        /// </summary>
        /// <param name="offset">The number of records to skip.</param>
        /// <param name="maxCount">The max number of records to load.</param>
        /// <returns>collection of taped blocks</returns>
        IEnumerable<TapeRecord> ReadRecords(long offset, int maxCount);

        /// <summary>
        /// Returns current storage version
        /// </summary>
        /// <returns></returns>
        long GetCurrentCount();

        /// <summary>
        /// Saves the specified records
        /// </summary>
        /// <param name="records">The records to save.</param>
        /// <param name="appendCondition">The append condition.</param>
        bool AppendRecords(ICollection<byte[]> records, TapeAppendCondition appendCondition = default(TapeAppendCondition));
    }

    public struct TapeAppendCondition
    {
        public readonly long Index;
        public readonly bool IsSpecified;

        public TapeAppendCondition(long index) 
        {
            Index = index;
            IsSpecified = true;
        }

        public static readonly TapeAppendCondition None = default(TapeAppendCondition);

        public bool Satisfy(long index)
        {
            if (!IsSpecified)
                return true;
            return index == Index;
        }

        public void Enforce(long index)
        {
            if (!Satisfy(index))
            {
                var message = string.Format("Expected store version {0} but was {1}. Probablye is was modified concurrently.", Index, index);
                throw new TapeAppendException(message);
            }
        }
    }

    [Serializable]
    public class TapeAppendException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public TapeAppendException() {}
        public TapeAppendException(string message) : base(message) {}
        public TapeAppendException(string message, Exception inner) : base(message, inner) {}

        protected TapeAppendException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) {}

        
    }
}