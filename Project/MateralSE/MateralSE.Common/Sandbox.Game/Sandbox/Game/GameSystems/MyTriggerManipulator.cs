namespace Sandbox.Game.GameSystems
{
    using Sandbox.Game.Components;
    using Sandbox.Game.SessionComponents;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyTriggerManipulator
    {
        private Vector3D m_currentPosition;
        private readonly List<MyTriggerComponent> m_currentQuery = new List<MyTriggerComponent>();
        private readonly Predicate<MyTriggerComponent> m_triggerEvaluationpPredicate;
        private MyTriggerComponent m_selectedTrigger;

        public MyTriggerManipulator(Predicate<MyTriggerComponent> triggerEvaluationPredicate = null)
        {
            this.m_triggerEvaluationpPredicate = triggerEvaluationPredicate;
        }

        protected virtual void OnPositionChanged(Vector3D oldPosition, Vector3D newPosition)
        {
            this.m_currentQuery.Clear();
            foreach (MyTriggerComponent component in MySessionComponentTriggerSystem.Static.GetIntersectingTriggers(newPosition))
            {
                if (this.m_triggerEvaluationpPredicate == null)
                {
                    this.m_currentQuery.Add(component);
                    continue;
                }
                if (this.m_triggerEvaluationpPredicate(component))
                {
                    this.m_currentQuery.Add(component);
                }
            }
        }

        public void SelectClosest(Vector3D position)
        {
            double maxValue = double.MaxValue;
            if (this.SelectedTrigger != null)
            {
                this.SelectedTrigger.CustomDebugColor = new Color?(Color.Red);
            }
            foreach (MyTriggerComponent component in this.m_currentQuery)
            {
                double num2 = (component.Center - position).LengthSquared();
                if (num2 < maxValue)
                {
                    maxValue = num2;
                    this.SelectedTrigger = component;
                }
            }
            if (Math.Abs((double) (maxValue - double.MaxValue)) < double.Epsilon)
            {
                this.SelectedTrigger = null;
            }
            if (this.SelectedTrigger != null)
            {
                this.SelectedTrigger.CustomDebugColor = new Color?(Color.Yellow);
            }
        }

        public Vector3D CurrentPosition
        {
            get => 
                this.m_currentPosition;
            set
            {
                if (value != this.m_currentPosition)
                {
                    Vector3D currentPosition = this.m_currentPosition;
                    this.m_currentPosition = value;
                    this.OnPositionChanged(currentPosition, this.m_currentPosition);
                }
            }
        }

        public List<MyTriggerComponent> CurrentQuery =>
            this.m_currentQuery;

        public MyTriggerComponent SelectedTrigger
        {
            get => 
                this.m_selectedTrigger;
            set
            {
                if (!ReferenceEquals(this.m_selectedTrigger, value))
                {
                    if (this.m_selectedTrigger != null)
                    {
                        this.m_selectedTrigger.CustomDebugColor = new Color?(Color.Red);
                    }
                    this.m_selectedTrigger = value;
                    if (this.m_selectedTrigger != null)
                    {
                        this.m_selectedTrigger.CustomDebugColor = new Color?(Color.Yellow);
                    }
                }
            }
        }
    }
}

