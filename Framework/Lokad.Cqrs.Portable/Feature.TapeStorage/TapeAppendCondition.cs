namespace Lokad.Cqrs.Feature.TapeStorage
{
    /// <summary>
    /// <para>Allows to specify optional condition for appending to the storage.</para>
    /// <para>This is defined as struct to allow proper use in optional params</para>
    /// </summary>
    public struct TapeAppendCondition
    {
        /// <summary>
        /// Version to match against, if <see cref="IsSpecified"/> is set to <em>True</em>.
        /// </summary>
        public readonly long Version;
        /// <summary>
        /// If the condition has been specified
        /// </summary>
        public readonly bool IsSpecified;

        TapeAppendCondition(long index) 
        {
            Version = index;
            IsSpecified = true;
        }

        /// <summary>
        /// Constructs condition that matches the specified version
        /// </summary>
        /// <param name="version">The version.</param>
        /// <returns>new condition instance</returns>
        public static TapeAppendCondition VersionIs(long version)
        {
            return new TapeAppendCondition(version);
        }

        /// <summary>
        /// Condition that always matches
        /// </summary>
        public static readonly TapeAppendCondition None = default(TapeAppendCondition);

        public bool Satisfy(long index)
        {
            if (!IsSpecified)
                return true;
            return index == Version;
        }

        /// <summary>
        /// Enforces the specified version, throwing exception if the condition was not met.
        /// </summary>
        /// <param name="version">The version to match.</param>
        /// <exception cref="TapeAppendConditionException">when the condition was not met by the version specified</exception>
        public void Enforce(long version)
        {
            if (!Satisfy(version))
            {
                var message = string.Format("Expected store version {0} but was {1}. Probablye is was modified concurrently.", Version, version);
                throw new TapeAppendConditionException(message);
            }
        }
    }
}