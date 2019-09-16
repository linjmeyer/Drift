# Drift

**Work in progress** A command line tool and service for monitoring and updating kubernetes cluster states.

## Features

The following features are available but still require some major polishing:

* :white_check_mark: Create multiple jobs
* :white_check_mark: Jobs consist of multiple steps run sequentially
* :white_check_mark: Steps can read, update or delete resources in a Kubernetes cluster
* :white_check_mark: Other helper steps like Webhook and ScriptStep allow for end-user extensibility
* :white_check_mark: Extend steps with scripts using [cs-script](https://github.com/oleg-shilo/cs-script) and Javascript
* :white_check_mark: Run locally as a command line (useful for one time jobs and script development)
* :white_check_mark: Run as a service with scheduled jobs (uses ASP.NET and Hangfire)

## Steps

| Step Name                | Purpose                  | Supports User Scripts    |
|--------------------------|--------------------------|--------------------------|
| Script  | Execute a user script    |:white_check_mark:        |
| Webhook  | Sends an HTTP POST with user defined data   |:white_check_mark:  |
| GetPod  | Gets a pod by name/namespace   |:white_check_mark:  |
| SearchPods  | Find 1+ pods by name (regexp), namespace |:white_check_mark:  |
| DeletePods  | Deletes 1+ pods by names, namespace |:white_check_mark:  |

