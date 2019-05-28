namespace VRage.Game.VisualScripting.Campaign
{
    using System;
    using VRage.Game.ObjectBuilders.Campaign;
    using VRage.Generics;

    public class MyCampaignStateMachine : MySingleStateMachine
    {
        private MyObjectBuilder_CampaignSM m_objectBuilder;

        public void Deserialize(MyObjectBuilder_CampaignSM ob)
        {
            if (this.m_objectBuilder == null)
            {
                this.m_objectBuilder = ob;
                foreach (MyObjectBuilder_CampaignSMNode node in this.m_objectBuilder.Nodes)
                {
                    MyCampaignStateMachineNode node1 = new MyCampaignStateMachineNode(node.Name);
                    node1.SavePath = node.SaveFilePath;
                    MyCampaignStateMachineNode newNode = node1;
                    this.AddNode(newNode);
                }
                foreach (MyObjectBuilder_CampaignSMTransition transition in this.m_objectBuilder.Transitions)
                {
                    this.AddTransition(transition.From, transition.To, null, transition.Name);
                }
            }
        }

        public void ResetToStart()
        {
            foreach (MyCampaignStateMachineNode node in base.m_nodes.Values)
            {
                if ((node != null) && (node.InTransitionCount == 0))
                {
                    base.SetState(node.Name);
                    break;
                }
            }
        }

        public bool Initialized =>
            (this.m_objectBuilder != null);

        public bool Finished =>
            ((MyCampaignStateMachineNode) base.CurrentNode).Finished;
    }
}

