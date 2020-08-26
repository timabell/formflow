using System;

namespace FormFlow
{
    public class FormFlowInstanceFeature
    {
        public FormFlowInstanceFeature(Instance instance)
        {
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        public Instance Instance { get; }
    }
}
