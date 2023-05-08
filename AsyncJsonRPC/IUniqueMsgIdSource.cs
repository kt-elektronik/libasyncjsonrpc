namespace AsyncJsonRPC
{
    /// <summary>
    /// An interface for temporarily unique identifers, like message sequence
    /// numbers, UUIDs, or JSON-RPC Id field values.
    /// </summary>
    /// <typeparam name="IdType"></typeparam>
    internal interface IUniqueMsgIdSource<IdType> where IdType : struct
    {
        /// <summary>
        /// Acquire a unique message identifier. The same identifier may have
        /// been issued before, and may be issued again later, but not before
        /// this one is released by a call to the Release() method.
        /// </summary>
        /// <returns>A temporarily unique Id.</returns>
        IdType Fetch();
        /// <summary>
        /// Release is only valid to call for an Id previously aquired from a
        /// call of the Fetch() method, that has not yet been released in the
        /// meantime.
        /// </summary>
        /// <param name="id">A message identifier from a previous call to Fetch().</param>
        void Release(IdType id);
    }
}
