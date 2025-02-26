// <auto-generated>
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Azure.Management.Network.Models
{
    using Microsoft.Rest;
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Define match conditions.
    /// </summary>
    public partial class MatchCondition
    {
        /// <summary>
        /// Initializes a new instance of the MatchCondition class.
        /// </summary>
        public MatchCondition()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the MatchCondition class.
        /// </summary>
        /// <param name="matchVariables">List of match variables.</param>
        /// <param name="operatorProperty">Describes operator to be matched.
        /// Possible values include: 'IPMatch', 'Equal', 'Contains',
        /// 'LessThan', 'GreaterThan', 'LessThanOrEqual', 'GreaterThanOrEqual',
        /// 'BeginsWith', 'EndsWith', 'Regex', 'GeoMatch'</param>
        /// <param name="matchValues">Match value.</param>
        /// <param name="negationConditon">Describes if this is negate
        /// condition or not.</param>
        /// <param name="transforms">List of transforms.</param>
        public MatchCondition(IList<MatchVariable> matchVariables, string operatorProperty, IList<string> matchValues, bool? negationConditon = default(bool?), IList<string> transforms = default(IList<string>))
        {
            MatchVariables = matchVariables;
            OperatorProperty = operatorProperty;
            NegationConditon = negationConditon;
            MatchValues = matchValues;
            Transforms = transforms;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets list of match variables.
        /// </summary>
        [JsonProperty(PropertyName = "matchVariables")]
        public IList<MatchVariable> MatchVariables { get; set; }

        /// <summary>
        /// Gets or sets describes operator to be matched. Possible values
        /// include: 'IPMatch', 'Equal', 'Contains', 'LessThan', 'GreaterThan',
        /// 'LessThanOrEqual', 'GreaterThanOrEqual', 'BeginsWith', 'EndsWith',
        /// 'Regex', 'GeoMatch'
        /// </summary>
        [JsonProperty(PropertyName = "operator")]
        public string OperatorProperty { get; set; }

        /// <summary>
        /// Gets or sets describes if this is negate condition or not.
        /// </summary>
        [JsonProperty(PropertyName = "negationConditon")]
        public bool? NegationConditon { get; set; }

        /// <summary>
        /// Gets or sets match value.
        /// </summary>
        [JsonProperty(PropertyName = "matchValues")]
        public IList<string> MatchValues { get; set; }

        /// <summary>
        /// Gets or sets list of transforms.
        /// </summary>
        [JsonProperty(PropertyName = "transforms")]
        public IList<string> Transforms { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (MatchVariables == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "MatchVariables");
            }
            if (OperatorProperty == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "OperatorProperty");
            }
            if (MatchValues == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "MatchValues");
            }
            if (MatchVariables != null)
            {
                foreach (var element in MatchVariables)
                {
                    if (element != null)
                    {
                        element.Validate();
                    }
                }
            }
        }
    }
}
