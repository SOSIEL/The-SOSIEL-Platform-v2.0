/// Name: DecisionOption.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik

using System;
using System.Linq;
using SOSIEL.Environments;
using SOSIEL.Helpers;

namespace SOSIEL.Entities
{
    public class DecisionOption : ICloneable<DecisionOption>, IEquatable<DecisionOption>
    {
        public DecisionOptionLayer Layer { get; set; }


        /// <summary>
        /// Set in configuration json only
        /// </summary>
        public int MentalModel { get; set; }

        /// <summary>
        /// Set in configuration json only
        /// </summary>
        public int DecisionOptionsLayer { get; set; }


        public int PositionNumber { get; set; }

        public DecisionOptionAntecedentPart[] Antecedent { get; set; }

        public DecisionOptionConsequent Consequent { get; set; }

        public bool IsModifiable { get; set; }

        public int RequiredParticipants { get; set; }

        public string Scope { get; set; }

        public bool AutoGenerated { get; set; }

        public string Origin { get; set; }

        public bool IsCollectiveAction
        {
            get
            {
                return RequiredParticipants > 1;
            }
        }


        public string Id
        {
            get
            {
                return string.Format("MM{0}-{1}_DO{2}", Layer.Set.PositionNumber, Layer.PositionNumber, PositionNumber);
            }
        }

        /// <summary>
        /// Checks agent variables on antecedent conditions
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public bool IsMatch(IAgent agent)
        {
            return Antecedent.All(a => a.IsMatch(agent));
        }


        /// <summary>
        /// Applies the decision option. Copies consequent value or reference variable value to agent variables
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public TakenAction Apply(IAgent agent)
        {
            dynamic value;

            if (string.IsNullOrEmpty(Consequent.VariableValue) == false)
            {
                value = agent[Consequent.VariableValue];
            }
            else
            {
                value = Consequent.Value;
            }


            if (Consequent.SavePrevious)
            {
                string key = string.Format("{0}_{1}", SosielVariables.PreviousPrefix, Consequent.Param);

                agent[key] = agent[Consequent.Param];

                if (Consequent.CopyToCommon)
                {
                    agent.SetToCommon(string.Format("{0}_{1}_{2}", SosielVariables.AgentPrefix, agent.Id, key), agent[Consequent.Param]);
                }
            }

            if (Consequent.CopyToCommon)
            {
                string key = string.Format("{0}_{1}_{2}", SosielVariables.AgentPrefix, agent.Id, Consequent.Param);

                agent.SetToCommon(key, value);
            }

            agent[Consequent.Param] = value;


            agent.DecisionOptionActivationFreshness[this] = 0;

            return new TakenAction(Id, Consequent.Param, value);
        }

        /// <summary>
        /// Creates shallow object copy
        /// </summary>
        /// <returns></returns>
        public DecisionOption Clone()
        {
            return (DecisionOption)this.MemberwiseClone();
        }



        /// <summary>
        /// Creates decision option copy but replaces antecedent parts and consequent by new values.
        /// </summary>
        /// <param name="old"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public static DecisionOption Renew(DecisionOption old, DecisionOptionAntecedentPart[] newAntecedent, DecisionOptionConsequent newConsequent)
        {
            DecisionOption decisionOption = old.Clone();

            decisionOption.Antecedent = newAntecedent;
            decisionOption.Consequent = newConsequent;

            decisionOption.Origin = old.Id;

            return decisionOption;
        }

        /// <summary>
        /// Compares two decision option objects
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(DecisionOption other)
        {
            //check on reference equality first
            //custom logic for comparing two objects
            return ReferenceEquals(this, other)
                || (other != null
                && Consequent == other.Consequent
                && Antecedent.Length == other.Antecedent.Length
                && Antecedent.All(ant => other.Antecedent.Any(ant2 => ant == ant2))
                && Id == other.Id);
        }

        public override bool Equals(object obj)
        {
            //check on reference equality first
            return base.Equals(obj) || Equals(obj as DecisionOption);
        }

        public override int GetHashCode()
        {
            //disable comparing by hash code
            return 0;
        }

        public static bool operator ==(DecisionOption a, DecisionOption b)
        {
            if (Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(DecisionOption a, DecisionOption b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return Id;
        }
    }
}
