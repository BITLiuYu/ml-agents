using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace MLAgents
{
    /// <summary>
    /// A component that when attached to an Agent will automatically request decisions from it
    /// at regular intervals.
    /// </summary>
    [AddComponentMenu("ML Agents/Decision Requester", (int)MenuGroup.Default)]
    public class DecisionRequester : MonoBehaviour
    {
        /// <summary>
        /// The frequency with which the agent requests a decision. A DecisionPeriod of 5 means
        /// that the Agent will request a decision every 5 Academy steps.
        /// </summary>
        [Range(1, 20)]
        [Tooltip("The frequency with which the agent requests a decision. A DecisionPeriod " +
                 "of 5 means that the Agent will request a decision every 5 Academy steps.")]
        public int DecisionPeriod = 5;

        /// <summary>
        /// A delegate that can be used to customize the logic of when your Agent gets its
        /// <see cref="Agent.RequestDecision"/> and <see cref="Agent.RequestAction"/> called.
        /// If this value is not get, the default logic will use <see cref="DecisionPeriod"/> and
        /// <see cref="TakeActionsBetweenDecisions"/> in order to decide when to call those two
        /// methods on your Agent.
        /// </summary>
        /// <param name="academyStepCount"> The <see cref="Academy.StepCount"/>.</param>
        /// <param name="decisionPeriod">The <see cref="DecisionRequester.DecisionPeriod"/> of the calling
        /// <see cref="DecisionRequester"/>.</param>
        /// <param name="takeActionsBetweenDecisions">The <see cref="DecisionRequester.TakeActionsBetweenDecisions"/>
        /// of the calling <see cref="DecisionRequester"/></param>
        public delegate void MakeRequestsDelegate(int academyStepCount, int decisionPeriod, bool takeActionsBetweenDecisions);

        /// <summary>
        /// The <see cref="MakeRequestsDelegate"/>" that can optionally be used to make calls to
        /// <see cref="Agent.RequestDecision"/> and/or <see cref="Agent.m_RequestAction"/>.
        /// </summary>
        public MakeRequestsDelegate makeRequestsDelegate;

        /// <summary>
        /// Indicates whether or not the agent will take an action during the Academy steps where
        /// it does not request a decision. Has no effect when DecisionPeriod is set to 1.
        /// </summary>
        [Tooltip("Indicates whether or not the agent will take an action during the Academy " +
                 "steps where it does not request a decision. Has no effect when DecisionPeriod " +
                 "is set to 1.")]
        [FormerlySerializedAs("RepeatAction")]
        public bool TakeActionsBetweenDecisions = true;

        Agent m_Agent;

        internal void Awake()
        {
            m_Agent = gameObject.GetComponent<Agent>();
            Debug.Assert(m_Agent != null, "Agent component was not found on this gameObject and is required.");
            Academy.Instance.AgentSetStatus += MakeRequests;
        }

        void OnDestroy()
        {
            if (Academy.IsInitialized)
            {
                Academy.Instance.AgentSetStatus -= MakeRequests;
            }
        }

        /// <summary>
        /// Method that hooks into the Academy in order inform the Agent on whether or not it should request a
        /// decision, and whether or not it should take actions between decisions.
        /// </summary>
        /// <param name="academyStepCount">The current step count of the academy.</param>
        void MakeRequests(int academyStepCount)
        {
            if (makeRequestsDelegate != null)
            {
                makeRequestsDelegate(academyStepCount, DecisionPeriod, TakeActionsBetweenDecisions);
                return;
            }

            if (academyStepCount % DecisionPeriod == 0)
            {
                m_Agent?.RequestDecision();
            }
            if (TakeActionsBetweenDecisions)
            {
                m_Agent?.RequestAction();
            }
        }
    }
}
