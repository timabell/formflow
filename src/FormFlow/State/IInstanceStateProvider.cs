using System.Threading.Tasks;

namespace FormFlow.State
{
    public interface IInstanceStateProvider
    {
        Task<Instance> GetInstance(string instanceId);
    }
}
