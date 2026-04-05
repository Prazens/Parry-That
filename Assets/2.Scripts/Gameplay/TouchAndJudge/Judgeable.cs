using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Judgeable
{
    private List<Action<Judgeable>> onDestroy = new();
    public Direction noteDirection;
    public AttackType attackType;
    public float arriveBeat;

    private bool _isDestroyed = false;

    public Judgeable(AttackType _attackType, float _arriveBeat, Direction _noteDirection, Action<Judgeable> _onDestroy = null)
    {
        attackType = _attackType;
        arriveBeat = _arriveBeat;
        noteDirection = _noteDirection;
        AddOnDestroy(_onDestroy);
    }

    public void AddOnDestroy(Action<Judgeable> _onDestroy)
    {
        if (_onDestroy != null) onDestroy.Add(_onDestroy);
    }

    public void Destroy()
    {
        if (_isDestroyed) return;
        _isDestroyed = true;

        var callbacks = onDestroy;
        onDestroy = new List<Action<Judgeable>>();

        foreach (var action in callbacks)
            action?.Invoke(this);
    }
}
