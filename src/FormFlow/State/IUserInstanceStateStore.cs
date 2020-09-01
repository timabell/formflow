namespace FormFlow.State
{
    public interface IUserInstanceStateStore
    {
        void SetState(string key, byte[] data);
        bool TryGetState(string key, out byte[] data);
    }
}
