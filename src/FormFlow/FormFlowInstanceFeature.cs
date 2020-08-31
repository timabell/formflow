using System;

namespace FormFlow
{
    internal class FormFlowInstanceFeature
    {
        public FormFlowInstanceFeature(FormFlowInstance instance)
        {
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        public FormFlowInstance Instance { get; }
    }
}
