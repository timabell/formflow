using System;

namespace FormFlow
{
    public class FormFlowInstanceFeature
    {
        public FormFlowInstanceFeature(FormFlowInstance instance)
        {
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        public FormFlowInstance Instance { get; }
    }
}
