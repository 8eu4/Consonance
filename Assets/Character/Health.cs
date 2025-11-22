using System;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] protected int _MaxHP;
    protected int _CurrentHP;
    protected event Action<int, int> _OnHealthChanged; // (current, max)
    protected event Action _OnDied;

    void Start()
    {
        _CurrentHP = _MaxHP;
        OnHealthChanged?.Invoke(_CurrentHP, _MaxHP);
    }

    public int MaxHP
    {
        get { return _MaxHP; }
        set { _MaxHP = value; }
    }

    public int CurrentHP
    {
        get { return _CurrentHP; }
        set
        {
            int previousHP = _CurrentHP;
            _CurrentHP = Mathf.Clamp(value, 0, _MaxHP);

            if (previousHP != _CurrentHP)
            {
                OnHealthChanged?.Invoke(_CurrentHP, _MaxHP);

                if (_CurrentHP <= 0)
                {
                    OnDied?.Invoke();
                }
            }
        }
    }

    public Action<int,int> OnHealthChanged
    {
        get { return _OnHealthChanged; }
        set { _OnHealthChanged = value; }
    }

    public Action OnDied
    {
        get { return _OnDied; }
        set { _OnDied = value; }
    }
}
