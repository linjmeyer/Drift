using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using k8s;

public enum KubernetesActionTypes
{
    Create,
    Read,
    Update,
    Delete
}

public class KubernetesTask
{
    private IKubernetes _client;
    private List<Func<bool>> _steps = new List<Func<bool>>();

    public Dictionary<string, Tuple<Type, object>> _state = new Dictionary<string, Tuple<Type, object>>();

    public KubernetesTask(IKubernetes client)
    {
        _client = client;
    }

    public void Execute()
    {
        foreach (var step in _steps)
        {
            var result = step.Invoke();
            if (!result)
            {
                break; // Action ends when they return false
            }
        }
    }

    public void AddState(string key, object item)
    {
        _state[key] = new Tuple<Type, object>(item.GetType(), item);
    }

    public Tuple<Type, object> GetState(string key)
    {
        if (_state.Keys.Any(k => k == key))
        {
            return _state[key];
        }

        throw new Exception("No key fool");
    }

    public T GetState<T>(string key)
    {
        var value = GetState(key);
        return (T) value.Item2;
    }

    public KubernetesTask GetPod(string name, string @namespace = "default", Func<string, bool> evaluate = null)
    {
        _steps.Add(() =>
            {
                var pod = _client.ReadNamespacedPod(name, @namespace);
                return SafeEvaluator(pod.Metadata.Name, evaluate);
            });

        return this;
    }

    public KubernetesTask DeletePod(string name, string @namespace)
    {


        return this;
    }

    private bool SafeEvaluator(string name, Func<string, bool> action = null)
    {
        if (action != null)
        {
            return action.Invoke(name);
        }
        return true;
    }
}