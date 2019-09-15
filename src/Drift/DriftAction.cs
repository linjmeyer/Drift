using System.Collections.Generic;
using Drift;
using Drift.Steps;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class DriftAction
    {
        [JsonIgnore]
        public IDriftStep[] Steps { get; set; }

        [JsonProperty(nameof(Steps))]
        public JArray JArraySteps
        {
            get => null;
            set
            {
                ParseActual(value);
            }
        }
        public string Name { get; set; }

        private void ParseActual(JArray raw)
        {
            var steps = new List<IDriftStep>();
            foreach(var item in raw.Children())
            {
                // Serialize generic step to access the Type string set by user
                var genericStep = item.ToObject<TempInternalDriftStep>();
                // Get real type based on Type value set by user
                var assembly = typeof(TempInternalDriftStep).Assembly;
                var dotNetTypeString = $"{nameof(Drift)}.{nameof(Drift.Steps)}.{genericStep.Type}Step";
                var dotNetType = assembly.GetType(dotNetTypeString);
                // Serialize to user requested type
                var concreteStep = item.ToObject(dotNetType);
                // Convert back to interface and pass to list
                steps.Add((IDriftStep) concreteStep);
            }
    
            Steps = steps.ToArray();
        }

        /// <summary>
        /// Runs all steps within the action
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public bool Run(ILogger<DriftClient> logger)
        {
            var previousContexts = new List<IDriftStep>();
            dynamic previousBag = null;
            for (var i = 0; i < Steps.Length; i++)
            {
                logger?.LogInformation($"Starting step: {i}");
                var step = Steps[i];
                // Save previous contexts to this step
                step.PreviousContexts.AddRange(previousContexts);
                // Save previous Bag (user data store)
                if (previousBag != null)
                {
                    step.Bag = previousBag;
                    logger?.LogDebug($"Passing bag from previous step to step {i}.  Contents: {step.Bag}");
                }
                
                logger?.LogDebug("Starting load step");
                step.Load();

                // Run the step and get the result
                var runResult = step.Run();
                logger?.LogInformation($"Result of {step.Type}: {runResult} {(runResult ? "Will run user script" : "Will not run user script")}");
                if(!runResult) return false; // Stop loop if this step is false
                
                // Run the user script and get the result
                var scriptResult = step.RunUserScript<bool?>(typeString: "bool?");
                if(scriptResult.HasValue) 
                {
                    logger?.LogInformation($"Result of {step.Type} User Script: {scriptResult}");
                    if(!scriptResult.Value)
                    {
                        return false; // user returned false, stop action
                    }
                }
                else
                {
                    logger?.LogInformation($"No user script found");
                }

                // save this step and bag for next step
                previousContexts.Add(step);
                previousBag = step.Bag;
            }

            return true; // If everything finished the steps all ran successfully 
        }
    }